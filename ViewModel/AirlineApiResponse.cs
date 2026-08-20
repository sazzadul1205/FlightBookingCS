namespace FlightBookingCS.ViewModel;

public class AirlineApiResponse
{
    public string RequestTime { get; set; } = string.Empty;
    public string ResponseTime { get; set; } = string.Empty;
    public string? RequestURL { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<AirlinePayload> Airlines { get; set; } = new();
    public string PayloadType { get; set; } = string.Empty;
    public string ApiRequestTime { get; set; } = string.Empty;
    public string ApiResponseTime { get; set; } = string.Empty;
    public string MainApiRequestTime { get; set; } = string.Empty;
    public string MainApiResponseTime { get; set; } = string.Empty;
    public string Time1 { get; set; } = string.Empty;
    public string Time2 { get; set; } = string.Empty;
    public string Time3 { get; set; } = string.Empty;
    public string Time4 { get; set; } = string.Empty;
    public string Time5 { get; set; } = string.Empty;
    public int TotalVolume { get; set; }
    public int Take { get; set; }
    public bool IsComplete { get; set; }
}

public class AirlinePayload
{
    public string Code { get; set; } = string.Empty;
    public string AriLineName { get; set; } = string.Empty;
    public int ID { get; set; }
}

public class AirlineViewModel
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<AirlinePayload> Airlines { get; set; } = new();
}