using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Api.DataAccess.Models.Dms;
using Api.Domain.EntityRequests.Dms;
using System.Transactions;

namespace Api.Repository.Masters
{
	public interface ICategoriesSharedRepository : IRepository<CategoriesShared>
	{
		Task<CategoriesShared> InsertCategoriesShared(RequestCategoriesShared request);
	}

	public class CategoriesSharedRepository(DataContext context, IHttpContextAccessor accessor) : Repository<CategoriesShared>(context, accessor), ICategoriesSharedRepository
	{

		public async Task<CategoriesShared> InsertCategoriesShared(RequestCategoriesShared request)
		{
			using var transaction = await context.Database.BeginTransactionAsync();
			{
				try
				{
					var name = accessor?.HttpContext?.User?.Identity?.Name ?? "0";
					int userId = int.Parse(name);
					var modelCategoriesShared = new CategoriesShared();
					foreach (var x in request.User)
					{
						
						var modelCategoriesSharedPriv = new CategoriesSharedPrivillege();
						modelCategoriesShared.Id = 0;
						modelCategoriesShared.CategoryID = request.CategoryID;
						modelCategoriesShared.UserID = x.UserID;
						modelCategoriesShared.InsertedBy = userId;
						modelCategoriesShared.InsertedAt = DateTime.UtcNow;

						await context.AddAsync<CategoriesShared>(modelCategoriesShared);

						modelCategoriesSharedPriv.Id = 0;
						modelCategoriesSharedPriv.CategoriesSharedID = modelCategoriesShared.Id;
						if (request.User.Select(x => x.UserPrivillege.Value == Domain.Enum.EnumPrivillege.All).FirstOrDefault())
						{
							modelCategoriesSharedPriv.IsView = true;
							modelCategoriesSharedPriv.IsEdit = true;
							modelCategoriesSharedPriv.IsDelete = true;
						}
						else if (request.User.Select(x => x.UserPrivillege.Value == Domain.Enum.EnumPrivillege.View).FirstOrDefault())
							modelCategoriesSharedPriv.IsView = true;
						else if (request.User.Select(x => x.UserPrivillege.Value == Domain.Enum.EnumPrivillege.Edit).FirstOrDefault())
							modelCategoriesSharedPriv.IsEdit = true;
						else if (request.User.Select(x => x.UserPrivillege.Value == Domain.Enum.EnumPrivillege.Delete).FirstOrDefault())
							modelCategoriesSharedPriv.IsDelete = true;

						modelCategoriesSharedPriv.InsertedBy = userId;
						modelCategoriesSharedPriv.InsertedAt = DateTime.UtcNow;

						await context.AddAsync<CategoriesSharedPrivillege>(modelCategoriesSharedPriv);
						transaction.Commit();

					}
					return modelCategoriesShared;
				}

				catch (Exception ex) {
					transaction.Rollback();
					throw;

				}

			}
		}
	}
}