using Ecommerce_site.Dto;

namespace Ecommerce_site.Exception
{
    public class ApiValidationException : System.Exception
    {
        public ApiValidationException(string message)
            : base(message)
        {
            
        }
    }
}