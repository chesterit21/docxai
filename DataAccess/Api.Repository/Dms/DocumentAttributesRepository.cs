using Api.DataAccess;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Api.Repository.Masters
{
    public interface IDocumentAttributesRepository : IRepository<DocumentAttributes>
    {
		
	}

    public class DocumentAttributesRepository(DataContext context, IHttpContextAccessor accessor) : Repository<DocumentAttributes>(context, accessor), IDocumentAttributesRepository
	{
		
	}
}
