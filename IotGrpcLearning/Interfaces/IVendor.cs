using IotGrpcLearning.Models;

namespace IotGrpcLearning.Interfaces
{
    public interface IVendor
    {
		Task<IEnumerable<VendorDto>> GetAllAsync(CancellationToken ct = default);
		Task<VendorDto?> GetAsync(string id, CancellationToken ct = default);
		Task<VendorDto> CreateAsync(VendorDto dto, CancellationToken ct = default);
		Task<bool> UpdateAsync(string id, VendorDto dto, CancellationToken ct = default);
		Task<bool> DeleteAsync(string id, CancellationToken ct = default);
	}
}
