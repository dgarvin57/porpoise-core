using Porpoise.Core.Models;

namespace Porpoise.Core.Application.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(int tenantId);
    Task<Tenant?> GetByKeyAsync(string tenantKey);
    Task<IEnumerable<Tenant>> GetAllAsync();
    Task<int> AddAsync(Tenant tenant);
    Task<bool> UpdateAsync(Tenant tenant);
    Task<bool> DeleteAsync(int tenantId);
}
