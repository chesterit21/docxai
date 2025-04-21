using Api.DataAccess;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Api.Repository.Masters
{
    public interface IDocumentFilesRepository : IRepository<DocumentFiles>
    {
		
	}

    public class DocumentFilesRepository(DataContext context, IHttpContextAccessor accessor) : Repository<DocumentFiles>(context, accessor), IDocumentFilesRepository
	{
		
	}
}
