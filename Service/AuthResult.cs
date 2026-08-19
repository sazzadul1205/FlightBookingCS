namespace FlightBookingCS.Service
{
    public class AuthResult
    {
        public bool Succeeded { get; set; }
        public string? Token { get; set; }
        public List<string> Errors { get; set; } = new();

        public static AuthResult Success(string token) => new() { Succeeded = true, Token = token };
        public static AuthResult Fail(params string[] errors) => new() { Succeeded = false, Errors = errors.ToList() };
    }
}