using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityRequests.Masters;
using Api.Domain.EntityResponses.Dms;
using EFCore.BulkExtensions;
using MathNet.Numerics.Statistics.Mcmc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Math;

namespace Api.Repository.Masters
{
	public interface IGroupRepository : IRepository<Group>
	{
		Task<bool> CheckIfExist(List<int> groupIds);
		Task<int> DeleteWithChildren(List<int> groupIds);
		Task<Group> GetGroupByName(string groupName);
		Task<object> GetByIdAndMembers(int groupId);
		bool UpdateGroupMembers(RequestGroupUpdateUser request);
		Task<List<DropdownTextValue>> GetDropdownGroups();

		Task<ResponseGroupPagination> GetAsync(string groupname, bool isActive, int page, int limit);
		Task<Group> MarkAsDeletedAsync(int grpId);
		Task<Group> MarkAsUnDeletedAsync(int grpId);
		Task<int> DeleteAsync(int id);
		Task<bool> EmptyRecyclebin(int batchSize);
		Task<int> PurgeDeletedGroups(int batchSize, int purgingInMonth);
	}

	public class GroupRepository(DataContext context, IHttpContextAccessor accessor, IUserRepository userRepository) : Repository<Group>(context, accessor), IGroupRepository
	{
		private readonly IUserRepository _userRepository = userRepository;

		public async Task<bool> CheckIfExist(List<int> groupId)
		{
			var count = await context.Group.CountAsync(x => groupId.Contains(x.GroupId));
			return count == groupId.Count;
		}

		public async Task<int> DeleteWithChildren(List<int> groupIds)
		{
			var userGroups = await context.UserGroup.Where(x => groupIds.Contains(x.GroupId)).ToListAsync();
			var Groups = await context.Group.Where(x => groupIds.Contains(x.GroupId)).ToListAsync();

			context.RemoveRange(userGroups.ToArray());
			context.RemoveRange(Groups.ToArray());

			return await context.SaveChangesAsync();
		}

		public async Task<Group> GetGroupByName(string groupName)
		{
			return await context.Group.FirstOrDefaultAsync(x => x.GroupName.Contains(groupName));
		}

		public async Task<object> GetByIdAndMembers(int groupId)
		{
			return await context.Group.Include(x => x.UserGroup).ThenInclude(c => c.User)
				.LeftJoin(context.User, G => G.InsertedBy, user => user.UserId,
					(G, user) => new
					{
						G,
						InsertedByFullName = user.FullName,
						UpdatedByFullName = ""
					})
				.LeftJoin(context.User,
					"G.UpdatedBy",
					"UserId",
					(G1, userUpdate) => new
					{
						G1,
						G1.G.InsertedByFullName,
						UpdatedByFullName = userUpdate.FullName
					})
				.Where(x => x.G1.G.GroupId == groupId)
				.Select(x => new
				{
					x.G1.G.GroupId,
					x.G1.G.GroupName,
					x.G1.G.GroupDescription,
					x.G1.G.InsertedAt,
					x.G1.G.InsertedByFullName,
					x.G1.G.UpdatedAt,
					x.G1.G.UpdatedByFullName,
					UserGroup = x.G1.G.UserGroup.Select(c => new
					{
						c.UserId,
						c.User.FullName,
						c.User.EmailAddress
					})
				}).ToListAsync();
		}

		public async Task<List<DropdownTextValue>> GetDropdownGroups()
		{
			var users = await context.Group.Where(x => x.IsActive == true && x.GroupName.ToLower() != "everyone")
				.Select(x => new DropdownTextValue { Text = x.GroupName, Value = x.GroupId }).ToListAsync();
			//if (users.Any(x => x.Text.ToLower().Contains("everyone")))
			//{
			//	var g = users.FirstOrDefault(y => y.Text.ToLower().Contains("everyone"));
			//	users.Remove(g);
			//}
			return users;

		}

