namespace Deep.Transactions.Domain.Customer;

public interface ICustomerRepository
{
    Task<Customer?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    void Insert(Customer customer);
}
