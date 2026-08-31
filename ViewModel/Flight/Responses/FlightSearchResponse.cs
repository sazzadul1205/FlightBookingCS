namespace FlightBookingCS.ViewModel.Flight.Responses;

public class FlightSearchApiResponse
{
    public string RequestTime { get; set; } = string.Empty;
    public string ResponseTime { get; set; } = string.Empty;
    public string? RequestURL { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<FlightSearchApiPayload> Payload { get; set; } = new();
    public string? PayloadType { get; set; }
    public string? ApiRequestTime { get; set; }
    public string? ApiResponseTime { get; set; }
    public string? MainApiRequestTime { get; set; }
    public string? MainApiResponseTime { get; set; }
    public string? Time1 { get; set; }
    public string? Time2 { get; set; }
    public string? Time3 { get; set; }
    public string? Time4 { get; set; }
    public string? Time5 { get; set; }
    public int TotalVolume { get; set; }
    public int Take { get; set; }
    public bool IsComplete { get; set; }
}

public class FlightSearchApiPayload
{
    public List<ApiTotalTravelTime> TotalTravelTimes { get; set; } = new();
    public List<ApiFareBreakdown> FareBreakdown { get; set; } = new();
    public List<ApiFlightSegment> Onwards { get; set; } = new();
    public List<ApiFlightSegment> Returns { get; set; } = new();

    // Pricing
    public decimal TotalDiscount { get; set; }
    public decimal TotalServiceCharge { get; set; }
    public decimal ServiceChargeValue { get; set; }
    public string? ServiceChargeType { get; set; }
    public decimal TotalAIT { get; set; }
    public decimal AgentExtraFair { get; set; }
    public decimal SubAgentExtraFair { get; set; }
    public decimal TotalMarkup { get; set; }
    public decimal? CampaignDiscount { get; set; }
    public string? CampaignBank { get; set; }
    public decimal? CampaignMaxDiscount { get; set; }
    public decimal DepositTotalPrice { get; set; }
    public decimal OnlineTotalPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal BasePrice { get; set; }
    public decimal Fees { get; set; }
    public decimal AIT { get; set; }
    public decimal TotalTax { get; set; }
    public decimal ApiTotalDiscount { get; set; }

    // Passenger Info
    public string? APICurrencyType { get; set; }
    public string? PassengerType { get; set; }
    public int Adults { get; set; }
    public int Childs { get; set; }
    public int Infants { get; set; }

    // Airline Info
    public string? PlatingCarrier { get; set; }
    public string? PlatingCarrierName { get; set; }
    public string? GDSCode { get; set; }

    // Booking Status
    public bool IsRefundable { get; set; }
    public bool IsBookable { get; set; }
    public bool IsTaxBreakdownAvailable { get; set; }
    public bool IsBrandedFare { get; set; }
    public bool IsPassportAndVisaRequired { get; set; }
    public bool IsReIssueEligible { get; set; }
    public bool IsClassAvailiblty { get; set; }
    public bool IsPartialPayable { get; set; }
    public bool HasOwnID { get; set; }

    // Dates & Times
    public string LatestTicketingTime { get; set; } = string.Empty;
    public string? LastPaymentDate { get; set; }

    // Keys & References
    public string? AirPricingSolution_Key { get; set; }
    public string? SegmentCode { get; set; }
    public string? PromoCode { get; set; }
    public string? OwnIDRef { get; set; }
    public string? IGXKey { get; set; }
    public string? AdditionalText { get; set; }
    public string? EncryptRules { get; set; }
    public string? AirPriceRetrieveLogUrl { get; set; }
    public string? MetaKey { get; set; }
    public string? ApiName { get; set; }
    public decimal MinPayablePercent { get; set; }
    public string? FareType { get; set; }
    public string? TripType { get; set; }

