using Vast.Common.Domain;
using Vast.Common.Domain.Auditing;

namespace Vast.Transactions.Domain.Customer;

[Auditable]
public sealed class Customer : Entity
{
    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    private Customer() { }

    public static Result<Customer> Create(string fullName, string email)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(fullName))
        {
            return CustomerErrors.InvalidCustomer;
        }
        var customer = new Customer
        {
            Id = Guid.CreateVersion7(),
            FullName = fullName,
            Email = email,
        };

        return customer;
    }
}
