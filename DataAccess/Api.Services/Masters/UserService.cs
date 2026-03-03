using Api.DataAccess.Models.Masters;
using Api.DataAccess.Models.Systems;
using Api.Domain;
using Api.Domain.Attributes;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Authentications;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityRequests.Masters;
using Api.Domain.EntityResponses;
using Api.Domain.EntityResponses.Users;
using Api.Domain.Enum;
using Api.Extensions;
using Api.Extensions.Services;
using Api.Repository;
using Api.Repository.Masters;
using Api.Repository.Systems;
using MailKit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NetTopologySuite.Algorithm;
using Syncfusion.EJ2.Notifications;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Api.Services.Masters
{
	public class UserService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, ICompanyRepository companyRepository,
		IUserRepository userRepository, IRoleRepository roleRepository, IUserRoleRepository userRoleRepository,
		IUserCompanyRepository userCompanyRepository, IGroupRepository groupRepository, IUserGroupRepository userGroupRepository,
		IEmailRepository emailRepository, ILicenseManager licenseManager) : BaseService(accessor, languageRepository)
	{

		string frontHost = "http://dev.shuba.co.id:3000"; //"http://localhost:3000";

		private object CreateReponse(User user)
		{
			return new
			{
				user.UserId,
				user.UserName,
				user.EmailAddress,
				user.PhoneNumber,
				user.FullName,
				user.EmailVerified,
				Company = new
				{
					user.CompanyId,
					user.Company.Name,
					user.Company.Address,
					user.Company.PhoneNumber,
				},
				Companies = user.UserCompanies.Select(x => new
				{
					x.CompanyId,
					x.Company.Name,
					x.Company.Address,
					x.Company.PhoneNumber
				}).ToList(),
				Groups = user.UserGroup.Select(x => new
				{
					x.GroupId,
					x.Group.GroupName
				}),
				user.UserType,
				user.IsADUser,
				user.IsActive,
				user.InsertedBy,
				user.InsertedAt,
				user.UpdatedBy,
				user.UpdatedAt,
				user.InsertedByUserName,
				user.InsertedByFullName,
				user.UpdatedByUserName,
				user.UpdatedByFullName,
				//Roles = user.UserRoles.Select(x => new
				//{
				//	x.RoleId,
				//	x.Role.Name
				//})
			};
		}

		private object CreateReponse(List<User> users)
		{
			return users.Select(user => new
			{
				user.UserId,
				user.UserName,
				user.FullName,
				user.EmailVerified,
				Company = new
				{
					user.CompanyId,
					user.Company.Name,
					user.Company.Address,
					user.Company.PhoneNumber,
				},
				Companies = user.UserCompanies.Select(x => new
				{
					x.CompanyId,
					x.Company.Name,
					x.Company.Address,
					x.Company.PhoneNumber
				}).ToList(),
				user.IsADUser,
				user.IsActive,
				user.InsertedBy,
				user.InsertedAt,
				user.UpdatedBy,
				user.UpdatedAt,
				user.InsertedByUserName,
				user.InsertedByFullName,
				user.UpdatedByUserName,
				user.UpdatedByFullName,
				Roles = user.UserRoles.Select(x => new
				{
					x.RoleId,
					x.Role.Name
				})
			});
		}

		private object CreateReponseProfile(User user)
		{
			return new
			{
				user.UserId,
				user.UserName,
				user.FullName,
				user.EmailAddress,
				user.PhoneNumber,
				UserPassword = Encryption.Decrypt(user.UserPassword),
				user.UserMedia
			};
			//new UserMedia
			//{
			//	PhotoName = x.PhotoName,
			//	//PhotoImage = x.PhotoImage,
			//	DateFormat = x.DateFormat,
			//	Base64String = Convert.ToBase64String(x.PhotoImage)
			//})
		}

		private string GeneratePassword(int length)
		{
			//const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
			const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
			StringBuilder res = new StringBuilder();
			Random rnd = new Random();
			while (0 < length--)
			{
				res.Append(valid[rnd.Next(valid.Length)]);
			}
			return res.ToString();
		}

		private string GenerateTokenResetPassword(int length)
		{
			const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
			StringBuilder res = new StringBuilder();
			Random rnd = new Random();
			while (0 < length--)
			{
				res.Append(valid[rnd.Next(valid.Length)]);
			}
			return res.ToString();
		}

		//public async Task<object> GetAll(RequestFilter request)
		//{
		//	await ValidateInputRequestAsync(request);
		//	var result = await userRepository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
		//	return ObjectFlatter.Flatten(result);
		//}

		public async Task<object> GetUserList(RequestUserList request, bool isDelete)
		{
			await ValidateInputRequestAsync(request);
			ResponseUserPagination response = await userRepository.GetUserList(request.Username, isDelete, request.Page, request.Limit);
			var totalRecord = response.TotalRecord;

			return new ResponsePagination
			{
				TotalRecords = totalRecord,
				TotalPages = GetTotalPages(totalRecord, request.Limit),
				Data = response.UsersRecord
			};
		}

		public async Task<object> GetUserAsResponse(string username) => CreateReponse(await GetUser(username));

		public async Task<object> GetUserAsResponse(int userId) => CreateReponse(await GetUser(userId));

		public async Task<object> GetUsersAsResponse(int page, int limit) => CreateReponse(await GetUsers(page, limit));

		public async Task<object> GetProfileAsResponse(int userId) => CreateReponseProfile(await GetUser(userId));

		public async Task<User> GetUser(string username)
		{
			await ValidateInputAsync(username);
			await userRepository.LogTransaction($"Get user by username {username}", Domain.Attributes.UserAction.Read);
			return await userRepository.GetUser(username);
		}

		public async Task<User> GetUserFromMailAddress(string emailaddress)
		{
			await ValidateInputAsync(emailaddress);
			//await userRepository.LogTransaction($"Get user by email address {emailaddress}", Domain.Attributes.UserAction.Read);
			return await userRepository.GetUserFromMailAddress(emailaddress);
		}
		//

		public async Task<User> GetUser(int userId)
		{
			await ValidateInputAsync(userId);
			await userRepository.LogTransaction($"Get user by user id {userId}", Domain.Attributes.UserAction.Read);
			return await userRepository.GetUser(userId);
		}

		public async Task<List<User>> GetUsers(int page, int limit)
		{
			await ValidateInputAsync([page, limit]);
			await userRepository.LogTransaction($"Get user page {page} limit {limit}", Domain.Attributes.UserAction.Read);
			return await userRepository.GetUsers(page, limit);
		}

		public async Task<ResponseLogin> Login(string username, string password)
		{
			await ValidateInputAsync([username, password]);

			//var users = await userRepository.GetAsync(x => x.UserName == username);
			var hashedPassword = Encryption.HashPassword(password);
			User u = await userRepository.GetUser(username, hashedPassword);
			if (u == null)
				return null;

			//if (u.IsLogin == true)
			//{
			//    var message = await GetMessage(LangCodes.LoginAlready);
			//    throw new ApiException($"User {u.UserName} {message}");
			//}

			if (u.IsADUser)
			{
				var atr = ActiveDirectory.GetUserAttribute(username, password);
				if (string.IsNullOrWhiteSpace(atr.SamAccountName))
					return null;
			}

			if (!u.IsADUser && !u.EmailVerified)
			{
				var message = await GetMessage(LangCodes.EmailUnverified);
				throw new ApiException($"{message}. Email {u.UserName}");
			}

			await userRepository.SetLastLogin(u.UserId);

			await userRepository.SetLoginLog("Login", u.UserId);

			await userRepository.LogTransaction($"User authenticate/login", Domain.Attributes.UserAction.Read);

			return new ResponseLogin
			{
				UserId = u.UserId,
				CompanyId = u.CompanyId,
				CompanyName = u.Company?.Name,
				UserName = u.UserName,
				FullName = u.FullName,
				IsAdmin = u.IsAdmin,
				UserType = u.UserType,
				Companies = u.UserCompanies?.Select(x => new ResponseLogin.Company
				{
					CompanyId = x.CompanyId,
					CompanyName = x.Company.Name
				}).ToList() ?? []
			};
		}

		public async Task Logout(int userId)
		{
			await userRepository.SetIsLogin(userId, false);
		}

		public async Task<string> UpdateResetPasswordProfile(RequestUserIdUpdatePassword request)
		{
			await ValidateInputRequestAsync(request);
			var (strong, reason) = InputValidator.IsStrongPassword(request.NewPassword);
			if (!strong)
				throw new ApiException(reason);

			var currentUser = await userRepository.GetSingleAsync(x => x.UserId == request.UserId);

			if (currentUser == null)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"{message}. User ID {request.UserId}");
			}

			//if (currentUser.UserPassword != Encryption.HashPassword(request.OldPassword))
			//{
			//	var message = await GetMessage(LangCodes.PassInvalid);
			//	throw new ApiException($"{message}");
			//}
			if (request.NewPassword != request.ConfirmNewPassword)
			{
				var message = await GetMessage(LangCodes.PassNotMatch);
				throw new ApiException($"{message}");
			}

			var success = await userRepository.UpdatePassword(request.UserId, Encryption.HashPassword(request.NewPassword));
			if (success)
			{
				//await userRepository.LogTransaction($"User updated password", Domain.Attributes.UserAction.Update);
				return request.NewPassword;
			}
			else
				return string.Empty;
		}

		public async Task<string> ChangePasswordProfile(RequestUserChangePasswordProfile request)
		{
			await ValidateInputRequestAsync(request);
			var (strong, reason) = InputValidator.IsStrongPassword(request.NewPassword);
			if (!strong)
				throw new ApiException(reason);

			var currentUser = await userRepository.GetSingleAsync(x => x.UserId == request.UserId);

			if (currentUser == null)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"{message}. User ID {request.UserId}");
			}

			if (currentUser.UserPassword != Encryption.HashPassword(request.OldPassword))
			{
				var message = await GetMessage(LangCodes.PassInvalid);
				throw new ApiException($"{message}");
			}
			if (request.NewPassword != request.ConfirmNewPassword)
			{
				var message = await GetMessage(LangCodes.PassNotMatch);
				throw new ApiException($"{message}");
			}

			var success = await userRepository.UpdatePassword(request.UserId, Encryption.HashPassword(request.NewPassword));
			if (success)
			{
				await userRepository.LogTransaction($"User updated password", Domain.Attributes.UserAction.Update);
				return request.NewPassword;
			}
			else
				return string.Empty;
		}

		public async Task<bool> CreateNewUser(RequestNewUserCreate request)
		{
			await ValidateInputRequestAsync(request);
			await CheckIfUserNameNotExist(request.UserName);
			var userCopy = request.CopyProperties<User>();
			await userRepository.LogTransactionAndAuditTrail($"Insert new User {request.UserName}", Domain.Attributes.UserAction.Insert, userCopy);

			return userRepository.InsertUserRole(request);
		}

		public async Task<bool> UpdateNewUser(RequestUpdateNewUser request)
		{
			await ValidateInputRequestAsync(request);
			var userCopy = request.CopyProperties<User>();
			await userRepository.LogTransactionAndAuditTrail($"Update new User {request.UserName}", Domain.Attributes.UserAction.Insert, userCopy);

			return userRepository.UpdateUserRole(request);
		}

		public async Task<bool> UpdateProfile(RequestUpdateUserInfo request)
		{
			await ValidateInputRequestAsync(request);
			var userCopy = request.CopyProperties<User>();
			await userRepository.LogTransactionAndAuditTrail($"Update Profile {request.UserId}", Domain.Attributes.UserAction.Insert, userCopy);

			return await userRepository.UpdateProfile(request);
		}

		public async Task<bool> UploadProfile(IFormFile file)
		{
			await userRepository.LogTransactionAndAuditTrail($"Upload Profile", Domain.Attributes.UserAction.Insert, new User());
			return await userRepository.UploadProfile(file);
		}

		public async Task<User> Create(RequestUserCreate request)
		{
			await ValidateInputRequestAsync(request);

			var any = await userRepository.AnyAsync(x => x.UserName == request.UserName);//&& x.EmailVerified == true
			if (any)
			{
				var message = await GetMessage(LangCodes.DuplicateUsername);
				throw new ApiException($"{message}. Username {request.UserName}");
			}

			var anyEmailAddress = await userRepository.AnyAsync(x => x.EmailAddress == request.EmailAddress);
			if (anyEmailAddress)
			{
				var message = await GetMessage(LangCodes.DuplicateEmailAdress);
				throw new ApiException($"{message}. Email Address {request.EmailAddress}");
			}

			var phoneNumber = await userRepository.AnyAsync(x => x.PhoneNumber == request.PhoneNumber);
			if (anyEmailAddress)
			{
				var message = await GetMessage(LangCodes.DuplicatePhoneNumber);
				throw new ApiException($"{message}");
			}

			if (request.Groups.Count == 0)
			{
				var message = await GetMessage(LangCodes.InputEmpty);
				throw new ApiException($"{message}. @Groups");
			}

			//var groupValid = await groupRepository.CheckIfExist([request.GroupId]);
			//if (!groupValid)
			//{
			//	var message = await GetMessage(LangCodes.InputInvalid);
			//	throw new ApiException($"{message} Group ID {request.GroupId}");
			//}

			bool groupValid = false;
			if (request.Groups.Any())
			{
				groupValid = await groupRepository.CheckIfExist(request.Groups.Distinct().ToList());
				if (!groupValid)
				{
					var message = await GetMessage(LangCodes.InputInvalid);
					throw new ApiException($"{message}. @Groups");
				}
			}

			if (!string.IsNullOrEmpty(request.CompanyId))
			{
				var companyValid = await companyRepository.CheckIfExist([request.CompanyId]);
				if (!companyValid)
				{
					var message = await GetMessage(LangCodes.InputInvalid);
					throw new ApiException($"{message} Company ID {request.CompanyId}");
				}

				if (request.Companies.Any())
				{
					companyValid = await companyRepository.CheckIfExist(request.Companies);
					if (!companyValid)
					{
						var message = await GetMessage(LangCodes.InputInvalid);
						throw new ApiException($"{message}. @Companies");
					}
				}
			}

			var userExist = await userRepository.AnyAsync(x => x.UserName == request.UserName && x.CompanyId == request.CompanyId);
			if (userExist)
			{
				var message = await GetMessage(LangCodes.Duplicate);
				throw new ApiException($"{message}. User name {request.UserName} && Company {request.CompanyId}");
			}

			var generatedPassword = GeneratePassword(15);
			var user = new User
			{
				IsADUser = request.IsADUser,
				CompanyId = request.CompanyId,
				UserType = request.UserType,
				EmailAddress = request.EmailAddress,
				UserName = request.UserName,
				FullName = request.FullName,
				UserPassword = request.IsADUser ? null : Encryption.HashPassword(generatedPassword),
				EmailVerified = false,//Debugger.IsAttached
				IsActive = false,
				IsDeleted = false,
				PhoneNumber = request.PhoneNumber,
				UserStatus = UserStatus.EmailVerifying.GetHashCode()
			};

			User saveduser = await userRepository.CrateNewUser(user, request.Groups, request.Companies);

			if (!request.IsADUser)
			{
				//var (strong, reason) = InputValidator.IsStrongPassword(request.Password);
				//if (!strong)
				//    throw new ApiException(reason);

				var mailId = new
				{
					userid = user.UserId.ToString(),
					username = request.UserName,
					expires = DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ"),
				};

				var encrypted = Encryption.Encrypt(JsonSerializer.Serialize(mailId));

				var setting = AppSettings.Read();
				string Appurl = setting.ApplicationInfoData.ApplicationUrl;

				var body = $"<!DOCTYPE html><html><head><title>Docubase Account Verification</title></head>" +
				$"<body>Dear {user.FullName}</br>" +
				$"<p>Welcome to Docubase Document Management System</p>" +
				$"<p>Please,<a href=\"{Appurl}/verification/verify?xd={encrypted}\">Click Here</a> to verify your account</p>" +
				$"<p><b>Your password is : {generatedPassword}</b></p>" +
				$"<p>Please kindly change your password soon as you login to the application.</p>" +
				$"<p>This link will expire in 1 day. </br>" +
				$"If your link has expired please contact your Administrator.</p>" +
				$"</body></html>";

				var mail = new MailServiceBuilder()
					.Subject("Docubase Account Verification")
					.Body(body, true)
					.AddRecipient(request.EmailAddress)
					.Build();

				try
				{
					mail.Send();
				}
				catch
				{
					Email email = new Email
					{
						To = request.EmailAddress,
						Subject = "Docubase Account Verification",
						Body = body,
						IsHtml = true,
						SentStatus = 0
					};
					await emailRepository.InsertAsync(email);
				}
			}

			if (request.IsADUser)
			{
				var ad = ActiveDirectory.QueryUser(request.UserName);
				if (ad == null)
				{
					var message = await GetMessage(LangCodes.NotFound);
					throw new ApiException($"{message}. Username {request.UserName}");
				}

				if (!string.IsNullOrEmpty(ad.DisplayName))
				{
					await userRepository.SetFullName(request.UserName, ad.DisplayName);
				}

				if (!string.IsNullOrWhiteSpace(ad?.Mail))
				{
					var body = "Congratulations!\r\n" +
					$"Your Active Directory account is now activated.\r\n\r\n" +
					$"Thank you";

					var mail = new MailServiceBuilder()
						.Subject("Account Activated")
						.Body(body, false)
						.AddRecipient(ad.Mail)
						.Build();

					try
					{
						mail.Send();
					}
					catch
					{
						Email email = new Email
						{
							To = ad.Mail,
							Subject = "Account Activated",
							Body = body,
							IsHtml = true,
							SentStatus = 0
						};
						await emailRepository.InsertAsync(email);
					}
				}
			}

			var userCopy = saveduser.CopyProperties<User>();
			await userRepository.LogTransactionAndAuditTrail($"Administrative user creation, user id {user.UserId} {user.UserName}", Domain.Attributes.UserAction.Insert, userCopy);
			return saveduser;
		}

		public async Task<User> Update(RequestUserUpdate request)
		{
			await ValidateInputRequestAsync(request);

			var currentUser = await userRepository.GetSingleAsync(x => x.UserId == request.UserId);
			if (currentUser == null)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"{message}. User ID {request.UserId}");
			}

			//if changing username it needs to revalidate
			var usernameChanged = !currentUser.UserName.Equals(request.UserName, StringComparison.OrdinalIgnoreCase);
			if (usernameChanged)
			{
				var any = await userRepository.AnyAsync(x => x.UserName == request.UserName);
				if (any)
				{
					var message = await GetMessage(LangCodes.Duplicate);
					throw new ApiException($"{message}. User name {request.UserName}");
				}
			}

			//if (request.Roles.Count == 0)
			//{
			//	var message = await GetMessage(LangCodes.InputEmpty);
			//	throw new ApiException($"{message}. @Roles");
			//}

			//var rolesValid = await roleRepository.CheckIfExist(request.Roles.Distinct().ToList());
			//if (!rolesValid)
			//{
			//	var message = await GetMessage(LangCodes.InputInvalid);
			//	throw new ApiException($"{message}. @Roles");
			//}

			//var groupValid = await groupRepository.CheckIfExist([request.GroupId]);
			//if (!groupValid)
			//{
			//	var message = await GetMessage(LangCodes.InputInvalid);
			//	throw new ApiException($"{message}. @Group ID");
			//}

			if (request.Groups.Count == 0)
			{
				var message = await GetMessage(LangCodes.InputEmpty);
				throw new ApiException($"{message}. @Groups");
			}

			var groupValid = await groupRepository.CheckIfExist(request.Groups.Distinct().ToList());
			if (!groupValid)
			{
				var message = await GetMessage(LangCodes.InputInvalid);
				throw new ApiException($"{message}. @Groups");
			}

			var companyValid = await companyRepository.CheckIfExist([request.CompanyId]);
			if (!companyValid)
			{
				var message = await GetMessage(LangCodes.InputInvalid);
				throw new ApiException($"{message}. @Company ID");
			}

			if (request.Companies.Any())
			{
				companyValid = await companyRepository.CheckIfExist(request.Companies);
				if (!companyValid)
				{
					var message = await GetMessage(LangCodes.InputInvalid);
					throw new ApiException($"{message}. @Companies");
				}
			}

			//var userExist = await userRepository.AnyAsync(x => x.UserName == request.UserName && x.CompanyId == request.CompanyId);
			//if (userExist)
			//    throw new ApiException(LangCodes.Duplicate);

			currentUser.IsADUser = request.IsADUser;
			currentUser.UserName = request.UserName;
			currentUser.CompanyId = request.CompanyId;
			currentUser.FullName = request.FullName;
			currentUser.IsActive = request.IsActive;
			currentUser.EmailAddress = request.EmailAddress;
			currentUser.UserType = request.UserType;
			currentUser.PhoneNumber = request.PhoneNumber;
			if(request.IsActive)
				currentUser.UserStatus = UserStatus.Active.GetHashCode();
			else
				currentUser.UserStatus = UserStatus.InActive.GetHashCode();

			//if (request.IsADUser && usernameChanged)
			//{
			//    var atr = ActiveDirectory.GetUserAttribute(request.UserName, request.Password);
			//    if (string.IsNullOrWhiteSpace(atr.SamAccountName))
			//        throw new ApiException(LangCodes.LoginInvalid);
			//}

			//----remarked on 22 Dec 25----
			//if (!request.IsADUser)
			//{
			//	if (!currentUser.EmailVerified)
			//	{
			//		currentUser.EmailVerified = false;

			//		var mailId = new
			//		{
			//			userid = request.UserId.ToString(),
			//			username = request.UserName,
			//			expires = DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ"),
			//		};

			//		var encrypted = Encryption.Encrypt(JsonSerializer.Serialize(mailId));

			//		var setting = AppSettings.Read();
			//		string Appurl = setting.ApplicationInfoData.ApplicationUrl;

			//		var body = $"<!DOCTYPE html><html><head><title>Email Verification</title></head><body><h3>Verify Your Email</h3><p>Please,<a href=\"{Appurl}/verification/verify?query={encrypted}\">Click Here</a>to verify your email. This link will expire in 1 day. If your link has expired contact your Administrator.</p></body></html>";

			//		var mail = new MailServiceBuilder()
			//			.Subject("Email Verification")
			//			.Body(body, true)
			//			.AddRecipient(request.EmailAddress)
			//			.Build();

			//		mail.Send();
			//	}				
			//}

			//if (request.IsADUser)
			//{
			//	var ad = ActiveDirectory.QueryUser(request.UserName);
			//	if (ad == null)
			//	{
			//		var message = await GetMessage(LangCodes.NotFound);
			//		throw new ApiException($"{message}. User name {request.UserName}");
			//	}

			//	if (!string.IsNullOrEmpty(ad.DisplayName))
			//	{
			//		await userRepository.SetFullName(request.UserName, ad.DisplayName);
			//	}

			//	if (!string.IsNullOrWhiteSpace(ad?.Mail))
			//	{
			//		var body = "Congratulations!\r\n" +
			//		$"Your Active Directory account is now activated.\r\n\r\n" +
			//		$"Thank you";

			//		var mail = new MailServiceBuilder()
			//			.Subject("Account Activated")
			//			.Body(body, false)
			//			.AddRecipient(ad.Mail)
			//			.Build();

			//		mail.Send();
			//	}
			//}
			//----end remarked on 22 Dec 25----

			var savedUser = await userRepository.UpdateAsync(currentUser);

			//var userRoleEntities = request.Roles.Select(x => new UserRole { RoleId = x, UserId = currentUser.UserId }).ToList();
			//await userRoleRepository.DeleteInsert(request.UserId, userRoleEntities);

			var userGroupEntities = request.Groups.Select(x => new UserGroup { GroupId = x, UserId = savedUser.UserId }).ToList();
			await userGroupRepository.DeleteInsert(request.UserId, userGroupEntities);

			if (request.Companies.Any())
			{
				var userCompanieEntities = request.Companies.Select(x => new UserCompany { CompanyId = x, UserId = request.UserId }).ToList();
				await userCompanyRepository.DeleteInsert(request.UserId, userCompanieEntities);
			}

			await userRepository.LogTransactionAndAuditTrail($"Administrative user edit", Domain.Attributes.UserAction.Update, savedUser);

			return savedUser;
		}

		public async Task<bool> VerifyEmail(int userId)
		{
			await ValidateInputAsync(userId);

			await CheckIfExist(userId);

			await userRepository.LogTransaction($"{accessor.HttpContext.GetClaim<string>("given_name")} verified email of user {userId}", Domain.Attributes.UserAction.Update);
			return await userRepository.VerifyEmail(userId);
		}

		public async Task<ResponseEmailVerification> VerifyEmail(string query)
		{
			var decrypted = Encryption.Decrypt(query);
			if (decrypted == null)
			{
				var message = await GetMessage(LangCodes.InputInvalidCode);
				throw new ApiException(message, HttpStatusCode.BadRequest);
			}

			var d = JsonSerializer.Deserialize<JsonObject>(decrypted);

			var exp = Convert.ToDateTime(d["expires"].ToString());
			var now = DateTime.Now;

			if (now > exp)
			{
				var message = await GetMessage(LangCodes.EmailExpired);
				throw new ApiException(message, HttpStatusCode.BadRequest);
			}

			var userid = int.Parse(d["userid"].ToString());
			var success = await VerifyEmail(userid);
			if (success)
			{
				var message = await GetMessage(LangCodes.EmailVerified);
				return new ResponseEmailVerification
				{
					StatusCode = HttpStatusCode.OK,
					Message = message
				};
			}
			else
			{
				var message = await GetMessage(LangCodes.ProcessFailed);
				return new ResponseEmailVerification
				{
					StatusCode = HttpStatusCode.InternalServerError,
					Message = message
				};
			}
		}

		public async Task<int> DeleteWithChildren(List<int> userIds)
		{
			await ValidateInputAsync(userIds);

			return await userRepository.DeleteWithChildren(userIds);
		}

		public async Task<User> SoftDelete(int userId)
		{
			await ValidateInputAsync(userId);

			await CheckIfExist(userId);
			var entity = new User { UserId = userId };
			await userRepository.LogTransactionAndAuditTrail($"Soft Delete for user with id {userId}", Domain.Attributes.UserAction.Update, entity);
			//return await userRepository.MarkAsDeletedAsync(entity);
			return await userRepository.MarkAsDeletedAsync(userId);
		}

		public async Task<User> SoftUndelete(int userId)
		{
			await ValidateInputAsync(userId);

			await CheckIfExist(userId);
			var entity = new User { UserId = userId };
			await userRepository.LogTransactionAndAuditTrail($"Soft undelete user with id {userId}", Domain.Attributes.UserAction.Update, entity);
			//return await userRepository.MarkAsNotDeletedAsync(entity);
			return await userRepository.MarkAsUnDeletedAsync(userId);
		}
		//public async Task<User> HardDelete(int id)
		public async Task<int> HardDelete(int id)
		{
			await ValidateInputAsync(id);
			await CheckIfExist(id);
			var entity = await userRepository.GetSingleAsync(x => x.UserId == id);
			await userRepository.LogTransactionAndAuditTrail($"Hard Delete User with user Id {id}", Domain.Attributes.UserAction.Delete, entity);
			//check again if usr has been used in, group user, user company, user role
			//return await userRepository.DeleteAsync(entity);
			return await userRepository.DeleteAsync(id);
		}

		private async Task CheckIfExist(int userId)
		{
			var any = await userRepository.AnyAsync(x => x.UserId == userId);
			if (!any)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"{message}. User ID {userId}");
			}
		}

		private async Task CheckIfUserNameExist(string userName)
		{
			var any = await userRepository.AnyAsync(x => x.UserName == userName);
			if (!any)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"{message}. Username {userName}");
			}
		}

		private async Task CheckIfUserNameNotExist(string userName)
		{
			var any = await userRepository.AnyAsync(x => x.UserName == userName);
			if (any)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"{message}. Username {userName}");
			}
		}

		#region RESET PASSWORD
		public async Task<ResponseData<ResponseForgetPassword>> SendVerificationResetPassword(RequestResetPassword email)
		{
			await ValidateInputRequestAsync(email);

			string emailAddress = email.EmailAddress.Replace(" ", "");
			if (!InputValidator.IsValidEmail(emailAddress))
			{
				var message = await GetMessage(LangCodes.EmailInvalid);
				throw new ApiException($"{message}. Email {emailAddress}");
			}

			string generatedVerificationCode = GenerateTokenResetPassword(6);

			var user = await GetUserFromMailAddress(emailAddress);
			if (user == null)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"{message}. Email {emailAddress}");
			}

			var setting = AppSettings.Read();
			string Appurl = setting.ApplicationInfoData.ApplicationUrl.Replace("/login", "");

			var html = $"<!doctype html><html lang=\"en-US\"><head> <meta content=\"text/html; charset=utf-8\" http-equiv=\"Content-Type\" /> " +
			$"<title>Reset Password Email Template</title> <meta name=\"description\" content=\"Reset Password Email Template.\">" +
			$" <style type=\"text/css\">a:hover {{text-decoration: underline !important;}} </style> </head>" +
			$"<body marginheight=\"0\" topmargin=\"0\" marginwidth=\"0\" style=\"margin: 0px; background-color: #f2f3f8;\" leftmargin=\"0\">" +
			$"<table cellspacing=\"0\" border=\"0\" cellpadding=\"0\" width=\"100%\" bgcolor=\"#f2f3f8\" style=\"@import url(https://fonts.googleapis.com/css?family=Rubik:300,400,500,700|Open+Sans:300,400,600,700); font-family: 'Open Sans', sans-serif;\">" +
			$"<tr><td>" +
			$"<table style=\"background-color: #f2f3f8; max-width:670px;  margin:0 auto;\" width=\"100%\" border=\"0\" align=\"center\" cellpadding=\"0\" cellspacing=\"0\">" +
			$"<tr><td style=\"height:80px;\">&nbsp;</td></tr><tr><td style=\"height:20px;\">&nbsp;</td></tr><tr><td><table width=\"95%\" border=\"0\" align=\"center\" cellpadding=\"0\" cellspacing=\"0\" style=\"max-width:670px;background:#fff; border-radius:3px; text-align:center;-webkit-box-shadow:0 6px 18px 0 rgba(0,0,0,.06);-moz-box-shadow:0 6px 18px 0 rgba(0,0,0,.06);box-shadow:0 6px 18px 0 rgba(0,0,0,.06);\">" +
			$"<tr><td style=\"height:40px;\">&nbsp;</td></tr><tr><td style=\"padding:0 35px;\"><h1 style=\"color:#1e1e2d; font-weight:500; margin:0;font-size:32px;font-family:'Rubik',sans-serif;\">" +
			$"You have requested to reset your password</h1><span style=\"display:inline-block; vertical-align:middle; margin:29px 0 26px; border-bottom:1px solid #cecece; width:100px;\"></span>" +
			$"<p style=\"color:#455056; font-size:15px;line-height:24px; margin:0;\">Dear User,</p><p style=\"color:#455056; font-size:15px;line-height:24px; margin:0;\"> " +
			$"We cannot simply send you your old password. A unique link to reset your password has been generated for you. To reset your password, click the following link and insert verification code below : </br>{generatedVerificationCode}.</p>" +
			$"<a href=\"javascript:void(0);\" style=\"background:#20e277;text-decoration:none !important; font-weight:500; margin-top:35px; color:#fff;text-transform:uppercase; font-size:14px;padding:10px 24px;display:inline-block;border-radius:50px;\">Reset Password</a></td></tr>" +
			$"<tr><td style=\"height:40px;\">&nbsp;</td></tr></table></td><tr><td style=\"height:20px;\">&nbsp;</td></tr>" +
			$"<tr><td style=\"height:80px;\">&nbsp;</td></tr></table></td></tr></table></body></html>";

			var obj = new EncryptedForgetPassword
			{
				Email = emailAddress,
				UserId = user.UserId,
				ValidityPeriod = DateTimeOffset.Now.AddDays(1).ToUnixTimeSeconds(),
			};

			var encrypted = Encryption.Encrypt(JsonSerializer.Serialize(obj));

			html = html.Replace("javascript:void(0);", $"{Appurl}/code-verification?enc={encrypted}");
			var mail = new MailServiceBuilder()
				.Subject("Reset Password")
				.Body(html, true)
				.AddRecipient(emailAddress)
			.Build();

			var result = mail.Send();
			if (result.StartsWith("OK"))
			{
				//insert into ResetPasswordUserVerificationCode
				await userRepository.SaveVerificationCode(user.UserId, generatedVerificationCode);

				//await userRepository.LogTransaction($"Forget password. Send direct email", Domain.Attributes.UserAction.Insert);
				return new ResponseData<ResponseForgetPassword>
				{
					Code = System.Net.HttpStatusCode.OK,
					Data = new ResponseForgetPassword
					{
						Reason = "OK",
						ValidityPeriod = obj.ValidityPeriod
					}
				};
			}

			throw new ApiException(result);
		}

		public async Task<object> CekVerificationCodeResetPassword(string verificationCode, string ecryptedQuerystring)
		{
			var decrypted = Encryption.Decrypt(ecryptedQuerystring);
			if (decrypted == null)
			{
				var message = await GetMessage(LangCodes.InputInvalidCode);
				throw new ApiException(message, HttpStatusCode.BadRequest);
			}
			var d = JsonSerializer.Deserialize<JsonObject>(decrypted);

			DateTimeOffset exp = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(d["ValidityPeriod"].ToString()));
			DateTime validityPeriod = exp.DateTime;
			var now = DateTime.Now;

			if (now > validityPeriod)
			{
				var message = await GetMessage(LangCodes.EmailExpired);
				throw new ApiException(message, HttpStatusCode.BadRequest);
			}

			//cek if user is active or not here

			var userid = int.Parse(d["UserId"].ToString());
			var issuccess = await userRepository.VerifiyResetPasswordCode(userid, verificationCode);
			if(!issuccess)
			{
				var message = await GetMessage(LangCodes.InvalidCodeVerification);
				throw new ApiException(message, HttpStatusCode.BadRequest);
			}
			
			return new { IsValid = issuccess, UserId = userid };
		}

		public async Task<object> ReturnEncResetPassword(string ecryptedQuerystring)
		{
			//var decrypted = Encryption.Decrypt(ecryptedQuerystring);
			//if (decrypted == null)
			//{
			//	var message = await GetMessage(LangCodes.InputInvalidCode);
			//	throw new ApiException(message, HttpStatusCode.BadRequest);
			//}
			//var d = JsonSerializer.Deserialize<JsonObject>(decrypted);

			//var exp = Convert.ToDateTime(d["expires"].ToString());
			//var now = DateTime.Now;

			//if (now > exp)
			//{
			//	var message = await GetMessage(LangCodes.EmailExpired);
			//	throw new ApiException(message, HttpStatusCode.BadRequest);
			//}

			//var userid = int.Parse(d["userid"].ToString());
			//return new
			//{
			//	EmailAddress = d["email"].ToString(),
			//	ApproverUserID = userid,
			//	Expiration = d["expires"].ToString()
			//};
			return new
			{
				encResetPassword = ecryptedQuerystring
			};
		}

		public async Task<ResponseMessage> ResetPassword(string encryptedResetPassword, RequestUserResetPassword request)
		{
			await ValidateInputAsync(encryptedResetPassword);
			await ValidateInputRequestAsync(request);

			if (request.RepeatPassword != request.Password)
			{
				var message = await GetMessage(LangCodes.PassInvalid);
				throw new ApiException($"{message}. @Password");
			}

			var decrypted = Encryption.Decrypt(encryptedResetPassword);
			if (decrypted == null)
			{
				var message = await GetMessage(LangCodes.InputInvalid);
				throw new ApiException(message, HttpStatusCode.BadRequest);
			}
			var d = JsonSerializer.Deserialize<JsonObject>(decrypted);

			var exp = Convert.ToDateTime(d["expires"].ToString());
			var now = DateTime.Now;

			if (now > exp)
			{
				var message = await GetMessage(LangCodes.EmailExpired);
				throw new ApiException(message, HttpStatusCode.BadRequest);
			}

			var userid = int.Parse(d["userid"].ToString());
			//var decrypted = Encryption.Decrypt(encryptedResetPassword);
			//if (decrypted == null)
			//{
			//	var message = await GetMessage(LangCodes.InputInvalid);
			//	throw new ApiException($"{message}. @Password");
			//}

			//var req = JsonSerializer.Deserialize<EncryptedForgetPassword>(decrypted);
			//var now = DateTimeOffset.Now.ToUnixTimeSeconds();
			//if (req.ValidityPeriod < now)
			//{
			//	var message = await GetMessage(LangCodes.DateExpired);
			//	throw new ApiException($"{message}. Validity period {req.ValidityPeriod}");
			//}

			var currentUser = await userRepository.GetSingleAsync(x => x.UserId == userid);
			//if (currentUser == null)
			//{
			//	var message = await GetMessage(LangCodes.InputInvalid);
			//	throw new ApiException($"{message}. User ID {req.UserId}");
			//}

			if (currentUser == null)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"{message}. User ID {userid}");
			}

			bool success = await userRepository.UpdatePassword(userid, Encryption.HashPassword(request.RepeatPassword));
			if (success)
			{
				await userRepository.LogTransaction($"User reset password", Domain.Attributes.UserAction.Update);
				return new ResponseMessage
				{
					Code = System.Net.HttpStatusCode.OK,
					Message = LangCodes.ProcessSuccess
				};
			}

			var msg = await GetMessage(LangCodes.UnknownException);
			throw new ApiException(msg);
		}
		#endregion

		public async Task<bool> EmptyRecyclebin()
		{
			await userRepository.LogTransaction($"Empty recyclebin of users", Domain.Attributes.UserAction.Delete);
			return await userRepository.EmptyRecyclebin(20);
		}

		public async Task ValidateCanRegisterNewUser()
		{
			var currentCount = await userRepository.CountAsync();
			bool isenableRegister = licenseManager.ValidateUserCount((int)currentCount);

			if (!isenableRegister)
			{
				var message = await GetMessage(LangCodes.UserMaxReached);
				throw new ApiException(message);
			}
		}
	}
}
