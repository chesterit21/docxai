using Api.DataAccess;
using Api.DataAccess.Models.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Api.Repository.Masters
{
    public interface ICompanyRepository : IRepository<Company>
    {
        Task<bool> CheckIfEExist(List<string> companyIds);
        Task<int> Delete(List<string> companyIds);
    }

    public class CompanyRepository(DataContext context, IHttpContextAccessor accessor) : Repository<Company>(context, accessor), ICompanyRepository
    {
        public async Task<bool> CheckIfEExist(List<string> companyIds)
        {
            var count = await context.Company.CountAsync(x => companyIds.Contains(x.CompanyId));
            return count == companyIds.Count;
        }

        public async Task<int> Delete(List<string> companyIds)
        {
            return await context.Company.Where(x => companyIds.Contains(x.CompanyId)).ExecuteDeleteAsync();
        }
    }
}
