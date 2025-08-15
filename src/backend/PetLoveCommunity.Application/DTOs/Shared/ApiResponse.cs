namespace PetLoveCommunity.Application.DTOs.Shared;

public record ApiResponse(bool IsSuccess, string? Message = null)
{
    public static ApiResponse Success() => new(true, null);
    public static ApiResponse Fail(string? message) => new(false, message);
}

public record ApiResponse<TData>(bool IsSuccess, TData Data, string? Message)
{
    public static ApiResponse<TData> Success(TData data) => new(true, data, null);
    public static ApiResponse<TData> Success(TData data, string message) => new(true, data, message);
    public static ApiResponse<TData> Fail(string? message) => new(false, default!, message);
    public static ApiResponse<TData> Failure(string? message) => new(false, default!, message);
    public static ApiResponse<TData> ValidationError(IList<string> errors) => new(false, default!, $"Validation failed: {string.Join(", ", errors)}");
}