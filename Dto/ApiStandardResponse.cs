namespace Ecommerce_site.Dto;

// Standardized API response structure.
// T represents then Type of the response data
public class ApiStandardResponse<T>
{
    public ApiStandardResponse(T data)
    {
        Data = data;
        Success = true;
    }

    public ApiStandardResponse(int statusCode, T data)
    {
        StatusCode = statusCode;
        Success = true;
        Data = data;
    }

    public ApiStandardResponse(int statusCode, List<object> errors)
    {
        StatusCode = statusCode;
        Success = false;
        Errors = errors;
    }

    public ApiStandardResponse(int statusCode, object error)
    {
        StatusCode = statusCode;
        Success = false;
        Errors = new List<object> { error };
    }

    // HTTP status code of the response.
    public int StatusCode { get; set; }

    // Indicates whether the request was successful.
    public bool Success { get; set; }

    // Response data in case successful and null when it is not
    public T? Data { get; set; }

    // List of error messages, if any.
    public List<object>? Errors { get; set; }
}