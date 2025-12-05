using Porpoise.Core.Models;

namespace Porpoise.Core.Application.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(string tenantId);
    Task<Tenant?> GetByKeyAsync(string tenantKey);
    Task<IEnumerable<Tenant>> GetAllAsync();
    Task<string> AddAsync(Tenant tenant);  // Returns GUID as string
    Task<bool> UpdateAsync(Tenant tenant);
    Task<bool> DeleteAsync(string tenantId);
}
