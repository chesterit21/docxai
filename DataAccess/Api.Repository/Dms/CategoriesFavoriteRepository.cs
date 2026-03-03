using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain.EntityResponses.Dms;
using Api.Domain.Formatters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Api.Repository.Masters
{
	public interface ICategoriesFavoriteRepository : IRepository<CategoriesFavorite>
	{
		Task<ResponseCategoriesFavoriteAndCount> GetAsync(string categoryName, int page, int limit);
		Task<int> GetCountAsync(string categoryName);
	}

	public class CategoriesFavoriteRepository(DataContext context, IHttpContextAccessor accessor) : Repository<CategoriesFavorite>(context, accessor), ICategoriesFavoriteRepository
	{
		private long GetFileSizeCumulated(List<Documents> docs)
		{
			long TotalSize = 0;
			foreach (var doc in docs)
			{
				TotalSize += doc.DocumentFiles.Sum(x => x.DocumentFileSize);
			}
			return TotalSize;
		}

		public async Task<ResponseCategoriesFavoriteAndCount> GetAsync(string categoryName, int page, int limit)
		{
			var skip = Skip(page, limit);

			var query = context.CategoriesFavorites.Where(x => x.Owner == UserId)
				.Join(
					context.Categories.Include(ct => ct.OwnerInfo).Include(d => d.Documents).ThenInclude(d => d.DocumentFiles)
						.LeftJoin(context.User,
						"UpdatedBy",
						"UserId",
						(CT, userUpdate) => new
						{
							CT,
							UpdatedByUserName = userUpdate.UserName,
							UpdatedByFullName = userUpdate.FullName,
							//Size = CT.Document
						})
						.Where(y => y.CT.CategoryName.Trim().ToLower().Contains((string.IsNullOrEmpty(categoryName) ? "" : categoryName.Trim().ToLower()))),
				   e => e.CategoryID,
				   d => d.CT.Id,
				   (e, d) => new ResponseCategoriesFavorite
				   {
					   Id = d.CT.Id,
					   CategoryName = d.CT.CategoryName,
					   CategoryDesc = d.CT.CategoryDesc,
					   Owner = d.CT.Owner,
					   OwnerFullname = d.CT.OwnerInfo.FullName,
					   InsertedAt = d.CT.InsertedAt,
					   UpdateAt = d.CT.UpdatedAt,
					   UpdatedByFullName = d.UpdatedByFullName,
					   Size = SizeFormatter.SizeSuffix((long)d.CT.Documents.ToList().Sum(x => x.FileSize),  2),
					   IsFavorite = true
				   });

			ResponseCategoriesFavoriteAndCount responseCategoriesFavoriteAndCount = new ResponseCategoriesFavoriteAndCount() { TotalRecord = query.Count() };

			if (page > 0 && limit > 0)
				query = query.Skip(skip).Take(limit);

			responseCategoriesFavoriteAndCount.Records = await query.ToListAsync();
			return responseCategoriesFavoriteAndCount;
		}

		public async Task<int> GetCountAsync(string categoryName)
		{
			var query = context.CategoriesFavorites.Where(x => x.Owner == UserId).Join(context.Categories.Where(y => y.CategoryName.Trim().ToLower().Contains((string.IsNullOrEmpty(categoryName) ? "" : categoryName.Trim().ToLower()))),
			   e => e.CategoryID,
			   d => d.Id,
			   (e, d) => new { d });


			return await query.CountAsync();
		}
	}
}