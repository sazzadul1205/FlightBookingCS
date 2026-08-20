using FlightBookingCS.Service.Interface;
using FlightBookingCS.ViewModel;
using System.Text;
using System.Text.Json;
using System.IO.Compression;

namespace FlightBookingCS.Service;

public class AirlineService : IAirlineService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AirlineService> _logger;

    public AirlineService(HttpClient httpClient, ILogger<AirlineService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AirlineApiResponse> GetAirlineAsync()
    {
        try
        {
            var apiUrl = "https://uthaotrip.com/api/api/GetAirLines";
            _logger.LogInformation("Fetching airlines from: {ApiUrl}", apiUrl);

            // 
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "FlightBookingApp/1.0");
            _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

            var response = await _httpClient.GetAsync(apiUrl);

            if (response == null)
            {
                _logger.LogError("API Response Is Null");
                return CreateErrorResponse("API returned Null Value");
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("API call failed with status code: {StatusCode}", response.StatusCode);
                return CreateErrorResponse($"API returned {response.StatusCode} - {response.ReasonPhrase}");
            }

            // Read as byte array first (same as debug method)
            var bytes = await response.Content.ReadAsByteArrayAsync();

            if (bytes == null || bytes.Length == 0)
            {
                _logger.LogWarning("API returned empty response");
                return CreateErrorResponse("API returned empty response");
            }

            // Check if it's GZIP compressed (starts with 0x1F 0x8B) - same as debug method
            bool isGzip = bytes.Length > 2 && bytes[0] == 0x1F && bytes[1] == 0x8B;

            string jsonString;
            if (isGzip)
            {
                _logger.LogInformation("Response is GZIP compressed. Decompressing...");
                jsonString = DecompressGzip(bytes);
            }
            else
            {
                // Not compressed, convert directly
                jsonString = Encoding.UTF8.GetString(bytes);
            }

            // Check if response is empty after decompression
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                _logger.LogWarning("API returned empty response after decompression");
                return CreateErrorResponse("API returned empty response");
            }

            // Log first 200 characters for debugging
            var preview = jsonString.Length > 200 ? jsonString.Substring(0, 200) + "..." : jsonString;
            _logger.LogInformation("Response preview: {Preview}", preview);

            // Parse JSON
            return ParseJsonResponse(jsonString);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP request error while fetching airlines");
            return CreateErrorResponse($"Network error: {httpEx.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching airlines");
            return CreateErrorResponse($"Unexpected error: {ex.Message}");
        }
    }

    private string DecompressGzip(byte[] compressedData)
    {
        try
        {
            using var inputStream = new MemoryStream(compressedData);
            using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
            using var outputStream = new MemoryStream();

            gzipStream.CopyTo(outputStream);
            var decompressedBytes = outputStream.ToArray();

            return Encoding.UTF8.GetString(decompressedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decompress GZIP data");
            throw;
        }
    }

    private AirlineApiResponse ParseJsonResponse(string jsonString)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            var apiResponse = JsonSerializer.Deserialize<AirlineApiResponse>(jsonString, options);

            if (apiResponse == null)
            {
                _logger.LogWarning("Deserialized API response is null");
                return CreateErrorResponse("Failed to parse API response - null result");
            }

            // Check if the API call was successful
            if (!apiResponse.Success)
            {
                _logger.LogWarning("API returned failure: {Message}", apiResponse.Message);
                return new AirlineApiResponse
                {
                    Success = false,
                    Message = apiResponse.Message ?? "API returned failure",
                    Payload = new List<AirlinePayload>()  // Return empty Payload
                };
            }

            // Check if Payload is null
            if (apiResponse.Payload == null)
            {
                _logger.LogWarning("API response Payload is null");
                return new AirlineApiResponse
                {
                    Success = true,
                    Message = apiResponse.Message ?? "No airlines found",
                    Payload = new List<AirlinePayload>()
                };
            }

            // Return the response with Payload
            return new AirlineApiResponse
            {
                Success = apiResponse.Success,
                Message = apiResponse.Message ?? "Data found",
                Payload = apiResponse.Payload ?? new List<AirlinePayload>()
            };
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON parsing error. Response preview: {Response}",
                jsonString.Length > 200 ? jsonString.Substring(0, 200) + "..." : jsonString);
            return CreateErrorResponse($"JSON parsing error: {jsonEx.Message}");
        }
    }

    private AirlineApiResponse CreateErrorResponse(string message)
    {
        return new AirlineApiResponse
        {
            Success = false,
            Message = message,
            Payload = new List<AirlinePayload>()
        };
    }
}