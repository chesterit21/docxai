using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.DataAccess.Models.Systems;
using Api.Domain.Attributes;
using Api.Domain.EntityRequests.Authentications;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityResponses.Users;
using Api.Domain.Enum;
using Api.Extensions;
using MathNet.Numerics.Statistics.Mcmc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NPOI.POIFS.FileSystem;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Api.Repository.Masters
{
	public interface IUserRepository : IRepository<User>
	{
		Task<bool> CheckIfExist(List<int> userIds);
		Task<int> DeleteWithChildren(List<int> userIds);
		Task<User> GetUser(string username, string password);
		Task<User> GetUser(string username);
		Task<User> GetUserFromMailAddress(string mailAddress);
		Task<User> GetUser(int userId);
		Task<List<User>> GetUsers(int page, int limit);


		Task<bool> UpdatePassword(int userId, string password);
		Task<bool> VerifyEmail(int userId);
		Task<bool> CheckIfExist(int userId);
		Task<bool> SetLastLogin(int userId);
		Task<bool> SetIsLogin(int userId, bool isLogin);
		Task<bool> SetFullName(string username, string fullName);

		Task<List<DropdownUsers>> GetDropdownUsers();
		Task<List<DropdownUsersAndGroups>> GetDropdownUsersAndGroups();
		bool InsertUserRole(RequestNewUserCreate request);
		bool UpdateUserRole(RequestUpdateNewUser request);
		Task<bool> UpdateProfile(RequestUpdateUserInfo request);
		Task<bool> UploadProfile(IFormFile request);

		Task<ResponseUserPagination> GetUserList(string pUsername, bool isDelete, int page, int limit);
		Task<User> MarkAsDeletedAsync(int userId);
		Task<User> MarkAsUnDeletedAsync(int userId);
		Task<int> DeleteAsync(int id);
		Task<bool> EmptyRecyclebin(int batchSize);
		Task<int> SaveVerificationCode(int userId, string verificationCode);
		Task<bool> VerifiyResetPasswordCode(int userId, string verificationCode);
		Task<User> CrateNewUser(User user, List<int> groupIds, List<string> companyIds);
		Task<bool> PurgeDeletedUsers(int batchSize, int purgingInMonth);
		Task<int> PurgeUsersByIds(List<int> userIds);
		Task<Guid> SetLoginLog(string userAction, int userId);
	}

	public class UserRepository(DataContext context, IHttpContextAccessor accessor) : Repository<User>(context, accessor), IUserRepository
	{
		public async Task<List<User>> GetUsers(int page, int limit)
		{
			var skip = Skip(page, limit);

			var list = await context.User
			.Include(x => x.UserRoles)
				.ThenInclude(x => x.Role)
			//.Include(x => x.Company)
			.Include(x => x.UserCompanies)
				.ThenInclude(x => x.Company)
			.LeftJoin(context.User,
				left => left.InsertedBy,
				user => user.UserId,
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
				})
			//.Select(x => x.Entity) // Select only the User entity
			.Skip(skip)
			.Take(limit)
			.ToListAsync();

			return list.Select(x =>
			{
				x.Entity.InsertedByUserName = x.InsertedByUserName;
				x.Entity.InsertedByFullName = x.InsertedByFullName;
				x.Entity.UpdatedByUserName = x.UpdatedByUserName;
				x.Entity.UpdatedByFullName = x.UpdatedByFullName;
				return x.Entity;
			}).ToList();
			//return await context.User
			//    .Include(x => x.UserRoles)
			//    .ThenInclude(x => x.Group)
			//    .Include(x => x.Company)
			//    .Include(x => x.UserCompanies)
			//    .ThenInclude(x => x.Company)
			//    .Skip(skip).Take(limit).ToListAsync();

		}

		public async Task<User> GetUser(int userId)
		{
			return await context.User
				.Include(x => x.UserGroup)
					.ThenInclude(x => x.Group)
				.Include(x => x.UserMedia)
				.Include(x => x.Company)
					.Include(x => x.UserCompanies)
					.ThenInclude(x => x.Company)
				.FirstOrDefaultAsync(x => x.UserId == userId); // && x.IsActive == true
		}

		public async Task<User> GetUser(string username)
		{
			return await context.User
				 .Include(x => x.UserRoles)
					.ThenInclude(x => x.Role)
				 .Include(x => x.Company)
				 //.Include(x => x.UserCompanies)
				 //.ThenInclude(x => x.Company)
				 .FirstOrDefaultAsync(x => x.UserName == username && x.IsActive == true);
		}

		public async Task<User> GetUserFromMailAddress(string mailAddress)
		{
			return await context.User
				 .Include(x => x.UserRoles)
					.ThenInclude(x => x.Role)
				 .Include(x => x.Company)
				 .FirstOrDefaultAsync(x => x.EmailAddress == mailAddress && x.IsActive == true);
		}

		public async Task<User> GetUser(string username, string password)
		{
			return await context.User
				.Include(x => x.UserRoles)
				.ThenInclude(x => x.Role)
				.Include(x => x.Company)
				//.Include(x => x.UserCompanies)
				//.ThenInclude(x => x.Company)
				.FirstOrDefaultAsync(x => x.UserName == username && x.UserPassword == password && x.IsActive == true);
		}

		public async Task<bool> UpdatePassword(int userId, string password)
		{
			var entity = new User { UserId = userId, UserPassword = password, UpdatedAt = DateTime.Now };
			context.Attach(entity);
			context.Entry(entity).Property(x => x.UserPassword).IsModified = true;
			context.Entry(entity).Property(x => x.UpdatedAt).IsModified = true;

			return await context.SaveChangesAsync() > 0;
		}

		public async Task<bool> VerifyEmail(int userId)
		{
			var entity = new User { UserId = userId, EmailVerified = true, IsActive = true, UserStatus = UserStatus.Active.GetHashCode() };
			context.Attach(entity);
			context.Entry(entity).Property(x => x.EmailVerified).IsModified = true;
			context.Entry(entity).Property(x => x.IsActive).IsModified = true;
			context.Entry(entity).Property(x => x.UserStatus).IsModified = true;

			return await context.SaveChangesAsync() > 0;
		}

		public async Task<bool> CheckIfExist(List<int> userIds)
		{
			var count = await context.User.CountAsync(x => userIds.Contains(x.UserId));
			return count == userIds.Count;
		}

		public async Task<bool> CheckIfExist(int userId) => await context.User.AnyAsync(x => x.UserId == userId);

		public async Task<bool> SetLastLogin(int userId)
		{
			//context.Attach(entity);
			//context.Entry(entity).Property(x => x.EmailVerified).IsModified = true;
			//context.Entry(entity).Property(x => x.IsActive).IsModified = true;
			//context.Entry(entity).Property(x => x.UserStatus).IsModified = true;

			return await context.User.Where(x => x.UserId == userId)
				.ExecuteUpdateAsync(x => x.SetProperty(p => p.LastLogin, DateTime.Now).SetProperty(p => p.IsLogin, true)) > 0;
		}

		public async Task<bool> SetIsLogin(int userId, bool isLogin)
		{
			return await context.User.Where(x => x.UserId == userId).ExecuteUpdateAsync(x => x.SetProperty(p => p.IsLogin, isLogin)) > 0;
		}

		public async Task<bool> SetFullName(string username, string fullName)
		{
			return await context.User.Where(x => x.UserName == username).ExecuteUpdateAsync(x => x.SetProperty(p => p.FullName, fullName)) > 0;
		}

		public async Task<int> DeleteWithChildren(List<int> userIds)
		{
			await context.UserCompany.Where(x => userIds.Contains(x.UserId)).ExecuteDeleteAsync();
			await context.UserMatrix.Where(x => userIds.Contains(x.UserId)).ExecuteDeleteAsync();
			await context.UserRole.Where(x => userIds.Contains(x.UserId)).ExecuteDeleteAsync();
			return await context.User.Where(x => userIds.Contains(x.UserId)).ExecuteDeleteAsync();
		}

		public async Task<List<DropdownUsers>> GetDropdownUsers()
		{
			var users = await context.User.Where(x => x.UserId != UserId).Include(x => x.UserMedia).Select(x => new DropdownUsers { UserId = x.UserId, Photo = x.UserMedia.PhotoUrl, Email = x.EmailAddress, FullName = x.FullName }).ToListAsync();
			return users;
		}

		public async Task<List<DropdownUsersAndGroups>> GetDropdownUsersAndGroups()
		{
			var users = await context.User.Where(x => x.IsActive == true && x.UserId != UserId).Include(x => x.UserMedia)
				.Select(x => new DropdownUsersAndGroups
				{
					UserId = x.UserId,
					Photo = x.UserMedia.PhotoUrl,
					Email = x.EmailAddress,
					FullName = x.FullName,
					Type = "user",
					GroupId = 0,
					GroupName = "",
				}
				).ToListAsync();

			var groups = await context.Group.Where(x => x.IsActive == true)
				.Select(x => new DropdownUsersAndGroups
				{
					UserId = 0,
					Photo = "",
					Email = "",
					FullName = "",
					Type = "group",
					GroupId = x.GroupId,
					GroupName = x.GroupName,
				}
				).ToListAsync();

			var listunion = users.Union(groups);

			return listunion.ToList();

		}

		public bool InsertUserRole(RequestNewUserCreate request)
		{
			var executionStrategy = context.Database.CreateExecutionStrategy();
			var isSuccess = false;
			executionStrategy.Execute(
			() =>
			{
				using var transaction = context.Database.BeginTransaction();
				{
					try
					{
						var name = accessor?.HttpContext?.User?.Identity?.Name ?? "0";
						int userId = int.Parse(name);

						//insert user
						var user = new User
						{
							UserName = request.UserName,
							FullName = request.FullName,
							IsActive = request.IsActive,
							EmailAddress = request.EmailAddress,
							CompanyId = request.CompanyId,
							UserType = request.UserType,
							UserPassword = "P@ssw0rd".HashPassword(),
							InsertedBy = userId,
							InsertedAt = DateTime.Now,
							UpdatedBy = userId,
							UpdatedAt = DateTime.Now,
						};

						context.AddAsync<User>(user);
						context.SaveChanges();

						if (!string.IsNullOrEmpty(user.CompanyId))
						{
							//insert user company
							var userCompany = new UserCompany
							{
								UserId = user.UserId,
								CompanyId = user.CompanyId,
								InsertedBy = userId,
								InsertedAt = DateTime.Now,
								UpdatedBy = userId,
								UpdatedAt = DateTime.Now,
							};

							context.AddAsync<UserCompany>(userCompany);
							context.SaveChanges();
						}

						//insert user role
						foreach (var x in request.RolesId)
						{
							var userRole = new UserRole();
							userRole.UserId = user.UserId;
							userRole.RoleId = x;
							userRole.InsertedBy = userId;
							userRole.InsertedAt = DateTime.Now;
							userRole.UpdatedBy = userId;
							userRole.UpdatedAt = DateTime.Now;

							context.AddAsync<UserRole>(userRole);
							context.SaveChanges();
						}

						transaction.Commit();

						isSuccess = true;
					}
					catch (Exception ex)
					{
						transaction.Rollback();
						isSuccess = false;
						throw;

					}
				}
			});
			return isSuccess;
		}

		public bool UpdateUserRole(RequestUpdateNewUser request)
		{
			var executionStrategy = context.Database.CreateExecutionStrategy();
			var isSuccess = false;
			executionStrategy.Execute(
			() =>
			{
				using var transaction = context.Database.BeginTransaction();
				{
					try
					{
						var name = accessor?.HttpContext?.User?.Identity?.Name ?? "0";
						int userId = int.Parse(name);
						//update user
						var entityUser = context.User.AsNoTracking().FirstOrDefault(x => x.UserId == request.UserId);
						entityUser.UserName = request.UserName;
						entityUser.FullName = request.FullName;
						entityUser.IsActive = request.IsActive;
						entityUser.EmailAddress = request.EmailAddress;
						entityUser.CompanyId = request.CompanyId;
						entityUser.UpdatedBy = userId;
						entityUser.UpdatedAt = DateTime.Now;

						context.Update<User>(entityUser);
						context.SaveChanges();

						//update user company
						var userCompany = context.UserCompany.FirstOrDefault(x => x.UserId == request.UserId);
						userCompany.CompanyId = request.CompanyId;
						userCompany.UpdatedBy = userId;
						userCompany.UpdatedAt = DateTime.Now;
						context.Update<UserCompany>(userCompany);
						context.SaveChanges();

						//update user role
						var userRole = context.UserRole.Where(x => x.UserId == request.UserId);
						context.UserRole.RemoveRange(userRole);
						context.SaveChanges();

						foreach (var x in request.RolesId)
						{
							var entityUserRole = new UserRole();
							entityUserRole.UserId = entityUser.UserId;
							entityUserRole.RoleId = x;
							entityUserRole.InsertedBy = userId;
							entityUserRole.InsertedAt = DateTime.Now;
							entityUserRole.UpdatedBy = userId;
							entityUserRole.UpdatedAt = DateTime.Now;

							context.Add<UserRole>(entityUserRole);
							context.SaveChanges();
						}

						transaction.Commit();
						isSuccess = true;
					}

					catch (Exception ex)
					{
						transaction.Rollback();
						isSuccess = false;
						throw;
					}
				}
			});
			return isSuccess;
		}
		public async Task<bool> UpdateProfile(RequestUpdateUserInfo request)
		{
			int countSuccess = 0;
			var entity = await context.User.FirstOrDefaultAsync(x => x.UserId == request.UserId);
			if (!string.IsNullOrEmpty(request.UserPassword))
			{
				entity.UserPassword = Encryption.HashPassword(request.UserPassword);
			}

			entity.FullName = request.FullName;
			entity.EmailAddress = request.EmailAddress;
			entity.PhoneNumber = request.PhoneNumber;
			entity.UpdatedAt = DateTime.Now;
			entity.UpdatedBy = UserId;
			context.Update<User>(entity);
			countSuccess = await context.SaveChangesAsync();

			//UserMedia userMedia = new UserMedia();
			//userMedia.DateFormat = requestfile.DateFormat;
			//context.Update<UserMedia>(userMedia);
			//countSuccess += await context.SaveChangesAsync();

			return countSuccess > 0;
		}

		public async Task<bool> UploadProfile(IFormFile requestfile)
		{
			var userMedia = await context.UserMedia.FirstOrDefaultAsync(x => x.UserId == UserId);
			//var base64String = string.Empty;
			int result = 0;
			await using var memoryStream = new MemoryStream();
			if (requestfile.Length > 0)
			{
				string userPpFolder = Path.Combine(AppContext.BaseDirectory, "user_pp");

				string fileName = requestfile.FileName;
				string fileExtension = Path.GetExtension(fileName);

				string guidFilenameNext = Guid.NewGuid().ToString() + fileExtension;
				string userPpfilename = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "user_pp", guidFilenameNext);

				await requestfile.CopyToAsync(memoryStream);
				//base64String = Convert.ToBase64String(memoryStream.ToArray());
				string photoUrl = $"/profilep/{guidFilenameNext}";
				using (var stream = new FileStream(userPpfilename, FileMode.Create))
				{
					await requestfile.CopyToAsync(stream);
				}

				if (userMedia == null)
				{
					var model = new UserMedia()
					{
						UserId = UserId,
						PhotoName = requestfile.FileName,
						PhotoImage = memoryStream.ToArray(),
						InsertedBy = UserId,
						InsertedAt = DateTime.Now,
						UpdatedBy = UserId,
						UpdatedAt = DateTime.Now,
						PhotoUrl = photoUrl
					};

					await context.AddAsync<UserMedia>(model);
					result += await context.SaveChangesAsync();
				}
				else
				{
					if (File.Exists(userMedia.PhotoUrl))
					{
						File.Delete(userMedia.PhotoUrl);
					}
					userMedia.PhotoName = requestfile.FileName;
					userMedia.PhotoImage = memoryStream.ToArray();
					userMedia.UpdatedBy = UserId;
					userMedia.UpdatedAt = DateTime.Now;
					userMedia.PhotoUrl = photoUrl;
					context.Update<UserMedia>(userMedia);
					result += await context.SaveChangesAsync();
				}
				//return base64String;
			}

			return result > 0;
		}

		private string GeneratePassword(int length)
		{
			const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
			StringBuilder res = new StringBuilder();
			Random rnd = new Random();
			while (0 < length--)
			{
				res.Append(valid[rnd.Next(valid.Length)]);
			}
			return res.ToString();
		}

		public async Task<ResponseUserPagination> GetUserList(string pUsername, bool isDelete, int page, int limit)
		{
			var skip = Skip(page, limit);
			ResponseUserPagination PaginatedUsers = new ResponseUserPagination();

			var query = context.User
			.WhereIf(isDelete, x => x.UserStatus == UserStatus.RecycleBin.GetHashCode())
			.WhereIf(!isDelete, x => x.UserStatus < UserStatus.RecycleBin.GetHashCode())
			.Include(x => x.UserGroup)
				.ThenInclude(x => x.Group)
			.Include(x => x.UserCompanies)
				.ThenInclude(x => x.Company)
			.LeftJoin(context.User,
				left => left.InsertedBy,
				user => user.UserId,
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
				})
			.Select(x => new ResponseUserList()
			{
				UserId = x.Entity.UserId,
				UserName = x.Entity.UserName,
				EmailAddress = x.Entity.EmailAddress,
				PhoneNumber = x.Entity.PhoneNumber,
				FullName = x.Entity.FullName,
				UserType = x.Entity.UserType,
				IsActive = x.Entity.IsActive,
				IsDeleted = x.Entity.IsDeleted,
				InsertedAt = x.Entity.InsertedAt,
				UpdatedAt = x.Entity.UpdatedAt,
				CompanyId = x.Entity.CompanyId,
				Groups = x.Entity.UserGroup.Where(c => c.Group.GroupName.ToLower() != "everyone").Select(g => new ResponseUserList.Groupr() { GroupId = g.GroupId, GroupName = g.Group.GroupName }).ToList(),
				InsertedByFullName = x.Entity.InsertedByFullName,
				UpdatedByFullName = x.Entity.UpdatedByFullName,
				UserStatus = x.Entity.EmailVerified != true ? "Waiting For Verification" : (x.Entity.IsActive ? "Active" : "Inactive"),
				LastLogin = x.Entity.LastLogin,
				//Companies = x.Entity.UserCompanies.Select(g => new ResponseUserList.Company { CompanyId = g.CompanyId, CompanyName = g.Company.Name }).FirstOrDefault()
			})
			//.Where(x => x.IsDeleted == isDelete)
			.WhereIf(!string.IsNullOrEmpty(pUsername), x => (x.UserName.ToLower().Contains(pUsername.ToLower()) || x.FullName.ToLower().Contains(pUsername.ToLower())));

			PaginatedUsers.TotalRecord = query.Count();

			if (page > 0 && limit > 0)
				query = query.Skip(skip).Take(limit);

			PaginatedUsers.UsersRecord = await query.ToListAsync();

			return PaginatedUsers;
		}

		public async Task<User> MarkAsDeletedAsync(int userId)
		{
			var entity = await context.User.FirstOrDefaultAsync(x => x.UserId == userId);
			if (entity != null)
			{
				entity.IsActive = false;
				entity.UserStatus = UserStatus.RecycleBin.GetHashCode();
				//entity.IsDeleted = true;
				entity.UpdatedAt = DateTime.Now;
				entity.UpdatedBy = UserId;
				context.Update(entity);
				await context.SaveChangesAsync();
			}
			return entity;
		}

		public async Task<User> MarkAsUnDeletedAsync(int userId)
		{
			var entity = await context.User.FirstOrDefaultAsync(x => x.UserId == userId);
			if (entity != null)
			{
				entity.IsActive = true;
				entity.UserStatus = UserStatus.Active.GetHashCode();
				//entity.IsDeleted = false;
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
				entity.UserStatus = UserStatus.Deleted.GetHashCode();
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
			var strategy = context.Database.CreateExecutionStrategy();
			await strategy.ExecuteAsync(async () =>
			{
				using var transaction = context.Database.BeginTransaction();
				{
					try
					{
						var recordsToDelete = await context.User.AsNoTracking()
							.Where(x => x.UserStatus == UserStatus.RecycleBin.GetHashCode()).ToListAsync();
						if (recordsToDelete.Count == 0)
						{
							result = 0;
						}
						else
						{
							int totalEmptyRecycleUser = (int)Math.Ceiling((double)recordsToDelete.Count / batchSize);
							for (int i = 0; i < totalEmptyRecycleUser; i += batchSize)
							{
								int skip = Skip(i / batchSize + 1, batchSize);
								var batch = recordsToDelete.Skip(skip).Take(batchSize).ToList();
								foreach (var usr in batch)
								{
									List<Categories> myCats = await context.Categories.Where(x => x.Owner == usr.UserId).ToListAsync();
									foreach (var cat in myCats)
									{
										//string baseDir = AppDomain.CurrentDomain.BaseDirectory;
										//var categoryPathFolder = Path.Combine(baseDir, "upload", $"{cat.Id}-{cat.CategoryName.Trim()}");
										//var uploadPath = Path.Combine(baseDir, categoryPathFolder);
										//if (Directory.Exists(uploadPath))
										//{
										//	Directory.Delete(uploadPath, true);
										//}

										int categoryID = cat.Id;
										var approval = await context.Approvals.AsNoTracking().FirstOrDefaultAsync(x => x.CategoryID == categoryID && x.IsDeleted != true);
										if (approval != null)
										{
											var appActv = await context.ApprovalActivities.AsNoTracking().Where(x => x.ApprovalID == approval.Id && x.IsDeleted != true).ToListAsync();
											foreach (var act in appActv)
											{
												act.IsDeleted = true;
												act.DeletedBy = UserId;
												act.DeletedAt = DateTime.Now;
											}
											context.ApprovalActivities.UpdateRange(appActv);
											await context.SaveChangesAsync();

											var appFlows = await context.ApprovalFlows.AsNoTracking().Where(x => x.ApprovalID == approval.Id && x.IsDeleted != true).ToListAsync();
											foreach (var act in appFlows)
											{
												act.IsDeleted = true;
												act.DeletedBy = UserId;
												act.DeletedAt = DateTime.Now;
											}
											context.ApprovalFlows.UpdateRange(appFlows);
											await context.SaveChangesAsync();

											approval.IsDeleted = true;
											approval.DeletedBy = UserId;
											approval.DeletedAt = DateTime.Now;
											context.Approvals.Update(approval);
											await context.SaveChangesAsync();
										}

										var favs = await context.CategoriesFavorites.AsNoTracking().Where(x => (x.CategoryID == categoryID || x.Owner == UserId) && x.IsDeleted != true).ToListAsync();
										if (favs.Count > 0)
										{
											foreach (var act in favs)
											{
												act.IsDeleted = true;
												act.DeletedBy = UserId;
												act.DeletedAt = DateTime.Now;
											}
											context.CategoriesFavorites.UpdateRange(favs);
											await context.SaveChangesAsync();
										}

										var shared = await context.CategoriesShared.AsNoTracking().Where(x => x.CategoryID == categoryID && x.IsDeleted != true).ToListAsync();
										if (shared.Count > 0)
										{
											foreach (var share in shared)
											{
												share.IsDeleted = true;
												share.DeletedBy = UserId;
												share.DeletedAt = DateTime.Now;
											}
											context.CategoriesShared.UpdateRange(shared);
											await context.SaveChangesAsync();
										}

										//Documets in category
										var documentsToDelete = await context.Documents.AsNoTracking().Where(x => x.CategoryID == categoryID && x.IsDeleted != true).ToListAsync();
										for (int k = 0; k < documentsToDelete.Count; k += batchSize)
										{
											var batchdocument = documentsToDelete.Skip(i).Take(batchSize).ToList();
											int batchDocCount = batchdocument.Count();
											for (int x = 0; x < batchDocCount; x++)
											{
												int idDocument = batchdocument[x].Id;

												var dlogs = await context.DocumentLog.AsNoTracking().Where(x => x.DocumentID == idDocument).ToListAsync();
												if (dlogs.Count > 0)
												{
													foreach (var log in dlogs)
													{
														log.IsDeleted = true;
														log.DeletedBy = UserId;
														log.DeletedAt = DateTime.Now;
													}
													context.DocumentLog.UpdateRange(dlogs);
													result += await context.SaveChangesAsync();
												}

												var approvalDC = await context.Approvals.AsNoTracking().FirstOrDefaultAsync(x => x.DocumentID == idDocument && x.CategoryID != null);
												if (approvalDC != null)
												{
													var activities = await context.ApprovalActivities.AsNoTracking().Where(x => x.ApprovalID == approvalDC.Id).ToListAsync();
													if (activities.Count > 0)
													{
														foreach (var actAppDoc in activities)
														{
															actAppDoc.IsDeleted = true;
															actAppDoc.DeletedBy = UserId;
															actAppDoc.DeletedAt = DateTime.Now;
														}
														context.ApprovalActivities.UpdateRange(activities);
														result += await context.SaveChangesAsync();
													}

													var flows = await context.ApprovalFlows.AsNoTracking().Where(x => x.ApprovalID == approvalDC.Id).ToListAsync();
													if (flows.Count > 0)
													{
														foreach (var flw in flows)
														{
															flw.IsDeleted = true;
															flw.DeletedBy = UserId;
															flw.DeletedAt = DateTime.Now;
														}
														context.ApprovalFlows.UpdateRange(flows);
														result += await context.SaveChangesAsync();
													}

													approvalDC.IsDeleted = true;
													approvalDC.DeletedBy = UserId;
													approvalDC.DeletedAt = DateTime.Now;
													context.Approvals.UpdateRange(approvalDC);
													result += await context.SaveChangesAsync();
												}


												var docAttr = await context.DocumentAttributes.AsNoTracking().Where(x => x.DocumentID == idDocument).ToListAsync();
												if (docAttr.Count > 0)
												{
													foreach (var dcAttr in docAttr)
													{
														dcAttr.IsDeleted = true;
														dcAttr.DeletedBy = UserId;
														dcAttr.DeletedAt = DateTime.Now;
													}
												}
												context.DocumentAttributes.UpdateRange(docAttr);
												result += context.SaveChanges();

												var docFavs = await context.DocumentFavorites.AsNoTracking().Where(x => x.DocumentId == idDocument).ToListAsync();
												if (docFavs.Count > 0)
												{
													foreach (var dcFavs in docFavs)
													{
														dcFavs.IsDeleted = true;
														dcFavs.DeletedBy = UserId;
														dcFavs.DeletedAt = DateTime.Now;
													}

													context.DocumentFavorites.UpdateRange(docFavs);
													result += context.SaveChanges();
												}												

												var docFiles = await context.DocumentFiles.AsNoTracking().Where(x => x.DocumentID == idDocument).Include(x => x.InsertedByUser).ToListAsync();
												if (docFiles.Count > 0)
												{
													foreach (var fl in docFiles)
													{
														//var theFile = DocumentFilesHelper.GetResourcePathForDocumentFile(fl.InsertedBy, fl.InsertedByUser.UserName, categoryID, cat.CategoryName.Trim(), fl.NewDocumentFileName);
														//if (File.Exists(theFile))
														//{
														//	File.Delete(theFile);
														//}
														fl.IsDeleted = true;
														fl.DeletedBy = UserId;
														fl.DeletedAt = DateTime.Now;
													}

													context.DocumentFiles.UpdateRange(docFiles);
													result += context.SaveChanges();
												}												

												var docItemList = await context.DocumentItemList.AsNoTracking().Where(x => x.DocumentID == idDocument).ToListAsync();
												if (docItemList.Count > 0)
												{
													foreach (var dcItemList in docItemList)
													{
														dcItemList.IsDeleted = true;
														dcItemList.DeletedBy = UserId;
														dcItemList.DeletedAt = DateTime.Now;
													}

													context.DocumentItemList.UpdateRange(docItemList);
													result += context.SaveChanges();
												}												

												var docRelated = await context.RelatedDocuments.AsNoTracking().Where(x => x.DocumentID == idDocument).ToListAsync();
												if (docRelated.Count > 0)
												{
													foreach (var dcRelate in docRelated)
													{
														dcRelate.IsDeleted = true;
														dcRelate.DeletedBy = UserId;
														dcRelate.DeletedAt = DateTime.Now;
													}

													context.RelatedDocuments.UpdateRange(docRelated);
													result += context.SaveChanges();
												}

												var docReminders = await context.DocumentReminders.AsNoTracking().Where(x => x.DocumentID == idDocument).ToListAsync();
												if(docReminders.Count > 0)
												{
													foreach (var dcRmdrs in docReminders)
													{
														dcRmdrs.IsDeleted = true;
														dcRmdrs.DeletedBy = UserId;
														dcRmdrs.DeletedAt = DateTime.Now;
													}

													context.DocumentReminders.UpdateRange(docReminders);
													result += context.SaveChanges();
												}												

												var docShared = await context.DocumentShared.AsNoTracking().FirstOrDefaultAsync(x => x.DocumentID == idDocument);
												if (docShared != null)
												{
													var sharePriv = await context.DocumentSharedPrivillege.AsNoTracking().Where(x => x.DocumentSharedID == docShared.Id).ToListAsync();
													if (sharePriv.Count > 0)
													{
														foreach (var dcSharePRiv in sharePriv)
														{
															dcSharePRiv.IsActive = false;
															dcSharePRiv.IsDeleted = true;
															dcSharePRiv.DeletedBy = UserId;
															dcSharePRiv.DeletedAt = DateTime.Now;
														}

														context.DocumentSharedPrivillege.UpdateRange(sharePriv);
														result += context.SaveChanges();
													}

													docShared.IsActive = false;
													docShared.IsDeleted = true;
													docShared.DeletedBy = UserId;
													docShared.DeletedAt = DateTime.Now;
													context.DocumentShared.Update(docShared);
													result += context.SaveChanges();
												}

												Documents docx = batchdocument[x];
												docx.IsActive = false;
												docx.IsDeleted = true;
												docx.DeletedBy = UserId;
												docx.DeletedAt = DateTime.Now;
												context.Documents.Update(docx);
												result += context.SaveChanges();
											}
										}


										//Categories category = batchSize > 1 ? cat : new Categories()
										//{
										//	Id = categoryID,
										//	IsActive = false,
										//	IsDeleted = true,
										//	DeletedBy = UserId,
										//	DeletedAt = DateTime.Now
										//};
										cat.IsActive = false;
										cat.IsDeleted = true;
										cat.DeletedBy = UserId;
										cat.DeletedAt = DateTime.Now;
										context.Categories.Update(cat);
										await context.SaveChangesAsync();

									}

									usr.IsDeleted = true;
									usr.UserStatus = UserStatus.Deleted.GetHashCode();
									usr.DeletedBy = UserId;
									usr.DeletedAt = DateTime.Now;
									await context.SaveChangesAsync();
								}
								result += context.SaveChanges();
							}

							transaction.Commit();
						}						
					}
					catch (Exception ex)
					{
						transaction.Rollback();
						throw;
					}
				}
			});
			return result > 0;
		}

		public async Task<int> SaveVerificationCode(int userId, string verificationCode)
		{
			int countInsert = 0;
			var entity = await context.ResetPasswordUserVerificationCode.FirstOrDefaultAsync(x => x.UserId == userId);
			if (entity != null)
			{
				entity.VerificationCode = verificationCode;
				entity.InsertedAt = DateTime.Now;
				entity.InsertedBy = userId;
				context.Update(entity);
				countInsert = await context.SaveChangesAsync();
			}
			else
			{
				ResetPasswordUserVerificationCode vcode = new ResetPasswordUserVerificationCode()
				{
					UserId = userId,
					VerificationCode = verificationCode,
					InsertedAt = DateTime.Now,
					InsertedBy = userId
				};
				await context.AddAsync(vcode);
				countInsert = await context.SaveChangesAsync();
			}
			return countInsert;
		}

		public async Task<bool> VerifiyResetPasswordCode(int userId, string verificationCode)
		{
			//int countResult = 0;
			var entity = await context.ResetPasswordUserVerificationCode.FirstOrDefaultAsync(x => x.UserId == userId);
			if (entity != null)
			{
				if (entity.VerificationCode == verificationCode)
				{
					return true;
					//context.Remove(entity);
					//countResult = await context.SaveChangesAsync();
				}
			}
			return false;
		}

		public async Task<User> CrateNewUser(User user, List<int> groupIds, List<string> companyIds)
		{
			var strategy = context.Database.CreateExecutionStrategy();
			await strategy.ExecuteAsync(async () =>
			{
				using var transaction = context.Database.BeginTransaction();
				{
					try
					{
						user.InsertedBy = UserId;
						user.InsertedAt = DateTime.Now;
						user.UpdatedBy = UserId;
						user.UpdatedAt = DateTime.Now;

						await context.AddAsync<User>(user);
						context.SaveChanges();

						//added user to group everyone
						var everyoneGroup = await context.Group.FirstOrDefaultAsync(x => x.GroupName.ToLower() == "everyone");
						groupIds.Add(everyoneGroup.GroupId);

						if (groupIds.Any())
						{
							foreach (var groupId in groupIds)
							{
								var userGroup = new UserGroup
								{
									UserId = user.UserId,
									GroupId = groupId,
									InsertedBy = UserId,
									InsertedAt = DateTime.Now,
									UpdatedBy = UserId,
									UpdatedAt = DateTime.Now
								};
								await context.AddAsync<UserGroup>(userGroup);
								context.SaveChanges();
							}
						}

						if (companyIds.Any())
						{
							foreach (var companyId in companyIds)
							{
								var userCompany = new UserCompany
								{
									UserId = user.UserId,
									CompanyId = companyId,
									InsertedBy = UserId,
									InsertedAt = DateTime.Now,
									UpdatedBy = UserId,
									UpdatedAt = DateTime.Now
								};
								await context.AddAsync<UserCompany>(userCompany);
								context.SaveChanges();
							}
						}

						//DateTime dateNow = DateTime.Now;
						//UserRole userRole = new UserRole()
						//{
						//	UserId = user.UserId,
						//	RoleId = 2, //user							
						//	InsertedBy = UserId,
						//	InsertedAt = dateNow,
						//	UpdatedBy = UserId,
						//	UpdatedAt = dateNow
						//};
						//await context.AddAsync<UserRole>(userRole);
						//context.SaveChanges();

						string baseDir = AppDomain.CurrentDomain.BaseDirectory;

						DateTime dateNow2 = DateTime.Now;
						Categories myDocuments = new Categories()
						{
							Owner = user.UserId,
							ParentId = null,
							IsActive = true,
							CategoryName = "My Documents",
							InsertedBy = UserId,
							InsertedAt = dateNow2,
							UpdatedBy = UserId,
							UpdatedAt = dateNow2
						};

						await context.AddAsync<Categories>(myDocuments);
						context.SaveChanges();

						//create directory my document for user 
						string dirCategoryPath = DocumentFilesHelper.GetPhysicalPathForCategory(UserId, UserName, myDocuments.Id, myDocuments.CategoryName.Trim(), true);

						transaction.Commit();
					}
					catch (Exception ex)
					{
						transaction.Rollback();
						throw;

					}
				}
			});
			return user;
		}

		public async Task<bool> PurgeDeletedUsers(int batchSize, int purgingInMonth)
		{
			int result = 0;
			var strategy = context.Database.CreateExecutionStrategy();

			await strategy.ExecuteAsync(async () =>
			{
				var cutoff = DateTime.Now.AddMonths(-purgingInMonth);

				// deferred query for users eligible for permanent deletion
				//var baseQuery = context.User
				//	.Where(u => u.IsDeleted == true && u.DeletedAt.HasValue && u.DeletedAt.Value <= cutoff)
				//	.OrderBy(u => u.UserId);
				var baseQuery = context.User
					.Where(u => u.UserStatus == UserStatus.Deleted.GetHashCode() && u.DeletedAt.HasValue && u.DeletedAt.Value <= cutoff)
					.OrderBy(u => u.UserId);

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

					using var transaction = await context.Database.BeginTransactionAsync();
					try
					{
						foreach (var usr in batch)
						{
							// ---- Documents owned by user ----
							var userDocs = await context.Documents.Where(d => d.Owner == usr.UserId).ToListAsync();
							foreach (var doc in userDocs)
							{
								int idDocument = doc.Id;

								// Document logs
								var dlogs = await context.DocumentLog.Where(x => x.DocumentID == idDocument).ToListAsync();
								if (dlogs.Count > 0) context.DocumentLog.RemoveRange(dlogs);

								// Approvals related to this document
								var approvalDC = await context.Approvals.Where(x => x.DocumentID == idDocument).ToListAsync();
								if (approvalDC.Count > 0)
								{
									var approvalIds = approvalDC.Select(a => a.Id).ToList();

									var activities = await context.ApprovalActivities.Where(a => approvalIds.Contains(a.ApprovalID)).ToListAsync();
									if (activities.Count > 0) context.ApprovalActivities.RemoveRange(activities);

									var flows = await context.ApprovalFlows.Where(f => approvalIds.Contains(f.ApprovalID)).ToListAsync();
									if (flows.Count > 0) context.ApprovalFlows.RemoveRange(flows);

									context.Approvals.RemoveRange(approvalDC);
								}

								// Document attributes
								var docAttr = await context.DocumentAttributes.Where(x => x.DocumentID == idDocument).ToListAsync();
								if (docAttr.Count > 0) context.DocumentAttributes.RemoveRange(docAttr);

								// Document favorites
								var docFavs = await context.DocumentFavorites.Where(x => x.DocumentId == idDocument).ToListAsync();
								if (docFavs.Count > 0) context.DocumentFavorites.RemoveRange(docFavs);

								// Document files (delete physical files then DB rows)
								var docFiles = await context.DocumentFiles.Where(x => x.DocumentID == idDocument).ToListAsync();
								if (docFiles.Count > 0)
								{
									try
									{
										var model = await context.Categories.FirstOrDefaultAsync(y => y.Id == doc.CategoryID);
										var uploadPath = DocumentFilesHelper.GetPhysicalPathForCategory(usr.UserId, usr.UserName, model.Id, model.CategoryName);
										foreach (var fl in docFiles)
										{
											var theFile = Path.Combine(uploadPath, fl.NewDocumentFileName);
											if (!string.IsNullOrEmpty(fl.NewDocumentFileName) && File.Exists(theFile))
											{
												try { File.Delete(theFile); } catch { /* ignore file delete errors */ }
											}
										}
									}
									catch { /* ignore FS errors */ }

									context.DocumentFiles.RemoveRange(docFiles);
								}

								// Document item list
								var docItemList = await context.DocumentItemList.Where(x => x.DocumentID == idDocument).ToListAsync();
								if (docItemList.Count > 0) context.DocumentItemList.RemoveRange(docItemList);

								// Related documents
								var docRelated = await context.RelatedDocuments.Where(x => x.DocumentID == idDocument || x.RelatedDocumentID == idDocument).ToListAsync();
								if (docRelated.Count > 0) context.RelatedDocuments.RemoveRange(docRelated);

								// Document reminders
								var docReminders = await context.DocumentReminders.Where(x => x.DocumentID == idDocument).ToListAsync();
								if (docReminders.Count > 0) context.DocumentReminders.RemoveRange(docReminders);

								// Document shared and privileges
								var docShared = await context.DocumentShared.FirstOrDefaultAsync(x => x.DocumentID == idDocument);
								if (docShared != null)
								{
									var sharePriv = await context.DocumentSharedPrivillege.Where(x => x.DocumentSharedID == docShared.Id).ToListAsync();
									if (sharePriv.Count > 0) context.DocumentSharedPrivillege.RemoveRange(sharePriv);
									context.DocumentShared.Remove(docShared);
								}

								// Notifications for this document
								var notifs = await context.Notifications.Where(x => x.DocumentID == idDocument).ToListAsync();
								if (notifs.Count > 0) context.Notifications.RemoveRange(notifs);

								// Finally remove the document row
								context.Documents.Remove(doc);

								await context.SaveChangesAsync();
								result++;
							} // end documents

							// ---- Categories owned by user ----
							var myCats = await context.Categories.Where(x => x.Owner == usr.UserId).ToListAsync();
							foreach (var cat in myCats)
							{
								int categoryID = cat.Id;

								// Approvals for category (category-level approvals)
								var approval = await context.Approvals.Where(x => x.CategoryID == categoryID && x.DocumentID == null).ToListAsync();
								if (approval.Count > 0)
								{
									var approvalIds = approval.Select(a => a.Id).ToList();

									var appActv = await context.ApprovalActivities.Where(x => approvalIds.Contains(x.ApprovalID)).ToListAsync();
									if (appActv.Count > 0) context.ApprovalActivities.RemoveRange(appActv);

									var appFlows = await context.ApprovalFlows.Where(x => approvalIds.Contains(x.ApprovalID)).ToListAsync();
									if (appFlows.Count > 0) context.ApprovalFlows.RemoveRange(appFlows);

									context.Approvals.RemoveRange(approval);
								}

								// Category favorites
								var favs = await context.CategoriesFavorites.Where(x => x.CategoryID == categoryID).ToListAsync();
								if (favs.Count > 0) context.CategoriesFavorites.RemoveRange(favs);

								// Category shares
								var shared = await context.CategoriesShared.Where(x => x.CategoryID == categoryID).ToListAsync();
								if (shared.Count > 0) context.CategoriesShared.RemoveRange(shared);

								// Documents in this category (ensure cleanup)
								var documentsInCategory = await context.Documents.Where(x => x.CategoryID == categoryID).ToListAsync();
								foreach (var doc in documentsInCategory)
								{
									var idDocument = doc.Id;

									var dlogs = await context.DocumentLog.Where(x => x.DocumentID == idDocument).ToListAsync();
									if (dlogs.Count > 0) context.DocumentLog.RemoveRange(dlogs);

									var approvalDC = await context.Approvals.Where(x => x.DocumentID == idDocument).ToListAsync();
									if (approvalDC.Count > 0)
									{
										var approvalIds = approvalDC.Select(a => a.Id).ToList();

										var activities = await context.ApprovalActivities.Where(a => approvalIds.Contains(a.ApprovalID)).ToListAsync();
										if (activities.Count > 0) context.ApprovalActivities.RemoveRange(activities);

										var flows = await context.ApprovalFlows.Where(f => approvalIds.Contains(f.ApprovalID)).ToListAsync();
										if (flows.Count > 0) context.ApprovalFlows.RemoveRange(flows);

										context.Approvals.RemoveRange(approvalDC);
									}

									var docAttr = await context.DocumentAttributes.Where(x => x.DocumentID == idDocument).ToListAsync();
									if (docAttr.Count > 0) context.DocumentAttributes.RemoveRange(docAttr);

									var docFavs = await context.DocumentFavorites.Where(x => x.DocumentId == idDocument).ToListAsync();
									if (docFavs.Count > 0) context.DocumentFavorites.RemoveRange(docFavs);

									var docFiles = await context.DocumentFiles.Where(x => x.DocumentID == idDocument).ToListAsync();
									if (docFiles.Count > 0)
									{
										try
										{
											var model = await context.Categories.FirstOrDefaultAsync(y => y.Id == doc.CategoryID);
											var uploadPath = DocumentFilesHelper.GetPhysicalPathForCategory(usr.UserId, usr.UserName, model.Id, model.CategoryName);

											foreach (var fl in docFiles)
											{
												var theFile = Path.Combine(uploadPath, fl.NewDocumentFileName);
												if (!string.IsNullOrEmpty(fl.NewDocumentFileName) && File.Exists(theFile))
												{
													try { File.Delete(theFile); } catch { /* ignore */ }
												}
											}
										}
										catch { /* ignore */ }

										context.DocumentFiles.RemoveRange(docFiles);
									}

									var docItemList = await context.DocumentItemList.Where(x => x.DocumentID == idDocument).ToListAsync();
									if (docItemList.Count > 0) context.DocumentItemList.RemoveRange(docItemList);

									var docRelated = await context.RelatedDocuments.Where(x => x.DocumentID == idDocument || x.RelatedDocumentID == idDocument).ToListAsync();
									if (docRelated.Count > 0) context.RelatedDocuments.RemoveRange(docRelated);

									var docReminders = await context.DocumentReminders.Where(x => x.DocumentID == idDocument).ToListAsync();
									if (docReminders.Count > 0) context.DocumentReminders.RemoveRange(docReminders);

									var docShared = await context.DocumentShared.FirstOrDefaultAsync(x => x.DocumentID == idDocument);
									if (docShared != null)
									{
										var sharePriv = await context.DocumentSharedPrivillege.Where(x => x.DocumentSharedID == docShared.Id).ToListAsync();
										if (sharePriv.Count > 0) context.DocumentSharedPrivillege.RemoveRange(sharePriv);
										context.DocumentShared.Remove(docShared);
									}

									var notifs = await context.Notifications.Where(x => x.DocumentID == idDocument).ToListAsync();
									if (notifs.Count > 0) context.Notifications.RemoveRange(notifs);

									context.Documents.Remove(doc);
								}

								// Finally remove the category
								context.Categories.Remove(cat);

								await context.SaveChangesAsync();
								result++;
							} // end categories

							// ---- Remove user related reference tables ----
							var userCompanies = await context.UserCompany.Where(x => x.UserId == usr.UserId).ToListAsync();
							if (userCompanies.Count > 0) context.UserCompany.RemoveRange(userCompanies);

							var userMatrices = await context.UserMatrix.Where(x => x.UserId == usr.UserId).ToListAsync();
							if (userMatrices.Count > 0) context.UserMatrix.RemoveRange(userMatrices);

							var userRoles = await context.UserRole.Where(x => x.UserId == usr.UserId).ToListAsync();
							if (userRoles.Count > 0) context.UserRole.RemoveRange(userRoles);

							var userGroups = await context.UserGroup.Where(x => x.UserId == usr.UserId).ToListAsync();
							if (userGroups.Count > 0) context.UserGroup.RemoveRange(userGroups);

							var userMedia = await context.UserMedia.Where(x => x.UserId == usr.UserId).ToListAsync();
							if (userMedia.Count > 0)
							{
								foreach (var um in userMedia)
								{
									try
									{
										if (!string.IsNullOrEmpty(um.PhotoUrl))
										{
											var path = Path.Combine(AppContext.BaseDirectory, um.PhotoUrl.TrimStart('/').Replace("profilep", "user_pp").Replace('/', Path.DirectorySeparatorChar));
											if (File.Exists(path))
											{
												try { File.Delete(path); } catch { /* ignore */ }
											}
										}
									}
									catch { /* ignore */ }
								}
								context.UserMedia.RemoveRange(userMedia);
							}

							var resetCodes = await context.ResetPasswordUserVerificationCode.Where(x => x.UserId == usr.UserId).ToListAsync();
							if (resetCodes.Count > 0) context.ResetPasswordUserVerificationCode.RemoveRange(resetCodes);

							var userNotifications = await context.Notifications.Where(n => n.TargetActor == usr.UserId).ToListAsync();
							if (userNotifications.Count > 0) context.Notifications.RemoveRange(userNotifications);

							// Finally remove the user itself
							context.User.Remove(usr);

							await context.SaveChangesAsync();
						} // end user batch

						await transaction.CommitAsync();
					}
					catch (Exception)
					{
						await transaction.RollbackAsync();
						throw;
					}
				}
			});

			return result > 0;
		}


		public async Task<int> PurgeUsersByIds(List<int> userIds)
		{
			int removedCount = 0;
			if (userIds == null || userIds.Count == 0)
				return 0;

			var strategy = context.Database.CreateExecutionStrategy();

			await strategy.ExecuteAsync(async () =>
			{
				// process users one-by-one to keep transactions small and isolated
				foreach (var usrId in userIds.Distinct())
				{
					using var transaction = await context.Database.BeginTransactionAsync();
					try
					{
						// ---- Documents owned by user ----
						var userDocs = await context.Documents.Where(d => d.Owner == usrId).Include(x => x.OwnerInfo).ToListAsync();
						foreach (var doc in userDocs)
						{
							var idDocument = doc.Id;

							// Document logs
							var dlogs = await context.DocumentLog.Where(x => x.DocumentID == idDocument).ToListAsync();
							if (dlogs.Count > 0) context.DocumentLog.RemoveRange(dlogs);

							// Approvals related to this document (and activities/flows)
							var approvalDC = await context.Approvals.Where(x => x.DocumentID == idDocument).ToListAsync();
							if (approvalDC.Count > 0)
							{
								var approvalIds = approvalDC.Select(a => a.Id).ToList();

								var activities = await context.ApprovalActivities.Where(a => approvalIds.Contains(a.ApprovalID)).ToListAsync();
								if (activities.Count > 0) context.ApprovalActivities.RemoveRange(activities);

								var flows = await context.ApprovalFlows.Where(f => approvalIds.Contains(f.ApprovalID)).ToListAsync();
								if (flows.Count > 0) context.ApprovalFlows.RemoveRange(flows);

								context.Approvals.RemoveRange(approvalDC);
							}

							// Document attributes
							var docAttr = await context.DocumentAttributes.Where(x => x.DocumentID == idDocument).ToListAsync();
							if (docAttr.Count > 0) context.DocumentAttributes.RemoveRange(docAttr);

							// Document favorites
							var docFavs = await context.DocumentFavorites.Where(x => x.DocumentId == idDocument).ToListAsync();
							if (docFavs.Count > 0) context.DocumentFavorites.RemoveRange(docFavs);

							// Document files (delete physical files then DB rows)
							var docFiles = await context.DocumentFiles.Where(x => x.DocumentID == idDocument).Include(x => x.InsertedByUser).ToListAsync();
							if (docFiles.Count > 0)
							{
								try
								{
									var model = await context.Categories.FirstOrDefaultAsync(y => y.Id == doc.CategoryID);
									var uploadPath = DocumentFilesHelper.GetPhysicalPathForCategory(doc.InsertedBy, doc.OwnerInfo.UserName, model.Id, model.CategoryName);

									foreach (var fl in docFiles)
									{
										var theFile = Path.Combine(uploadPath, fl.NewDocumentFileName);
										if (!string.IsNullOrEmpty(fl.NewDocumentFileName) && File.Exists(theFile))
										{
											try { File.Delete(theFile); } catch { /* ignore file delete errors */ }
										}
									}
								}
								catch
								{
									// ignore file system errors - still remove DB rows
								}

								context.DocumentFiles.RemoveRange(docFiles);
							}

							// Document item list
							var docItemList = await context.DocumentItemList.Where(x => x.DocumentID == idDocument).ToListAsync();
							if (docItemList.Count > 0) context.DocumentItemList.RemoveRange(docItemList);

							// Related documents (both directions)
							var docRelated = await context.RelatedDocuments.Where(x => x.DocumentID == idDocument || x.RelatedDocumentID == idDocument).ToListAsync();
							if (docRelated.Count > 0) context.RelatedDocuments.RemoveRange(docRelated);

							// Document reminders
							var docReminders = await context.DocumentReminders.Where(x => x.DocumentID == idDocument).ToListAsync();
							if (docReminders.Count > 0) context.DocumentReminders.RemoveRange(docReminders);

							// Document shared and privileges
							var docShared = await context.DocumentShared.FirstOrDefaultAsync(x => x.DocumentID == idDocument);
							if (docShared != null)
							{
								var sharePriv = await context.DocumentSharedPrivillege.Where(x => x.DocumentSharedID == docShared.Id).ToListAsync();
								if (sharePriv.Count > 0) context.DocumentSharedPrivillege.RemoveRange(sharePriv);
								context.DocumentShared.Remove(docShared);
							}

							// Notifications for this document
							var notifs = await context.Notifications.Where(x => x.DocumentID == idDocument).ToListAsync();
							if (notifs.Count > 0) context.Notifications.RemoveRange(notifs);

							// Finally remove the document row
							context.Documents.Remove(doc);

							await context.SaveChangesAsync();
						} // end documents

						// ---- Categories owned by user ----
						var myCats = await context.Categories.Where(x => x.Owner == usrId).Include(x => x.OwnerInfo).ToListAsync();
						foreach (var cat in myCats)
						{
							int categoryID = cat.Id;

							// Approvals for category (category-level approvals)
							var approval = await context.Approvals.Where(x => x.CategoryID == categoryID && x.DocumentID == null).ToListAsync();
							if (approval.Count > 0)
							{
								var approvalIds = approval.Select(a => a.Id).ToList();

								var appActv = await context.ApprovalActivities.Where(x => approvalIds.Contains(x.ApprovalID)).ToListAsync();
								if (appActv.Count > 0) context.ApprovalActivities.RemoveRange(appActv);

								var appFlows = await context.ApprovalFlows.Where(x => approvalIds.Contains(x.ApprovalID)).ToListAsync();
								if (appFlows.Count > 0) context.ApprovalFlows.RemoveRange(appFlows);

								context.Approvals.RemoveRange(approval);
							}

							// Category favorites
							var favs = await context.CategoriesFavorites.Where(x => x.CategoryID == categoryID).ToListAsync();
							if (favs.Count > 0) context.CategoriesFavorites.RemoveRange(favs);

							// Category shares
							var shared = await context.CategoriesShared.Where(x => x.CategoryID == categoryID).ToListAsync();
							if (shared.Count > 0) context.CategoriesShared.RemoveRange(shared);

							// Documents in this category (ensure cleanup)
							var documentsInCategory = await context.Documents.Where(x => x.CategoryID == categoryID).ToListAsync();
							foreach (var doc in documentsInCategory)
							{
								var idDocument = doc.Id;

								var dlogs = await context.DocumentLog.Where(x => x.DocumentID == idDocument).ToListAsync();
								if (dlogs.Count > 0) context.DocumentLog.RemoveRange(dlogs);

								var approvalDC = await context.Approvals.Where(x => x.DocumentID == idDocument).ToListAsync();
								if (approvalDC.Count > 0)
								{
									var approvalIds = approvalDC.Select(a => a.Id).ToList();

									var activities = await context.ApprovalActivities.Where(a => approvalIds.Contains(a.ApprovalID)).ToListAsync();
									if (activities.Count > 0) context.ApprovalActivities.RemoveRange(activities);

									var flows = await context.ApprovalFlows.Where(f => approvalIds.Contains(f.ApprovalID)).ToListAsync();
									if (flows.Count > 0) context.ApprovalFlows.RemoveRange(flows);

									context.Approvals.RemoveRange(approvalDC);
								}

								var docAttr = await context.DocumentAttributes.Where(x => x.DocumentID == idDocument).ToListAsync();
								if (docAttr.Count > 0) context.DocumentAttributes.RemoveRange(docAttr);

								var docFavs = await context.DocumentFavorites.Where(x => x.DocumentId == idDocument).ToListAsync();
								if (docFavs.Count > 0) context.DocumentFavorites.RemoveRange(docFavs);

								var docFiles = await context.DocumentFiles.Where(x => x.DocumentID == idDocument).ToListAsync();
								if (docFiles.Count > 0)
								{
									try
									{
										var model = await context.Categories.FirstOrDefaultAsync(y => y.Id == doc.CategoryID);

										var uploadPath = DocumentFilesHelper.GetPhysicalPathForCategory(model.InsertedBy, cat.OwnerInfo.UserName, model.Id, model.CategoryName);

										foreach (var fl in docFiles)
										{
											var theFile = Path.Combine(uploadPath, fl.NewDocumentFileName ?? string.Empty);
											if (!string.IsNullOrEmpty(fl.NewDocumentFileName) && File.Exists(theFile))
											{
												try { File.Delete(theFile); } catch { /* ignore */ }
											}
										}
									}
									catch { /* ignore */ }

									context.DocumentFiles.RemoveRange(docFiles);
								}

								var docItemList = await context.DocumentItemList.Where(x => x.DocumentID == idDocument).ToListAsync();
								if (docItemList.Count > 0) context.DocumentItemList.RemoveRange(docItemList);

								var docRelated = await context.RelatedDocuments.Where(x => x.DocumentID == idDocument || x.RelatedDocumentID == idDocument).ToListAsync();
								if (docRelated.Count > 0) context.RelatedDocuments.RemoveRange(docRelated);

								var docReminders = await context.DocumentReminders.Where(x => x.DocumentID == idDocument).ToListAsync();
								if (docReminders.Count > 0) context.DocumentReminders.RemoveRange(docReminders);

								var docShared = await context.DocumentShared.FirstOrDefaultAsync(x => x.DocumentID == idDocument);
								if (docShared != null)
								{
									var sharePriv = await context.DocumentSharedPrivillege.Where(x => x.DocumentSharedID == docShared.Id).ToListAsync();
									if (sharePriv.Count > 0) context.DocumentSharedPrivillege.RemoveRange(sharePriv);
									context.DocumentShared.Remove(docShared);
								}

								var notifs = await context.Notifications.Where(x => x.DocumentID == idDocument).ToListAsync();
								if (notifs.Count > 0) context.Notifications.RemoveRange(notifs);

								context.Documents.Remove(doc);
							}

							// Finally remove the category
							context.Categories.Remove(cat);

							await context.SaveChangesAsync();
						} // end categories

						// ---- Remove user related reference tables ----
						var userCompanies = await context.UserCompany.Where(x => x.UserId == usrId).ToListAsync();
						if (userCompanies.Count > 0) context.UserCompany.RemoveRange(userCompanies);

						var userMatrices = await context.UserMatrix.Where(x => x.UserId == usrId).ToListAsync();
						if (userMatrices.Count > 0) context.UserMatrix.RemoveRange(userMatrices);

						var userRoles = await context.UserRole.Where(x => x.UserId == usrId).ToListAsync();
						if (userRoles.Count > 0) context.UserRole.RemoveRange(userRoles);

						var userGroups = await context.UserGroup.Where(x => x.UserId == usrId).ToListAsync();
						if (userGroups.Count > 0) context.UserGroup.RemoveRange(userGroups);

						var userMedia = await context.UserMedia.Where(x => x.UserId == usrId).ToListAsync();
						if (userMedia.Count > 0)
						{
							foreach (var um in userMedia)
							{
								try
								{
									if (!string.IsNullOrEmpty(um.PhotoUrl))
									{
										//var path = Path.Combine(AppContext.BaseDirectory, um.PhotoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
										var path = Path.Combine(AppContext.BaseDirectory, um.PhotoUrl.TrimStart('/').Replace("profilep", "user_pp").Replace('/', Path.DirectorySeparatorChar));
										if (File.Exists(path))
										{
											try { File.Delete(path); } catch { /* ignore */ }
										}
									}
								}
								catch { /* ignore */ }
							}
							context.UserMedia.RemoveRange(userMedia);
						}

						var resetCodes = await context.ResetPasswordUserVerificationCode.Where(x => x.UserId == usrId).ToListAsync();
						if (resetCodes.Count > 0) context.ResetPasswordUserVerificationCode.RemoveRange(resetCodes);

						var userNotifications = await context.Notifications.Where(n => n.TargetActor == usrId).ToListAsync();
						if (userNotifications.Count > 0) context.Notifications.RemoveRange(userNotifications);

						// Finally remove the user itself
						var userEntity = await context.User.FirstOrDefaultAsync(u => u.UserId == usrId);
						if (userEntity != null) context.User.Remove(userEntity);

						await context.SaveChangesAsync();

						await transaction.CommitAsync();
						removedCount++;
					}
					catch
					{
						await transaction.RollbackAsync();
						throw;
					}
				} // end foreach user
			});

			return removedCount;
		}

		public async Task<Guid> SetLoginLog(string userAction, int userId)
		{
			var request = accessor?.HttpContext?.Request;
			var path = request?.Path;
			var ipAddress = accessor?.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString();
			var userAgent = accessor?.HttpContext?.Request?.Headers?.UserAgent.ToString();
			var log = new LoginActivityLog
			{
				Id = Guid.NewGuid(),
				IPAddress = ipAddress,
				UserAgent = userAgent,
				UserAction = userAction,
				InsertedBy = userId,
				InsertedAt = DateTime.Now,
				LoginTime = DateTime.Now,
				ActorUserId = userId
			};
			context.LoginActivityLogs.Add(log);
			await context.SaveChangesAsync();
			return log.Id;
		}
	}
}