		public bool UpdateGroupMembers(RequestGroupUpdateUser request)
		{
			int result = 0;
			{
				var executionStrategy = context.Database.CreateExecutionStrategy();
				executionStrategy.Execute(
				() =>
				{
					using var transaction = context.Database.BeginTransaction();
					{
						try
						{
							var exg = context.Group.FirstOrDefault(x => x.GroupId == request.GroupId);

							var group = new Group()
							{
								GroupId = request.GroupId,
								GroupName = request.GroupName,
								GroupDescription = request.GroupDescription,
								UpdatedBy = UserId,
								UpdatedAt = DateTime.Now
							};

							if (exg != null)
							{
								group.IsActive = true;
								group.IsSystem = exg.IsSystem;
							}


							context.Update<Group>(group);
							result += context.SaveChanges();

							List<UserGroup> userGrpList = new List<UserGroup>();
							if (request.Users.Count > 0)
							{
								foreach (var a in request.Users)
								{
									var userGrp = new UserGroup()
									{
										GroupId = request.GroupId,
										UserId = a.UserId,
										InsertedBy = UserId,
										InsertedAt = DateTime.Now,
										UpdatedBy = UserId,
										UpdatedAt = DateTime.Now,
									};

									userGrpList.Add(userGrp);
									var anyExist = context.UserGroup.Count(u => u.GroupId == userGrp.GroupId && u.UserId == userGrp.UserId);
									if (anyExist > 0)
									{
										context.UserGroup.Update(userGrp);
									}
									else
									{
										context.UserGroup.AddAsync(userGrp);
									}
								}

								result += context.SaveChanges();
							}

							List<UserGroup> userGrForDeleteList = new List<UserGroup>();
							var usersGroupExist = context.UserGroup.Where(x => x.GroupId == request.GroupId);
							foreach (var u in usersGroupExist)
							{
								var isanyonrequest = userGrpList.FirstOrDefault(m => m.GroupId == u.GroupId && m.UserId == u.UserId);
								if (isanyonrequest == null)
								{
									userGrForDeleteList.Add(u);
								}
							}
							if (userGrForDeleteList.Count() > 0)
							{
								context.UserGroup.RemoveRange(userGrForDeleteList);
								result += context.SaveChanges();
							}

							transaction.Commit();
						}
						catch
						{
							transaction.Rollback();
							throw;
						}
					}
				});
			}
			return result > 0;
		}

		public async Task<ResponseGroupPagination> GetAsync(string groupname, bool isActive, int page, int limit)
		{
			var skip = Skip(page, limit);
			var query = context.Group
				.Where(x => x.IsActive == isActive)
				.WhereIf(!isActive, x => x.IsActive == isActive && x.IsDeleted != true)
				.WhereIf(!string.IsNullOrWhiteSpace(groupname), x => (x.GroupName.ToLower().Contains(groupname.ToLower()) || x.GroupDescription.ToLower().Contains(groupname.ToLower())))
				.LeftJoin(context.User, left => left.InsertedBy, user => user.UserId,
					(left, userInsert) => new
					{
						Entity = left,
						InsertedByUserName = userInsert.UserName,
						InsertedByFullName = userInsert.FullName,
						UpdatedByUserName = "",
						UpdatedByFullName = ""
					})
				.LeftJoin(context.User,
					"Entity.UpdatedBy",//left => left.Entity.UpdatedBy,
					"UserId",//user => user.UserId,
					(left, userUpdate) => new
					{
						left.Entity,
						left.InsertedByUserName,
						left.InsertedByFullName,
						UpdatedByUserName = userUpdate.UserName,
						UpdatedByFullName = userUpdate.FullName
					});

			ResponseGroupPagination responseGroupPagination = new ResponseGroupPagination()
			{
				TotalREcord = query.Count()
			};

			if (page > 0 && limit > 0)
				query = query.Skip(skip).Take(limit);

			responseGroupPagination.Record = await query.Select(c => new ResponseGroup()
			{
				Id = c.Entity.GroupId,
				GroupName = c.Entity.GroupName,
				GroupDescription = c.Entity.GroupDescription,
				insertedByByFullName = c.InsertedByFullName,
				UpdatedByFullName = c.UpdatedByFullName,
				InsertedAt = c.Entity.InsertedAt,
				UpdatedAt = c.Entity.UpdatedAt,
				IsSystem = c.Entity.IsSystem
			}).ToListAsync();

			return responseGroupPagination;
		}

		public async Task<Group> MarkAsDeletedAsync(int grpId)
		{
			var entity = await context.Group.FirstOrDefaultAsync(x => x.GroupId == grpId);
			if (entity != null)
			{
				entity.IsActive = false;
				entity.UpdatedAt = DateTime.Now;
				entity.UpdatedBy = UserId;
				context.Update(entity);
				await context.SaveChangesAsync();
			}
			return entity;
		}

		public async Task<Group> MarkAsUnDeletedAsync(int grpId)
		{
			var entity = await context.Group.FirstOrDefaultAsync(x => x.GroupId == grpId);
			if (entity != null)
			{
				entity.IsActive = true;
				entity.UpdatedAt = DateTime.Now;
				entity.UpdatedBy = UserId;
				context.Update(entity);
				await context.SaveChangesAsync();
			}
			return entity;
		}

