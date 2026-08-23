
using FlightBookingCS.Service.Interface;
using FlightBookingCS.ViewModel;
using System.Text.Json;

namespace FlightBookingCS.Service
{
    public class GetCitiesService : IGetCitiesService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GetCitiesService> _logger;

        public GetCitiesService(
            HttpClient httpClient,
            ILogger<GetCitiesService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<CitiesViewModal> GetCitiesAsync(string search)
        {
            try
            {
                if (search == null)
                {
                    _logger.LogError("The Search is Empty");
                    return CreateErrorResponse("Search Has Not been Provided");
                }

                var apiUrl = $"https://uthaotrip.com/api/Auto/GetCities/?input={search}";

                // 
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "FlightBookingApp/1.0");
                _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

                var response = await _httpClient.GetAsync(apiUrl);

                // Check if response is successful
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API call failed with status code: {StatusCode}", response.StatusCode);
                    return CreateErrorResponse($"API returned {response.StatusCode}");
                }

                var jsonString = await response.Content.ReadAsStringAsync();

                // 
                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    _logger.LogWarning("API Returned an Empty Response");
                    return CreateErrorResponse("API Returned Empty Response");
                }

                return ParseJsonResponse(jsonString);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error fetching cities");
                return CreateErrorResponse($"Network error: {httpEx.Message}");
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON parsing error");
                return CreateErrorResponse($"Data parsing error: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching cities");
                return CreateErrorResponse($"Unexpected error: {ex.Message}");
            }
        }

        private CitiesViewModal ParseJsonResponse(string jsonString)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                var cities = JsonSerializer.Deserialize<List<GetCitiesApiResponse>>(jsonString, options);

                if (cities == null)
                {
                    _logger.LogWarning("Deserialized API response is null");
                    return CreateErrorResponse("Failed To Parse Json response");
                }

                return new CitiesViewModal
                {
                    Success = true,
                    Message = $"Successfully retrieved {cities.Count} cities",
                    Cities = cities,
                };
            }
            catch (JsonException jsonEx)
            {
                _logger.LogWarning(jsonEx, "JSON Parsing Error in ParseJsonResponse");
                return CreateErrorResponse($"JSON parsing error: {jsonEx.Message}");
            }
        }

        private CitiesViewModal CreateErrorResponse(string message)
        {
            return new CitiesViewModal
            {
                Success = false,
                Message = message,
                Cities = new List<GetCitiesApiResponse>()
            };
        }
    }
}
