using Api.DataAccess;
using Api.DataAccess.Models.Dms;

namespace Api.Repository.Dms
{
    public class DocumentExtractedEntitiesRepository : Repository<DocumentExtractedEntities>, IDocumentExtractedEntitiesRepository
    {
        public DocumentExtractedEntitiesRepository(DataContext context, Microsoft.AspNetCore.Http.IHttpContextAccessor accessor) : base(context, accessor)
        {
        }
    }
}
