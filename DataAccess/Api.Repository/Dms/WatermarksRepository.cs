using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain.EntityResponses.Dms;
using Api.Domain.Formatters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Api.Repository.Masters
{
	public interface IWatermarksRepository : IRepository<Watermarks>
	{
		Task<ResponseWatermarkRequestAndCount> GetListWatermarks(string search, int page = 0, int limit = 0);
		Task<List<DropdownTextValue>> GetDropdownWatermarks();
		Task<bool> SoftDelete(int id); 
	}

	public class WatermarksRepository(DataContext context, IHttpContextAccessor accessor) : Repository<Watermarks>(context, accessor), IWatermarksRepository
	{
		public async Task<ResponseWatermarkRequestAndCount> GetListWatermarks(string search, int page = 0, int limit = 0)
		{
			var skip = Skip(page, limit);

			var query = context.Watermarks.Where(x => x.IsActive == true)
			.WhereIf(!string.IsNullOrEmpty(search), x => x.Text.ToLower().Contains(search.ToLower() ?? ""))
			.Include(x => x.InsertedUser)
			.Include(x => x.UpdatedUser)
			.Select(x => new
			ResponseWatermark
			{
				Id = x.Id,
				Text = x.Text,
				InsertedByFullname = x.InsertedUser.FullName,
				InsertedAt = x.InsertedAt,
				UpdatedByFullName = x.UpdatedUser == null ? "" : x.UpdatedUser.FullName,
				UpdatedAt = x.UpdatedAt
			});

			ResponseWatermarkRequestAndCount returnObj = new ResponseWatermarkRequestAndCount { TotalRecord = query.Count() };

			if (page > 0 && limit > 0)
				query = query.Skip(skip).Take(limit);

			returnObj.Records =await query.ToListAsync();
			return returnObj;
		}

		public async Task<List<DropdownTextValue>> GetDropdownWatermarks()
		{
			var users = await context.Watermarks.Where(s => s.IsActive == true).Select(x => new DropdownTextValue { Text = x.Text, Value = x.Id }).ToListAsync();
			return users;

		}

		public async Task<bool> SoftDelete(int id)
		{
			int result = context.Watermarks
				.Where(o => o.Id == id)
				.ExecuteUpdate(setters => setters
					.SetProperty(o => o.IsActive, false)
					.SetProperty(o => o.UpdatedBy, UserId)
					.SetProperty(o => o.UpdatedAt, DateTime.Now));

			return result > 0;

		}
	}
}