namespace Ecommerce_site.Dto;

// Standardized API response structure.
// T represents then Type of the response data
public class ApiStandardResponse<T>
{
    public ApiStandardResponse(T data)
    {
        Data = data;
        Success = true;
        Errors = new List<string>();
    }

    public ApiStandardResponse(int statusCode, T data)
    {
        StatusCode = statusCode;
        Success = true;
        Data = data;
        Errors = new List<string>();
    }

    public ApiStandardResponse(int statusCode, List<string> errors, T data)
    {
        StatusCode = statusCode;
        Success = false;
        Errors = errors;
        Data = data;
    }

    public ApiStandardResponse(int statusCode, string error, T data)
    {
        StatusCode = statusCode;
        Data = data;
        Success = false;
        Errors = new List<string> { error };
    }

    // HTTP status code of the response.
    public int StatusCode { get; set; }

    // Indicates whether the request was successful.
    public bool Success { get; set; }

    // Response data in case successful and null when it is not
    public T Data { get; set; }

    // List of error messages, if any.
    public List<string> Errors { get; set; }
}