using Api.DataAccess.Models.Masters;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Authentications;
using Api.Domain.EntityResponses;
using Api.Domain.EntityResponses.Users;
using Api.Extensions;
using Api.Repository.Masters;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Api.Services.Masters
{
    public class UserService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, ICompanyRepository companyRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUserRoleRepository userRoleRepository,
        IUserCompanyRepository userCompanyRepository) : BaseService(accessor, languageRepository)
    {

        string frontHost = "http://dev.shuba.co.id:3000"; //"http://localhost:3000";

        private object CreateReponse(User user)
        {
            return new
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

        public async Task<object> GetAll(ReqestFilter request)
        {
            await ValidateInputRequestAsync(request);
            var result = await userRepository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
            return ObjectFlatter.Flatten(result);
        }

        public async Task<object> GetUserAsResponse(string username) => CreateReponse(await GetUser(username));

        public async Task<object> GetUserAsResponse(int userId) => CreateReponse(await GetUser(userId));

        public async Task<object> GetUsersAsResponse(int page, int limit) => CreateReponse(await GetUsers(page, limit));

        public async Task<User> GetUser(string username)
        {
            await ValidateInputAsync(username);
            await userRepository.LogTransaction($"Get user by username {username}", Domain.Attributes.UserAction.Read);
            return await userRepository.GetUser(username);
        }

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

            await userRepository.LogTransaction($"User authenticate/login", Domain.Attributes.UserAction.Read);

            return new ResponseLogin
            {
                UserId = u.UserId,
                CompanyId = u.CompanyId,
                CompanyName = u.Company?.Name,
                UserName = u.UserName,
                FullName = u.FullName,
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

        public async Task<bool> UpdatePasswordProfile(RequestUserIdUpdatePassword request)
        {
            await ValidateInputRequestAsync(request);

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

            var success = await userRepository.UpdatePassword(request.UserId, Encryption.HashPassword(request.NewPassword));
            if (success)
                await userRepository.LogTransaction($"User updated password", Domain.Attributes.UserAction.Update);

            return success;
        }

        public async Task<User> Create(RequestUserCreate request)
        {
            await ValidateInputRequestAsync(request);

            var any = await userRepository.AnyAsync(x => x.UserName == request.UserName);
            if (any)
            {
                var message = await GetMessage(LangCodes.Duplicate);
                throw new ApiException($"{message}. User name {request.UserName}");
            }

            if (request.Roles.Count == 0)
            {
                var message = await GetMessage(LangCodes.InputEmpty);
                throw new ApiException($"{message}. @Roles");
            }

            var rolesValid = await roleRepository.CheckIfExist(request.Roles.Distinct().ToList());
            if (!rolesValid)
            {
                var message = await GetMessage(LangCodes.InputInvalid);
                throw new ApiException($"{message}. @Roles");
            }

            var companyValid = await companyRepository.CheckIfEExist([request.CompanyId]);
            if (!companyValid)
            {
                var message = await GetMessage(LangCodes.InputInvalid);
                throw new ApiException($"{message} Company ID {request.CompanyId}");
            }

            if (request.Companies.Any())
            {
                companyValid = await companyRepository.CheckIfEExist(request.Companies);
                if (!companyValid)
                {
                    var message = await GetMessage(LangCodes.InputInvalid);
                    throw new ApiException($"{message}. @Companies");
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
                UserName = request.UserName,
                FullName = request.FullName,
                UserPassword = request.IsADUser ? null : Encryption.HashPassword(generatedPassword),
                EmailVerified = Debugger.IsAttached
            };

            user = await userRepository.InsertAsync(user);

            if (!request.IsADUser)
            {
                //var (strong, reason) = InputValidator.IsStrongPassword(request.Password);
                //if (!strong)
                //    throw new ApiException(reason);

                var mailId = new
                {
                    userid = user.UserId.ToString(),
                    username = request.UserName,
                    expires = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                };

                var encrypted = Encryption.Encrypt(JsonSerializer.Serialize(mailId));
                //var body = "Please, click the link below to verify your email. This link will expire in 1 day. If your link has expired contact your Administrator.\r\n" +
                //    $"{frontHost}/profile/verification?query={encrypted}.\r\n" +
                //    $"Your password is : {generatedPassword}";

                var body = $"<!DOCTYPE html><html><head><title>Email Verification</title></head><body><h3>Verify Your Email</h3><p>Please,<a href=\"{frontHost}/profile/verification?query={encrypted}\">Click Here</a>to verify your email. This link will expire in 1 day. If your link has expired contact your Administrator.</p><p><b>Your password is : {generatedPassword}</b></p></body></html>";

                var mail = new MailServiceBuilder()
                    .Subject("Email Verification")
                    .Body(body, true)
                    .AddRecipient(request.UserName)
                    .Build();

                mail.Send();
            }

            if (request.IsADUser)
            {
                var ad = ActiveDirectory.QueryUser(request.UserName);
                if (ad == null)
                {
                    var message = await GetMessage(LangCodes.NotFound);
                    throw new ApiException($"{message}. User name {request.UserName}");
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

                    mail.Send();
                }
            }

            var userRoleEntities = request.Roles.Select(x => new UserRole { RoleId = x, UserId = user.UserId }).ToList();
            await userRoleRepository.InsertManyAsync(userRoleEntities);

            if (request.Companies.Any())
            {
                var userCompanieEntities = request.Companies.Select(x => new UserCompany { CompanyId = x, UserId = user.UserId }).ToList();
                await userCompanyRepository.InsertManyAsync(userCompanieEntities);
            }

            var userCopy = user.CopyProperties<User>();
            await userRepository.LogTransactionAndAuditTrail($"Administrative user creation id {user.UserId} {user.UserName}", Domain.Attributes.UserAction.Insert, userCopy);

            return user;
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

            if (request.Roles.Count == 0)
            {
                var message = await GetMessage(LangCodes.InputEmpty);
                throw new ApiException($"{message}. @Roles");
            }

            var rolesValid = await roleRepository.CheckIfExist(request.Roles.Distinct().ToList());
            if (!rolesValid)
            {
                var message = await GetMessage(LangCodes.InputInvalid);
                throw new ApiException($"{message}. @Roles");
            }

            var companyValid = await companyRepository.CheckIfEExist([request.CompanyId]);
            if (!companyValid)
            {
                var message = await GetMessage(LangCodes.InputInvalid);
                throw new ApiException($"{message}. @Company ID");
            }

            if (request.Companies.Any())
            {
                companyValid = await companyRepository.CheckIfEExist(request.Companies);
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

            //if (request.IsADUser && usernameChanged)
            //{
            //    var atr = ActiveDirectory.GetUserAttribute(request.UserName, request.Password);
            //    if (string.IsNullOrWhiteSpace(atr.SamAccountName))
            //        throw new ApiException(LangCodes.LoginInvalid);
            //}

            if (!request.IsADUser)
            {
                currentUser.EmailVerified = false;

                var mailId = new
                {
                    userid = request.UserId.ToString(),
                    username = request.UserName,
                    expires = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                };

                var encrypted = Encryption.Encrypt(JsonSerializer.Serialize(mailId));
                var body = $"<!DOCTYPE html><html><head><title>Email Verification</title></head><body><h3>Verify Your Email</h3><p>Please,<a href=\"{frontHost}/profile/verification?query={encrypted}\">Click Here</a>to verify your email. This link will expire in 1 day. If your link has expired contact your Administrator.</p></body></html>";

                var mail = new MailServiceBuilder()
                    .Subject("Email Verification")
                    .Body(body, true)
                    .AddRecipient(request.UserName)
                    .Build();

                mail.Send();
            }

            if (request.IsADUser)
            {
                var ad = ActiveDirectory.QueryUser(request.UserName);
                if (ad == null)
                {
                    var message = await GetMessage(LangCodes.NotFound);
                    throw new ApiException($"{message}. User name {request.UserName}");
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

                    mail.Send();
                }
            }


            await userRepository.UpdateAsync(currentUser);

            var userRoleEntities = request.Roles.Select(x => new UserRole { RoleId = x, UserId = currentUser.UserId }).ToList();
            await userRoleRepository.DeleteInsert(request.UserId, userRoleEntities);

            if (request.Companies.Any())
            {
                var userCompanieEntities = request.Companies.Select(x => new UserCompany { CompanyId = x, UserId = request.UserId }).ToList();
                await userCompanyRepository.DeleteInsert(request.UserId, userCompanieEntities);
            }

            await userRepository.LogTransactionAndAuditTrail($"Administrative user edit", Domain.Attributes.UserAction.Update, currentUser);

            return currentUser;
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
            var now = DateTime.UtcNow;

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
            var entity = new User { UserId = userId, };
            await userRepository.LogTransactionAndAuditTrail($"Soft delete user with id {userId}", Domain.Attributes.UserAction.Update, entity);
            return await userRepository.MarkAsDeletedAsync(entity);
        }

        public async Task<User> SoftUndelete(int userId)
        {
            await ValidateInputAsync(userId);

            await CheckIfExist(userId);
            var entity = new User { UserId = userId };
            await userRepository.LogTransactionAndAuditTrail($"Soft undelete user with id {userId}", Domain.Attributes.UserAction.Update, entity);
            return await userRepository.MarkAsNotDeletedAsync(entity);
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

        #region RESET PASSWORD
        public async Task<ResponseData<ResponseForgetPassword>> ResetPasswordToEmail(string email)
        {
            await ValidateInputRequestAsync(email);

            email = email.Replace(" ", "");
            if (!InputValidator.IsValidEmail(email))
            {
                var message = await GetMessage(LangCodes.EmailInvalid);
                throw new ApiException($"{message}. Email {email}");
            }

            var user = await GetUser(email);
            if (user == null)
            {
                var message = await GetMessage(LangCodes.NotFound);
                throw new ApiException($"{message}. Email {email}");
            }

            var html = "\r\n<!doctype html>\r\n<html lang=\"en-US\">\r\n\r\n<head>\r\n    <meta content=\"text/html; charset=utf-8\" http-equiv=\"Content-Type\" />\r\n    <title>Reset Password Email Template</title>\r\n    <meta name=\"description\" content=\"Reset Password Email Template.\">\r\n    <style type=\"text/css\">\r\n        a:hover {text-decoration: underline !important;}\r\n    </style>\r\n</head>\r\n\r\n<body marginheight=\"0\" topmargin=\"0\" marginwidth=\"0\" style=\"margin: 0px; background-color: #f2f3f8;\" leftmargin=\"0\">\r\n    <!--100% body table-->\r\n    <table cellspacing=\"0\" border=\"0\" cellpadding=\"0\" width=\"100%\" bgcolor=\"#f2f3f8\"\r\n        style=\"@import url(https://fonts.googleapis.com/css?family=Rubik:300,400,500,700|Open+Sans:300,400,600,700); font-family: 'Open Sans', sans-serif;\">\r\n        <tr>\r\n            <td>\r\n                <table style=\"background-color: #f2f3f8; max-width:670px;  margin:0 auto;\" width=\"100%\" border=\"0\"\r\n                    align=\"center\" cellpadding=\"0\" cellspacing=\"0\">\r\n                    <tr>\r\n                        <td style=\"height:80px;\">&nbsp;</td>\r\n                    </tr>\r\n\r\n                    <tr>\r\n                        <td style=\"height:20px;\">&nbsp;</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <td>\r\n                            <table width=\"95%\" border=\"0\" align=\"center\" cellpadding=\"0\" cellspacing=\"0\"\r\n                                style=\"max-width:670px;background:#fff; border-radius:3px; text-align:center;-webkit-box-shadow:0 6px 18px 0 rgba(0,0,0,.06);-moz-box-shadow:0 6px 18px 0 rgba(0,0,0,.06);box-shadow:0 6px 18px 0 rgba(0,0,0,.06);\">\r\n                                <tr>\r\n                                    <td style=\"height:40px;\">&nbsp;</td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td style=\"padding:0 35px;\">\r\n                                        <h1 style=\"color:#1e1e2d; font-weight:500; margin:0;font-size:32px;font-family:'Rubik',sans-serif;\">You have\r\n                                            requested to reset your password</h1>\r\n                                        <span\r\n                                            style=\"display:inline-block; vertical-align:middle; margin:29px 0 26px; border-bottom:1px solid #cecece; width:100px;\"></span>\r\n                                      <p style=\"color:#455056; font-size:15px;line-height:24px; margin:0;\">Dear User,</p>\r\n                                        <p style=\"color:#455056; font-size:15px;line-height:24px; margin:0;\">\r\n                                            We cannot simply send you your old password. A unique link to reset your\r\n                                            password has been generated for you. To reset your password, click the\r\n                                            following link and follow the instructions.\r\n                                        </p>\r\n                                        <a href=\"javascript:void(0);\"\r\n                                            style=\"background:#20e277;text-decoration:none !important; font-weight:500; margin-top:35px; color:#fff;text-transform:uppercase; font-size:14px;padding:10px 24px;display:inline-block;border-radius:50px;\">Reset\r\n                                            Password</a>\r\n                                    </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td style=\"height:40px;\">&nbsp;</td>\r\n                                </tr>\r\n                            </table>\r\n                        </td>\r\n                    <tr>\r\n                        <td style=\"height:20px;\">&nbsp;</td>\r\n                    </tr>\r\n\r\n                    <tr>\r\n                        <td style=\"height:80px;\">&nbsp;</td>\r\n                    </tr>\r\n                </table>\r\n            </td>\r\n        </tr>\r\n    </table>\r\n    <!--/100% body table-->\r\n</body>\r\n\r\n</html>";

            var obj = new EncryptedForgetPassword
            {
                Email = email,
                UserId = user.UserId,
                ValidityPeriod = DateTimeOffset.Now.AddDays(1).ToUnixTimeSeconds(),
            };

            var encrypted = Encryption.Encrypt(JsonSerializer.Serialize(obj));

            html = html.Replace("javascript:void(0);", $"{frontHost}/profile/reset-password?query={encrypted}&email={email}");
            var mail = new MailServiceBuilder()
                .Subject("Reset Password")
                .Body(html, true)
                .AddRecipient(email)
            .Build();

            var result = mail.Send();
            if (result.StartsWith("OK"))
            {
                await userRepository.LogTransaction($"Forget password. Send direct email", Domain.Attributes.UserAction.Insert);
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
                throw new ApiException($"{message}. @Password");
            }

            var req = JsonSerializer.Deserialize<EncryptedForgetPassword>(decrypted);
            var now = DateTimeOffset.Now.ToUnixTimeSeconds();
            if (req.ValidityPeriod < now)
            {
                var message = await GetMessage(LangCodes.DateExpired);
                throw new ApiException($"{message}. Validity period {req.ValidityPeriod}");
            }

            var currentUser = await userRepository.GetSingleAsync(x => x.UserId == req.UserId);
            if (currentUser == null)
            {
                var message = await GetMessage(LangCodes.InputInvalid);
                throw new ApiException($"{message}. User ID {req.UserId}");
            }

            if (currentUser == null)
            {
                var message = await GetMessage(LangCodes.NotFound);
                throw new ApiException($"{message}. User ID {req.UserId}");
            }

            bool success = await userRepository.UpdatePassword(req.UserId, Encryption.HashPassword(request.RepeatPassword));
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
    }
}
