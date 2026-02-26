using IotGrpcLearning.Models;

namespace IotGrpcLearning.Interfaces
{
    public interface ICustomer
    {
		Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken ct = default);
		Task<CustomerDto?> GetAsync(string id, CancellationToken ct = default);
		Task<CustomerDto> CreateAsync(CustomerDto dto, CancellationToken ct = default);
		Task<bool> UpdateAsync(string id, CustomerDto dto, CancellationToken ct = default);
		Task<bool> DeleteAsync(string id, CancellationToken ct = default);
	}
}
