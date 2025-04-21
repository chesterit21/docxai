using Api.DataAccess;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Api.Repository.Masters
{
    public interface IAttributtesRepository : IRepository<Attributtes>
    {
		
	}

    public class AttributtesRepository(DataContext context, IHttpContextAccessor accessor) : Repository<Attributtes>(context, accessor), IAttributtesRepository
	{
		
	}
}
