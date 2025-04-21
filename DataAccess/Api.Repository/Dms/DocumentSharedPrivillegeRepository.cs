using Api.DataAccess;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Api.Repository.Masters
{
    public interface IDocumentSharedPrivillegeRepository : IRepository<DocumentSharedPrivillege>
    {
		
	}

    public class DocumentSharedPrivillegeRepository(DataContext context, IHttpContextAccessor accessor) : Repository<DocumentSharedPrivillege>(context, accessor), IDocumentSharedPrivillegeRepository
	{
		
	}
}
