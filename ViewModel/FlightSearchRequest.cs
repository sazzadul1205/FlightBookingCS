namespace FlightBookingCS.ViewModel
{
    public class FlightSearchRequest
    {
        public int JourneyType { get; set; }
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string DepartureDate { get; set; } = string.Empty;
        public string? ReturnDate { get; set; }
        public string ClassType { get; set; } = string.Empty;
        public int NoOfAdult {  get; set; }
        public int NoOfChildren { get; set; }
        public int NoOfInfant { get; set; }
        public bool IsSpecialTexRedumtion { get; set; }
        public bool IsFlexSearch { get; set; }
        public string? Flex { get; set; }
        public int[] ChildrenAges { get; set; } = [];

        /// <summary>
        /// Generates a cache key based on the search parameters.
        /// </summary>
        public string GenerateCacheKey()
        {
            return $"{Origin}_{Destination}_{DepartureDate}_{ReturnDate ?? "NA"}_{JourneyType}_{NoOfAdult}_{NoOfChildren}_{NoOfInfant}_{ClassType}";
        }
    }
}
