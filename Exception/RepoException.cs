namespace Ecommerce_site.Exception;

public class RepoException(string message, System.Exception innerException) : System.Exception(message, innerException);