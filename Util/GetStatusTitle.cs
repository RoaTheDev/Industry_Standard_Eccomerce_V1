namespace Ecommerce_site.Util;

public class GetStatusTitle
{
    public static string GetTitleForStatus(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status503ServiceUnavailable => "Service Unavailable",
            _ => "Internal Server Error"
        };
    }
}