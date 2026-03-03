using Api.DataAccess;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityResponses.Dms;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Api.Repository.Masters
{
    public interface IDocumentAttributesRepository : IRepository<DocumentAttributes>
    {
        Task<ResponseDocumentAttributes> GetDocumentAttributeAsync(int DocumentId);
		Task<int> SaveDocumentAttribute(DocumentAttributes request);

	}

    public class DocumentAttributesRepository(DataContext context, IHttpContextAccessor accessor) : Repository<DocumentAttributes>(context, accessor), IDocumentAttributesRepository
    {
        public async Task<ResponseDocumentAttributes> GetDocumentAttributeAsync(int DocumentId) 
        {
            return await context.DocumentAttributes.Where(x => x.DocumentID == DocumentId)
                .Select(s => new ResponseDocumentAttributes
                {
                    DocumentID = s.DocumentID,
                    AttributeValues = s.AttributeValues,
                    Id = s.Id
                }).FirstOrDefaultAsync();
        }

		public async Task<int> SaveDocumentAttribute(DocumentAttributes request)
		{
			int result = 0;
			using var transaction = context.Database.BeginTransaction();
			{
				try
				{
					DocumentAttributes attr = new DocumentAttributes();
					if (request.DocumentID == 0)
					{
						attr = new DocumentAttributes()
						{
							DocumentID = request.DocumentID,
							AttributeValues = request.AttributeValues
						};
						await context.AddAsync(attr);
						result = context.SaveChanges();
					}
					else
					{
						attr = new DocumentAttributes()
						{
							DocumentID = request.DocumentID,
							AttributeValues = request.AttributeValues
						};

						context.Update(attr);
						await context.SaveChangesAsync();
					}

					result += context.SaveChanges();
					transaction.Commit();
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					throw;
				}
			}
			return result;
		}
	}
}
