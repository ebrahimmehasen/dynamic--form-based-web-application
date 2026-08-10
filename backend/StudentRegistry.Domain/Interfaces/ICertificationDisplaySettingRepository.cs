using System.Collections.Generic;
using System.Threading.Tasks;
using StudentRegistry.Domain.Entities;

namespace StudentRegistry.Domain.Interfaces
{
    public interface ICertificationDisplaySettingRepository
    {
        Task<IEnumerable<CertificationDisplaySetting>> GetAllAsync();
        Task<CertificationDisplaySetting?> GetByKeyAsync(string certificationKey);
        Task AddAsync(CertificationDisplaySetting setting);
    }
}
