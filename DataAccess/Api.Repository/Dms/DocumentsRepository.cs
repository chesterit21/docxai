using Api.DataAccess;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Api.Repository.Masters
{
    public interface IDocumentsRepository : IRepository<Documents>
    {
		
	}

    public class DocumentsRepository(DataContext context, IHttpContextAccessor accessor) : Repository<Documents>(context, accessor), IDocumentsRepository
	{
		
	}
}
