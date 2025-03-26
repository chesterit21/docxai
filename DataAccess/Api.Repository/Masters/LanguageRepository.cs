using Api.DataAccess;
using Api.DataAccess.Models.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Api.Repository.Masters
{
    public interface ILanguageRepository : IRepository<Language>
    {
        Task<bool> CheckIfEExist(List<string> codes, string type);
        Task<int> Delete(List<string> codes, string type);
        Task<Language> Get(string code, string type);
    }

    public class LanguageRepository(DataContext context, IHttpContextAccessor accessor) : Repository<Language>(context, accessor), ILanguageRepository
    {
        public async Task<Language> Get(string code, string type) => await context.Language.FirstOrDefaultAsync(x => x.Code == code && x.Type == type);

        public async Task<bool> CheckIfEExist(List<string> codes, string type)
        {
            var count = await context.Language.CountAsync(x => x.Type == type && codes.Contains(x.Code));
            return count == codes.Count;
        }

        public async Task<int> Delete(List<string> codes, string type)
        {
            return await context.Language.Where(x => x.Type == type && codes.Contains(x.Code)).ExecuteDeleteAsync();
        }
    }
}
