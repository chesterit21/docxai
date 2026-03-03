using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain.EntityResponses.Dms;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Api.Domain.Formatters;

namespace Api.Repository.Masters
{
	public interface IDocumentFavoriteRepository : IRepository<DocumentFavorite>
	{
		Task<ResponseDocumentsFavoriteAndCount> GetAsync(string documentTitle, int page, int limit);
		Task<int> GetCountAsync(string documentTitle);
	}

	public class DocumentFavoriteRepository(DataContext context, IHttpContextAccessor accessor) : Repository<DocumentFavorite>(context, accessor), IDocumentFavoriteRepository
	{
		private long GetFileSizeCumulated(List<DocumentFiles> docs)
		{
			long TotalSize = 0;
			foreach (var doc in docs)
			{
				TotalSize += doc.DocumentFileSize;
			}
			return TotalSize;
		}

		public async Task<ResponseDocumentsFavoriteAndCount> GetAsync(string documentTitle, int page, int limit)
		{
			var skip = Skip(page, limit);

			var query = context.DocumentFavorites.Where(x => x.Owner == UserId)
				.Join(context.Documents.Include(x => x.OwnerInfo).Include(f => f.DocumentFiles)
						.LeftJoin(context.User,
						"UpdatedBy",
						"UserId",
						(DC, userUpdate) => new
						{
							DC,
							UpdatedByUserName = userUpdate.UserName,
							UpdatedByFullName = userUpdate.FullName,
							Size = DC.DocumentFiles.Sum(x => x.DocumentFileSize),
						}).Where(y => y.DC.DocumentTitle.Trim().ToLower().Contains((string.IsNullOrEmpty(documentTitle) ? "" : documentTitle.Trim().ToLower()))),
				   e => e.DocumentId,
				   d => d.DC.Id,
				   (e, d) => new ResponseDocumentsFavorite
				   {
					   Id = d.DC.Id,
					   DocumentTitle = d.DC.DocumentTitle,
					   DocumentDesc = d.DC.DocumentDesc,
					   Owner = d.DC.Owner,
					   OwnerFullname = d.DC.OwnerInfo.FullName,
					   InsertedAt = d.DC.InsertedAt,
					   UpdateAt = d.DC.UpdatedAt,
					   UpdatedByFullName = d.UpdatedByFullName,
					   Size = SizeFormatter.SizeSuffix(d.Size, 2),
					   IsFavorite = true
				   });

			if (page > 0 && limit > 0)
				query = query.Skip(skip).Take(limit);

			ResponseDocumentsFavoriteAndCount d = new ResponseDocumentsFavoriteAndCount() { TotalRecord = query.Count() };
			d.Records = await query.ToListAsync();
			return d;
		}

		public async Task<int> GetCountAsync(string documentTitle)
		{
			var query = context.DocumentFavorites.Where(x => x.Owner == UserId).Join(context.Documents.Where(y => y.DocumentTitle.Trim().ToLower().Contains((string.IsNullOrEmpty(documentTitle) ? "" : documentTitle.Trim().ToLower()))),
			   e => e.DocumentId,
			   d => d.Id,
			   (e, d) => new { d });


			return await query.CountAsync();
		}
	}
}