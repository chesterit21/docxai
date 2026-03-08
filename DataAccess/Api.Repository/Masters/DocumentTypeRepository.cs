using Api.DataAccess;
using Api.DataAccess.Models.Masters;

namespace Api.Repository.Masters
{
    public class DocumentTypeRepository(DataContext context, Microsoft.AspNetCore.Http.IHttpContextAccessor accessor) : Repository<TmDocumentType>(context, accessor), IDocumentTypeRepository
    {
    }
}
