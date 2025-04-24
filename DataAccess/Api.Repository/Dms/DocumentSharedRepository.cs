using Api.DataAccess;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Api.Repository.Masters
{
    public interface IDocumentSharedRepository : IRepository<DocumentShared>
    {
		
	}

    public class DocumentSharedRepository(DataContext context, IHttpContextAccessor accessor) : Repository<DocumentShared>(context, accessor), IDocumentSharedRepository
	{
		
	}
}
