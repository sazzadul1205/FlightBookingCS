namespace TaskManagerCS.Services;

public class ServiceResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public static ServiceResult Ok()
    {
        return new ServiceResult()
        {
            Success = true
        };
    }

    public static ServiceResult Error(string errorMessage)
    {
        return new ServiceResult()
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}