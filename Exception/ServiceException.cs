namespace Ecommerce_site.Exception;

public class ServiceException(string message, System.Exception innerException) : System.Exception(message, innerException);