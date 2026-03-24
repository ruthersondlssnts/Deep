using Vast.Common.Domain;

namespace Vast.Transactions.Domain.Customer;

public static class CustomerErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Customer.NotFound", $"The user with the identifier {id} was not found");

    public static readonly Error CustomerAlreadyExists = Error.Problem(
        "Customer.AlreadyExists",
        "The customer already exists"
    );

    public static readonly Error InvalidCustomer = Error.Problem(
        "Customer.InvalidCustomer",
        "The customer is invalid"
    );
}