    public object? MatchingData { get; set; }
    public object? MatchingResults { get; set; }
    public bool IsToggleBrandedFare { get; set; }
    public object? ChangePenalties { get; set; }
    public object? CancelPenalties { get; set; }
    public List<ApiBrandedFareInfo> BrandedFareInfoes { get; set; } = new();
}

public class ApiTotalTravelTime
{
    public string? TotalLayoverTime { get; set; }
    public string? TravelType { get; set; }
    public string? TotalTravelDuration { get; set; }
    public int NoOfStop { get; set; }
    public int SegmentCount { get; set; }
}

public class ApiFareBreakdown
{
    public List<ApiTaxBreakdown> TaxesBreakdown { get; set; } = new();
    public decimal Discount { get; set; }
    public decimal MarkupAmount { get; set; }
    public decimal ServiceCharge { get; set; }
    public object? ReIssueCharge { get; set; }
    public object? IsMaxChildAge { get; set; }
    public decimal BimaFyPrice { get; set; }
    public decimal DocTimePrice { get; set; }
    public decimal AdditionalServiceCharge { get; set; }
    public string? PassengerType { get; set; }
    public decimal TotalFare { get; set; }
    public int NoOfPassenger { get; set; }
    public decimal BaseFare { get; set; }
    public decimal TotalTax { get; set; }
    public decimal Surcharges { get; set; }
    public decimal ApiDiscount { get; set; }
    public decimal Fees { get; set; }
    public decimal AIT { get; set; }
}

public class ApiTaxBreakdown
{
    public string? Category { get; set; }
    public decimal Amount { get; set; }
}

public class ApiFlightSegment
{
    public string? OriginAirPortName { get; set; }
    public string? DestinationAirPortName { get; set; }
    public string? GridRatio { get; set; }
    public string? LayoverGridRatio { get; set; }
    public string? LayoverTime { get; set; }
    public string? AirSegment_Key { get; set; }
    public int Group { get; set; }
    public string? Carrier { get; set; }
    public string? CarrierName { get; set; }
    public string? FlightNumber { get; set; }
    public string? Origin { get; set; }
    public string? Destination { get; set; }
    public string? DepartureTime { get; set; }
    public string? ArrivalTime { get; set; }
    public string? TravelDuration { get; set; }
    public string? Distance { get; set; }
    public string? AvailabilitySource { get; set; }
    public string? OperatingCarrier { get; set; }
    public string? OperatingCarrierName { get; set; }
    public string? OperatingFlightNumber { get; set; }
    public string? OriginTerminal { get; set; }
    public string? DestinationTerminal { get; set; }
    public string? BookingCode { get; set; }
    public string? BookingCount { get; set; }
    public string? CabinClass { get; set; }
    public string? FareBasis { get; set; }
    public string? Currency { get; set; }
    public string? AirBaggageAllowance { get; set; }
    public string? Equipment { get; set; }
    public object? HiddenSegment { get; set; }
    public List<ApiBaggageDetail> BaggageDetails { get; set; } = new();
}

public class ApiBaggageDetail
{
    public string? PassengerType { get; set; }
    public string? BaggageType { get; set; }
    public string? Quantity { get; set; }
}

public class ApiBrandedFareInfo
{
    public decimal TotalDiscount { get; set; }
    public decimal TotalServiceCharge { get; set; }
    public decimal TotalAIT { get; set; }
    public decimal AgentExtraFair { get; set; }
    public decimal SubAgentExtraFair { get; set; }
    public decimal TotalMarkup { get; set; }
    public List<ApiFareBreakdown> FareBreakdowns { get; set; } = new();
    public string? FareBrand { get; set; }
    public ApiBrandedFareInfoDetail? BrandedFareInfoDetail { get; set; }
    public decimal TotalFare { get; set; }
    public decimal BaseFare { get; set; }
    public decimal TotalTax { get; set; }
    public decimal ApiDiscount { get; set; }
    public decimal Fees { get; set; }
    public string? SegmentCode { get; set; }
    public string? Remarks { get; set; }
    public bool IsRefundable { get; set; }
}

public class ApiBrandedFareInfoDetail
{
    public ApiFareDetailItem? CabinBaggage { get; set; }
    public ApiFareDetailItem? CheckedBaggage { get; set; }
    public ApiFareDetailItem? PurchaseBaggage { get; set; }
    public ApiFareDetailItem? Change { get; set; }
    public ApiFareDetailItem? Cancellation { get; set; }
    public ApiFareDetailItem? Meal { get; set; }
    public ApiFareDetailItem? SeatSelection { get; set; }
    public ApiFareDetailItem? LoungeAccess { get; set; }
}

public class ApiFareDetailItem
{
    public string? Text { get; set; }
    public string? OtherText { get; set; }
    public bool IsAvailableContent { get; set; }
}