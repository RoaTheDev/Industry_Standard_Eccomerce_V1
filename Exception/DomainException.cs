namespace Ecommerce_site.Exception;

public class DomainException(string? message) : System.Exception(message);

public class EntityAlreadyExistException(string? message) : DomainException(message);

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string reason) : base($"{reason}")
    {
    }

    public EntityNotFoundException(Type entity, object id) : base(($"{nameof(entity.Name)} by {id} not found."))
    {
    }
}