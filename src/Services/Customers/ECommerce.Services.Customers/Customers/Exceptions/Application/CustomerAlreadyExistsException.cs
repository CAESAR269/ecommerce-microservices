using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Services.Customers.Customers.Exceptions.Application;

public class CustomerAlreadyExistsException : AppException
{
    public long? CustomerId { get; }
    public Guid? IdentityId { get; }

    public CustomerAlreadyExistsException(string message)
        : base(message, StatusCodes.Status409Conflict) { }

    public CustomerAlreadyExistsException(Guid identityId)
        : base($"Customer with IdentityId: '{identityId}' already exists.", StatusCodes.Status409Conflict)
    {
        IdentityId = identityId;
    }

    public CustomerAlreadyExistsException(long customerId)
        : base($"Customer with ID: '{customerId}' already exists.", StatusCodes.Status409Conflict)
    {
        CustomerId = customerId;
    }
}
