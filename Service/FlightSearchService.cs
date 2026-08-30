using FlightBookingCS.Service.Interface;
using FlightBookingCS.ViewModel;
using FlightBookingCS.ViewModel.ApiModels;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace FlightBookingCS.Service;

public class FlightSearchService : IFlightSearchService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FlightSearchService> _logger;
    private readonly ICacheService _cacheService;
    private readonly IPricingService _pricingService;
    private readonly IHttpContextAccessor _httpContextAccessor;


    public FlightSearchService(
        HttpClient httpClient,
        ICacheService cacheService,
        IPricingService pricingService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<FlightSearchService> logger)
    {
        _httpClient = httpClient;
        _cacheService = cacheService;
        _pricingService = pricingService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<FlightResultsViewModel> SearchFlightsAsync(FlightSearchRequest request)
    {
        try
        {
            var apiUrl = "https://uthaotrip.com/api/air/UnauthorizeSearchAir";
            _logger.LogInformation("Searching flights from: {ApiUrl}", apiUrl);

            // Try to get from cache first if available
            var cachedData = await _cacheService.GetAsync(request.GenerateCacheKey());
            if (cachedData?.ApiResponse != null && cachedData.ApiResponse.Success)
            {
                _logger.LogInformation("Cache hit for request");
                var cachedViewModel = MapToViewModel(cachedData.ApiResponse);
                cachedViewModel.IGXKey = cachedData.ApiResponse.Payload?.FirstOrDefault()?.IGXKey;
                
                var userId = _httpContextAccessor.HttpContext?.User
                    .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    _logger.LogInformation("User logged in - Applying pricing rules for user: {UserId}", userId);
                    cachedViewModel = await _pricingService.ApplyPricingToFlightsAsync(cachedViewModel, userId);
                }
                
                return cachedViewModel;
            }

            // Serialize request
            var requestBody = JsonSerializer.Serialize(request);
            _logger.LogDebug("Request Body: {RequestBody}", requestBody);

            // Create content
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            // Send request
            var response = await _httpClient.PostAsync(apiUrl, content);

            // Check if response is Null
            if (response == null)
            {
                _logger.LogError("API Response Is Null");
                return CreateErrorViewModel("API returned Null Value");
            }

            // Check if response Provided Success Status Code 
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("API call failed with status code: {StatusCode}", response.StatusCode);
                return CreateErrorViewModel($"API returned {response.StatusCode} - {response.ReasonPhrase}");
            }

            // Read as byte array
            var bytes = await response.Content.ReadAsByteArrayAsync();

            // Check if response is empty
            if (bytes == null || bytes.Length == 0)
            {
                _logger.LogWarning("API returned empty response");
                return CreateErrorViewModel("API returned empty response");
            }

            // Check if GZIP compressed (starts with 0x1F 0x8B)
            bool isGzip = bytes.Length > 2 && bytes[0] == 0x1F && bytes[1] == 0x8B;

            string jsonString;

            // Decompress if compressed
            if (isGzip)
            {
                _logger.LogDebug("Response is GZIP compressed. Decompressing...");
                jsonString = DecompressGzip(bytes);
            }
            else
            {
                // Not compressed, convert directly to string
                jsonString = Encoding.UTF8.GetString(bytes);
            }

            // Parse response
            var apiResponse = ParseJsonResponse(jsonString);

            /// Check if API call was successful
            if (!apiResponse.Success)
            {
                _logger.LogWarning("API returned failure: {Message}", apiResponse.Message);
                return CreateErrorViewModel(apiResponse.Message ?? "API returned failure");
            }

            // Extract IGXKey
            string? igxKey = apiResponse.Payload?.FirstOrDefault()?.IGXKey;

            // If IGX Key not null, cache the response
            if (!string.IsNullOrEmpty(igxKey))
            {
                await _cacheService.StoreAsyc(igxKey, apiResponse, request);
                _logger.LogInformation("Cached flight data with IGXKey: {IGXKey}", igxKey);
            }

            // Map to ViewModel and include the IGXKey
            var viewModel = MapToViewModel(apiResponse);
            viewModel.IGXKey = igxKey;

            var currentUserId = _httpContextAccessor.HttpContext?.User
                .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(currentUserId))
            {
                _logger.LogInformation("User logged in - Applying pricing rules for user: {UserId}", currentUserId);
                viewModel = await _pricingService.ApplyPricingToFlightsAsync(viewModel, currentUserId);
            }
            else
            {
                _logger.LogDebug("User not logged in - No pricing rules applied");
            }

            // Map to ViewModel
            return viewModel;
        }
        catch (HttpRequestException httpEx)  // Network error
        {
            _logger.LogError(httpEx, "HTTP request error while searching flights");
            return CreateErrorViewModel($"Network error: {httpEx.Message}");
        }
        catch (Exception ex)  // Unexpected error
        {
            _logger.LogError(ex, "Unexpected error while searching flights");
            return CreateErrorViewModel($"Unexpected error: {ex.Message}");
        }
    }

    // Decompress GZIP
    private string DecompressGzip(byte[] compressedData)
    {
        try
        {
            // Wrap in MemoryStream
            using var inputStream = new MemoryStream(compressedData);
            // Wrap in GZipStream
            using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
            // Wrap in MemoryStream
            using var outputStream = new MemoryStream();

            // Copy to outputStream
            gzipStream.CopyTo(outputStream);

            // Get decompressed bytes
            var decompressedBytes = outputStream.ToArray();

            // Return
            return Encoding.UTF8.GetString(decompressedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decompress GZIP data");
            throw;
        }
    }


    // Parse JSON response
    private FlightSearchApiResponse ParseJsonResponse(string jsonString)
    {
        try
        {
            // Configure JSON options
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            // Deserialize
            var apiResponse = JsonSerializer.Deserialize<FlightSearchApiResponse>(jsonString, options);

            /// Check if deserialization was successful
            if (apiResponse == null)
            {
                _logger.LogWarning("Deserialized API response is null");
                return new FlightSearchApiResponse
                {
                    Success = false,
                    Message = "Failed to parse API response - null result",
                    Payload = new List<FlightSearchApiPayload>()
                };
            }

            return apiResponse;  // Return deserialized response
        }
        catch (JsonException jsonEx)  // JSON parsing error
        {
            _logger.LogError(jsonEx, "JSON parsing error. Response preview: {Response}",
                jsonString.Length > 200 ? jsonString.Substring(0, 200) + "..." : jsonString);
            return new FlightSearchApiResponse
            {
                Success = false,
                Message = $"JSON parsing error: {jsonEx.Message}",
                Payload = new List<FlightSearchApiPayload>()
            };
        }
    }

    // Map to ViewModel
    private FlightResultsViewModel MapToViewModel(FlightSearchApiResponse apiResponse)
    {
        // Check if API call was successful
        if (!apiResponse.Success || apiResponse.Payload == null || !apiResponse.Payload.Any())
        {
            return new FlightResultsViewModel
            {
                Flights = new List<FlightResultItem>(),
                HasMore = false,
                TotalCount = 0
            };
        }

        // Map to ViewModel
        return new FlightResultsViewModel
        {
            Flights = apiResponse.Payload.Select(p => new FlightResultItem
            {
                Id = p.AirPricingSolution_Key ?? Guid.NewGuid().ToString(),
                IsRefundable = p.IsRefundable,
                IsBookable = p.IsBookable,
                TripType = p.TripType,
                PassengerType = p.PassengerType,
                PlatingCarrierName = p.PlatingCarrierName,
                FareType = p.FareType,

                // ONWARDS
                Onwards = p.Onwards.Select(s => new FlightSegmentInfo
                {
                    CarrierName = s.CarrierName ?? string.Empty,
                    Carrier = s.Carrier ?? string.Empty,
                    FlightNumber = s.FlightNumber ?? string.Empty,
                    OperatingCarrierName = s.OperatingCarrierName,
                    Origin = s.Origin ?? string.Empty,
                    Destination = s.Destination ?? string.Empty,
                    DepartureTime = s.DepartureTime ?? string.Empty,
                    ArrivalTime = s.ArrivalTime ?? string.Empty,
                    TravelDuration = s.TravelDuration,
                    AirBaggageAllowance = s.AirBaggageAllowance,
                    Equipment = s.Equipment,
                    BookingCode = s.BookingCode,
                    FareBasis = s.FareBasis,
                    Currency = s.Currency
                }).ToList(),

                // TOTAL TRAVEL TIMES
                TotalTravelTimes = p.TotalTravelTimes.Select(t => new TotalTravelTimeInfo
                {
                    TotalTravelDuration = t.TotalTravelDuration,
                    NoOfStop = t.NoOfStop
                }).ToList(),

                // FARE BREAKDOWN
                FareBreakdown = p.FareBreakdown.Select(f => new FareBreakdownInfo
                {
                    TotalFare = f.TotalFare,
                    BaseFare = f.BaseFare,
                    TotalTax = f.TotalTax,
                    ApiDiscount = f.ApiDiscount,
                    Fees = f.Fees,
                    PassengerType = f.PassengerType ?? p.PassengerType,
                    TaxesBreakdown = f.TaxesBreakdown.Select(t => new TaxBreakdownInfo
                    {
                        Category = t.Category,
                        Amount = t.Amount
                    }).ToList()
                }).ToList(),

                // BRANDED FARE INFO
                BrandedFareInfoes = p.BrandedFareInfoes.Select(b => new BrandedFareInfo
                {
                    BrandedFareInfoDetail = b.BrandedFareInfoDetail != null ? new BrandedFareInfoDetail
                    {
                        CabinBaggage = MapFareDetailItem(b.BrandedFareInfoDetail.CabinBaggage),
                        CheckedBaggage = MapFareDetailItem(b.BrandedFareInfoDetail.CheckedBaggage),
                        PurchaseBaggage = MapFareDetailItem(b.BrandedFareInfoDetail.PurchaseBaggage),
                        Change = MapFareDetailItem(b.BrandedFareInfoDetail.Change),
                        Cancellation = MapFareDetailItem(b.BrandedFareInfoDetail.Cancellation),
                        Meal = MapFareDetailItem(b.BrandedFareInfoDetail.Meal),
                        SeatSelection = MapFareDetailItem(b.BrandedFareInfoDetail.SeatSelection),
                        LoungeAccess = MapFareDetailItem(b.BrandedFareInfoDetail.LoungeAccess)
                    } : null
                }).ToList()
            }).ToList(),

            HasMore = !apiResponse.IsComplete,
            TotalCount = apiResponse.TotalVolume
        };
    }

    // Map to ViewModel
    private FareDetailItem? MapFareDetailItem(ApiFareDetailItem? source)
    {
        // Check if source is null
        if (source == null) return null;

        // Map to ViewModel
        return new FareDetailItem
        {
            Text = source.Text,
            OtherText = source.OtherText
        };
    }

    // Create error ViewModel
    private FlightResultsViewModel CreateErrorViewModel(string message)
    {
        _logger.LogError("Flight search error: {Message}", message);

        return new FlightResultsViewModel
        {
            Flights = new List<FlightResultItem>(),
            HasMore = false,
            TotalCount = 0
        };
    }
}