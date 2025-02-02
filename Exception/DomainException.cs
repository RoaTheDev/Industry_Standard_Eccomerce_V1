namespace Ecommerce_site.Exception;

public class DomainException(string? message) : System.Exception(message);

public class EntityAlreadyExistException(string? message) : DomainException(message);

public class EntityNotFoundException(Type entity, object id)
    : DomainException($"{nameof(entity.Name)} by {id} not found.");
    