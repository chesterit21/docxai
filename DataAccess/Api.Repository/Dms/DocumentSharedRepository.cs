using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.DataAccess.Models.Systems;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityResponses.Dms;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Api.Repository.Masters
{
	public interface IDocumentSharedRepository : IRepository<DocumentShared>
	{
		Task<ResponseDocumentShared> GetSharedUsersByDocumentIDAsync(int DocumentId);
		Task<int> UpsertDocumentSharePrivillege(RequestDocumentShared DocSharePriv);
	}

	public class DocumentSharedRepository(DataContext context, IHttpContextAccessor accessor) : Repository<DocumentShared>(context, accessor), IDocumentSharedRepository
	{
		public async Task<ResponseDocumentShared> GetSharedUsersByDocumentIDAsync(int documentId)
		{
			//var documentShared = await context.DocumentShared
			//	.AsNoTracking()
			//	.Where(d => d.DocumentID == documentId)
			//	.Select(d => new ResponseDocumentShared
			//	{
			//		Id = d.Id,
			//		DocumentID = d.DocumentID,
			//		SharedUser = d.DocumentSharePrivs
			//			.Select(u => new ResponseUserPrivillege
			//			{
			//				ShareType = u.ShareType,
			//				UserID = u.ShareType == "user" ? u.UserID : null,
			//				GroupID = u.ShareType == "group" ? u.GroupID : null,
			//				GroupName = u.ShareType == "group" ? u.GroupInfo.GroupName : null,
			//				UserName = u.ShareType == "user" ? u.UsersInfo.UserName : null,
			//				FullName = u.ShareType == "user" ? u.UsersInfo.FullName : null,
			//				Email = u.ShareType == "user" ? u.UsersInfo.EmailAddress : null,
			//				IsView = u.IsView,
			//				IsDelete = u.IsDelete,
			//				IsEdit = u.IsEdit
			//			})
			//			.ToList()
			//	})
			//	.FirstOrDefaultAsync();

			//		return documentShared;

			var result = await context.DocumentShared
				.Include(c => c.DocumentSharePrivs).ThenInclude(e => e.UsersInfo)
				.Include(c => c.DocumentSharePrivs).ThenInclude(e => e.GroupInfo)
				.Where(d => d.DocumentID == documentId)
				.Select(d => new ResponseDocumentShared()
				{
					Id = d.Id,
					DocumentID = d.DocumentID,
					SharedUser = d.DocumentSharePrivs
									.Select(u => new ResponseUserPrivillege
									{
										ShareType = u.ShareType,
										UserID = u.UsersInfo == null ? null : u.UsersInfo.UserId,
										GroupID = u.GroupID,
										GroupName = u.GroupInfo == null ? null : u.GroupInfo.GroupName,
										UserName = u.UsersInfo == null ? null : u.UsersInfo.UserName,
										FullName = u.UsersInfo == null ? null : u.UsersInfo.FullName,
										Email = u.UsersInfo == null ? null : u.UsersInfo.EmailAddress,
										IsView = u.IsView,
										IsDelete = u.IsDelete,
										IsEdit = u.IsEdit
									}).ToList()
				}).FirstOrDefaultAsync();
			return result;
		}

		public async Task<int> UpsertDocumentSharePrivillege(RequestDocumentShared request)
		{
			int result = 0;
			var strategy = context.Database.CreateExecutionStrategy();
			await strategy.ExecuteAsync(async () =>
			{
				using var transaction = context.Database.BeginTransaction();
				{
					try
					{
						DocumentShared userDocShared = context.DocumentShared.AsNoTracking().FirstOrDefault(x => x.DocumentID == request.DocumentID);
						if (userDocShared != null)
						{
							userDocShared.UpdatedAt = DateTime.Now;
							userDocShared.UpdatedBy = UserId;
							context.DocumentShared.Update(userDocShared);
							result = context.SaveChanges();
						}
						else
						{
							userDocShared = new DocumentShared()
							{
								DocumentID = request.DocumentID,
								InsertedAt = DateTime.Now,
								InsertedBy = UserId,
								UpdatedAt = DateTime.Now,
								UpdatedBy = UserId,
								//DocumentFileID = request.DocumentFileID.Value
							};
							await context.AddAsync(userDocShared);
							result = context.SaveChanges();
						}

						List<DocumentSharedPrivillege> privs = new List<DocumentSharedPrivillege>();
						List<DocumentSharedPrivillege> privsUpdate = new List<DocumentSharedPrivillege>();
						foreach (var item in request.requestUserDocPrivillege)
						{
							DocumentSharedPrivillege priv = new DocumentSharedPrivillege()
							{
								DocumentSharedID = userDocShared.Id,
								ShareType = item.ShareType,
								UserID = item.ShareType == "user" ? item.UserID : null,
								GroupID = item.ShareType == "group" ? item.GroupID : null,
								IsView = item.IsView,
								IsEdit = item.IsEdit,
								IsDelete = item.IsDelete,
								InsertedAt = DateTime.Now,
								InsertedBy = UserId,
								UpdatedAt = DateTime.Now,
								UpdatedBy = UserId,
							};

							DocumentSharedPrivillege ExistUserPriv = null;
							if (item.ShareType == "user")
							{
								ExistUserPriv = context.DocumentSharedPrivillege.FirstOrDefault(x => x.UserID == item.UserID && x.ShareType == "user" && x.DocumentSharedID == userDocShared.Id);
							}
							if (item.ShareType == "group")
							{
								ExistUserPriv = context.DocumentSharedPrivillege.FirstOrDefault(x => x.GroupID == item.GroupID && x.ShareType == "group" && x.DocumentSharedID == userDocShared.Id);
							}

							if (ExistUserPriv == null)
							{
								privs.Add(priv);
							}
							else
							{
								ExistUserPriv.UserID = item.UserID;
								ExistUserPriv.ShareType = item.ShareType;
								ExistUserPriv.GroupID = item.GroupID;
								ExistUserPriv.IsView = item.IsView;
								ExistUserPriv.IsEdit = item.IsEdit;
								ExistUserPriv.IsDelete = item.IsDelete;
								ExistUserPriv.UpdatedAt = DateTime.Now;
								ExistUserPriv.UpdatedBy = UserId;
								privsUpdate.Add(ExistUserPriv);
							}

						}

						var usersAndGroupsPrivExist = context.DocumentSharedPrivillege.Where(x => x.DocumentSharedID == userDocShared.Id);
						var userAndGroupDeleted = usersAndGroupsPrivExist.Where(x => !privs.Contains(x) && !privsUpdate.Contains(x));
						if (userAndGroupDeleted.Count() > 0)
						{
							context.DocumentSharedPrivillege.RemoveRange(userAndGroupDeleted);
							result += context.SaveChanges();
						}

						if (privs.Count > 0)
						{
							await context.AddRangeAsync(privs);
							result += context.SaveChanges();
						}

						if (privsUpdate.Count > 0)
						{
							context.DocumentSharedPrivillege.UpdateRange(privsUpdate);
							result += context.SaveChanges();
						}

						transaction.Commit();
					}
					catch (Exception ex)
					{
						transaction.Rollback();
						throw;
					}
				}
			});
			return result;
		}
	}
}
