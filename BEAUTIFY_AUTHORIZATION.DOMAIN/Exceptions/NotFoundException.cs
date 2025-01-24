namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Exceptions;
public abstract class NotFoundException : DomainException
{
    protected NotFoundException(string message)
        : base("Not Found", message)
    {
    }
}