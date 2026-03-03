using Api.DataAccess;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using MathNet.Numerics.Statistics.Mcmc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Api.Repository.Masters
{
    public interface ICompanyRepository : IRepository<Company>
    {
        Task<bool> CheckIfExist(List<string> companyIds);
        Task<int> Delete(List<string> companyIds);
		Task<List<DropdownTextValueString>> GetDropdownCompany();
		//Task<List<Company>> GetAsync(Expression<Func<Company, bool>> predicate, int page, int limit, string orderBy, string orderOrientation, string filterBy, string filterValue);
	}

    public class CompanyRepository(DataContext context, IHttpContextAccessor accessor) : Repository<Company>(context, accessor), ICompanyRepository
    {
        public async Task<bool> CheckIfExist(List<string> companyIds)
        {
            var count = await context.Company.CountAsync(x => companyIds.Contains(x.CompanyId));
            return count == companyIds.Count;
        }

        public async Task<int> Delete(List<string> companyIds)
        {
            return await context.Company.Where(x => companyIds.Contains(x.CompanyId)).ExecuteDeleteAsync();
        }

		public async Task<List<DropdownTextValueString>> GetDropdownCompany()
		{
			var users = await context.Company.Select(x => new DropdownTextValueString { Text = x.Name, Value = x.CompanyId }).ToListAsync();
			return users;

		}

        //public async override Task<List<Company>> GetAsync(Expression<Func<Company, bool>> predicate, int page, int limit, string orderBy, string orderOrientation, string filterBy, string filterValue)
        //{
        //    context.Company.Where(x => x.IsActive == true)
        //}
	}
}
