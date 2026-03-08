using Api.DataAccess;
using Api.DataAccess.Models.Masters;

namespace Api.Repository.Masters
{
    public class DocumentTypeAttributesRepository(DataContext context, Microsoft.AspNetCore.Http.IHttpContextAccessor accessor) : Repository<TmDocumentTypeAttributes>(context, accessor), IDocumentTypeAttributesRepository
    {
    }
}
