using IotGrpcLearning.Models;

namespace IotGrpcLearning.Interfaces
{
    public interface ICustomer
    {
		Task<IEnumerable<CustomerDto>> GetAllAsync(PaginationDto body, CancellationToken ct = default);
		Task<CustomerDto?> GetAsync(int id, CancellationToken ct = default);
		Task<CustomerDto> CreateAsync(CustomerDto dto, CancellationToken ct = default);
		Task<bool> UpdateAsync(int id, CustomerDto dto, CancellationToken ct = default);
		Task<bool> DeleteAsync(int id, CancellationToken ct = default);
	}
}