		public async Task<int> DeleteAsync(int id)
		{
			int result = 0;
			var entity = await context.User.FirstOrDefaultAsync(x => x.UserId == id);
			if (entity != null)
			{
				entity.IsDeleted = true;
				entity.DeletedAt = DateTime.Now;
				entity.DeletedBy = UserId;
				context.Update(entity);
				result += await context.SaveChangesAsync();
			}
			return result;
		}

		public async Task<bool> EmptyRecyclebin(int batchSize)
		{
			int result = 0;
			using var transaction = context.Database.BeginTransaction();
			{
				try
				{
					var recordsToDelete = await context.Group
						.Where(x => x.IsActive == false).ToListAsync();
					if (recordsToDelete.Count == 0)
					{
						return true;
					}

					for (int i = 0; i < recordsToDelete.Count; i += batchSize)
					{
						var batch = recordsToDelete.Skip(i).Take(batchSize).ToList();
						foreach (var grp in batch)
						{
							grp.IsDeleted = true;
							grp.DeletedBy = UserId;
							grp.DeletedAt = DateTime.Now;
							await context.SaveChangesAsync();
						}
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
			return result > 0;
		}

		public async Task<int> PurgeDeletedGroups(int batchSize, int purgingInMonth)
		{
			int deletedCount = 0;
			var cutoff = DateTime.Now.AddMonths(-purgingInMonth);
			var strategy = context.Database.CreateExecutionStrategy();

			await strategy.ExecuteAsync(async () =>
			{
				var baseQuery = context.Group
					.Where(g => g.IsDeleted == true && g.DeletedAt.HasValue && g.DeletedAt.Value <= cutoff)
					.OrderBy(g => g.GroupId);

				var total = await baseQuery.CountAsync();
				if (total == 0)
					return;

				var totalPages = (int)Math.Ceiling((double)total / batchSize);

				for (int page = 1; page <= totalPages; page++)
				{
					int skip = Skip(page, batchSize);
					var batch = await baseQuery.Skip(skip).Take(batchSize).ToListAsync();
					if (batch.Count == 0)
						continue;

					var groupIds = batch.Select(g => g.GroupId).ToList();

					using var transaction = await context.Database.BeginTransactionAsync();
					try
					{
						// 1) determine users that belong to these groups
						var userIdsInGroups = await context.UserGroup
							.Where(ug => groupIds.Contains(ug.GroupId))
							.Select(ug => ug.UserId)
							.Distinct()
							.ToListAsync();

						// 2) find users who will have NO groups left after these groups are removed
						var usersToRemove = new List<int>();
						foreach (var uid in userIdsInGroups)
						{
							var otherGroupCount = await context.UserGroup
								.Where(ug => ug.UserId == uid && !groupIds.Contains(ug.GroupId))
								.CountAsync();

							if (otherGroupCount == 0)
								usersToRemove.Add(uid);
						}

						// 3) delegate full user cleanup to UserRepository
						if (usersToRemove.Count > 0)
						{
							await _userRepository.PurgeUsersByIds(usersToRemove);
						}

						// 4) Remove user-group memberships for the groups being purged (remaining users)
						var userGroups = await context.UserGroup.Where(ug => groupIds.Contains(ug.GroupId)).ToListAsync();
						if (userGroups.Count > 0) context.UserGroup.RemoveRange(userGroups);

						// 5) Remove category shares referencing these groups
						var catShares = await context.CategoriesShared.Where(cs => cs.GroupID != null && groupIds.Contains(cs.GroupID.Value)).ToListAsync();
						if (catShares.Count > 0) context.CategoriesShared.RemoveRange(catShares);

						// 6) Remove document shared privileges referencing these groups
						var docSharePrivs = await context.DocumentSharedPrivillege.Where(p => p.GroupID != null && groupIds.Contains(p.GroupID.Value)).ToListAsync();
						if (docSharePrivs.Count > 0) context.DocumentSharedPrivillege.RemoveRange(docSharePrivs);

						// 7) Finally remove the group rows
						context.Group.RemoveRange(batch);

						// persist removals for this page
						await context.SaveChangesAsync();
						await transaction.CommitAsync();

						deletedCount += batch.Count;
					}
					catch
					{
						await transaction.RollbackAsync();
						throw;
					}
				}
			});

			return deletedCount;
		}
	}
}