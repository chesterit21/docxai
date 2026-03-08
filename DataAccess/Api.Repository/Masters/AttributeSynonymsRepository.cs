using Api.DataAccess;
using Api.DataAccess.Models.Masters;

namespace Api.Repository.Masters
{
    public class AttributeSynonymsRepository(DataContext context, Microsoft.AspNetCore.Http.IHttpContextAccessor accessor) : Repository<TmAttributeSynonyms>(context, accessor), IAttributeSynonymsRepository
    {
    }
}
