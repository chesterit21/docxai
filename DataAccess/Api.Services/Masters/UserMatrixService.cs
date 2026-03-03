using Api.DataAccess.Models.Masters;
using Api.Domain;
using Api.Domain.Attributes;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Masters;
using Api.Domain.EntityResponses.Matrices;
using Api.Domain.EntityResponses.Users;
using Api.Extensions;
using Api.Repository.Masters;
using Microsoft.AspNetCore.Http;

namespace Api.Services.Masters
{
    public class UserMatrixService(
        IHttpContextAccessor accessor,
        IUserRepository userRepository,
        IMenuRepository menuRepository,
        IUserRoleRepository userRoleRepository,
        IUserMatrixRepository userMatrixRepository,
        IRoleMatrixRepository roleMatrixRepository,
        ILanguageRepository languageRepository) : BaseService(accessor, languageRepository)
    {
        public async Task<object> GetAll(RequestFilter request)
        {
            await ValidateInputRequestAsync(request);
            var result = await userMatrixRepository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
            return ObjectFlatter.Flatten(result);
        }

        public async Task<ResponseMenuMatrix> GetFinalMatrixMenu(int userId)
        {
            await ValidateInputAsync(userId);
            var user = await userRepository.GetUser(userId);

            if (user == null)
            {
                var message = await GetMessage(LangCodes.NotFound);
                throw new ApiException($"{message}. User ID {userId}");
            }

            var metrices = new List<ResponseMenuMatrix.Matrix>();

            var menus = await languageRepository.GetAsync(x => x.Type == "Menu");

            var userRolesIds = await userRoleRepository.GetRolesByUserId(userId);
            var roleMatrices = userRolesIds.Any() ? await roleMatrixRepository.GetAsync(userRolesIds) : [];

            if (roleMatrices.Any())
            {
                foreach (var m in roleMatrices)
                {
                    var lang = menus.FirstOrDefault(x => x.Code == m.MenuId);

                    metrices.Add(new ResponseMenuMatrix.Matrix
                    {
                        MenuId = m.MenuId,
                        ParentMenuId = m.Menu.ParentMenuId,
                        Description = m.Menu.Description,
                        Id = lang?.Id,
                        En = lang?.En,
                        Sequence = m.Menu.Sequence,
                        Level = m.Menu.Level,
                        Url = m.Menu.Url,
                        Icon = m.Menu.Icon,
                        IsDelete = m.IsDelete,
                        IsInsert = m.IsInsert,
                        IsRead = m.IsRead,
                        IsUpdate = m.IsUpdate
                    });
                }
            }

            var userMatrices = await userMatrixRepository.GetAsync([userId]);
            if (userMatrices.Any())
            {
                foreach (var m in userMatrices)
                {
                    var role = metrices.FirstOrDefault(x => x.MenuId == m.MenuId);
                    if (role != null)
                    {
                        if (m.IsDelete) role.IsDelete = m.IsDelete;
                        if (m.IsInsert) role.IsInsert = m.IsInsert;
                        if (m.IsUpdate) role.IsUpdate = m.IsUpdate;
                        if (m.IsRead) role.IsRead = m.IsRead;
                    }
                    else
                    {
                        var lang = menus.FirstOrDefault(x => x.Code == m.MenuId);

                        metrices.Add(new ResponseMenuMatrix.Matrix
                        {
                            MenuId = m.MenuId,
                            ParentMenuId = m.Menu.ParentMenuId,
                            Description = m.Menu.Description,
                            Id = lang?.Id,
                            En = lang?.En,
                            Sequence = m.Menu.Sequence,
                            Level = m.Menu.Level,
                            Url = m.Menu.Url,
                            Icon = m.Menu.Icon,
                            IsDelete = m.IsDelete,
                            IsInsert = m.IsInsert,
                            IsRead = m.IsRead,
                            IsUpdate = m.IsUpdate
                        });
                    }
                }
            }

            return new ResponseMenuMatrix
            {
                IsADUser = user.IsADUser,
                CompanyId = user.CompanyId,
                //CompanyName = user.Company.Name,
                UserId = user.UserId,
                UserName = user.UserName,
                EmailVerified = user.EmailVerified,
                FullName = user.FullName,
                IsActive = user.IsActive,
                Matrices = metrices,
                Roles = user.UserRoles.Select(x => new ResponseMenuMatrix.Role { RoleId = x.RoleId, Name = x.Role.Name }).ToList()
            };
        }

        public async Task<ResponseMenuMatrix> GetFinalMatrixUser(int userId)
        {
            await ValidateInputAsync(userId);
            var user = await userRepository.GetUser(userId);

            if (user == null)
            {
                var message = await GetMessage(LangCodes.NotFound);
                throw new ApiException($"{message}. User ID {userId}");
            }

            var menus = await menuRepository.GetAsync();
            var menuLang = await languageRepository.GetAsync(x => x.Type == "Menu");
            var userRolesIds = await userRoleRepository.GetRolesByUserId(userId);
            var roleMatrices = userRolesIds.Any() ? await roleMatrixRepository.GetAsync(userRolesIds) : [];
            var userMatrices = await userMatrixRepository.GetAsync([userId]);

            var metrices = new List<ResponseMenuMatrix.Matrix>();

            foreach (var menu in menus)
            {
                var lang = menuLang.FirstOrDefault(x => x.Code == menu.MenuId);
                var roleMatrix = roleMatrices.FirstOrDefault(x => x.MenuId == menu.MenuId);
                var userMatrix = userMatrices.FirstOrDefault(x => x.MenuId == menu.MenuId);

                metrices.Add(new ResponseMenuMatrix.Matrix
                {
                    MenuId = menu.MenuId,
                    ParentMenuId = menu.ParentMenuId,
                    Description = menu.Description,
                    Id = lang?.Id,
                    En = lang?.En,
                    Sequence = menu.Sequence,
                    Level = menu.Level,
                    Url = menu.Url,
                    Icon = menu.Icon,
                    IsDelete = (roleMatrix?.IsDelete ?? false) || (userMatrix?.IsDelete ?? false),
                    IsInsert = (roleMatrix?.IsRead ?? false) || (userMatrix?.IsInsert ?? false),
                    IsRead = (roleMatrix?.IsRead ?? false) || (userMatrix?.IsRead ?? false),
                    IsUpdate = (roleMatrix?.IsUpdate ?? false) || (userMatrix?.IsUpdate ?? false),
                });
            }

            return new ResponseMenuMatrix
            {
                IsADUser = user.IsADUser,
                CompanyId = user.CompanyId,
                //CompanyName = user.Company.Name,
                UserId = user.UserId,
                UserName = user.UserName,
                EmailVerified = user.EmailVerified,
                FullName = user.FullName,
                IsActive = user.IsActive,
                Matrices = metrices,
                Roles = user.UserRoles.Select(x => new ResponseMenuMatrix.Role { RoleId = x.RoleId, Name = x.Role.Name }).ToList()
            };
        }

        public async Task<bool> HasAccess(int userId, string menuId, UserAction userAction)
        {
            await ValidateInputAsync([userId]);
            await ValidateInputAsync([menuId]);

            var user = await userRepository.GetUser(userId);
            if (user == null)
                return false;

            //the winner is USER NOT ROLE
            var userMatrix = await userMatrixRepository.GetSingleAsync(x => x.UserId == userId && x.MenuId == menuId);
            var propUserAction = $"Is{userAction}";
            if (userMatrix != null)
            {
                var isAllowed = (bool)userMatrix.GetType().GetProperty(propUserAction).GetValue(userMatrix, null);
                if (isAllowed)
                    return isAllowed;
            }

            //check if the current user has roles
            var userRolesIds = await userRoleRepository.GetRolesByUserId(userId);
            if (userRolesIds.Count == 0)
                return false;

            var roleMatrices = await roleMatrixRepository.GetAsync(x => userRolesIds.Contains(x.RoleId) && x.MenuId == menuId);
            if (roleMatrices.Count == 0)
                return false;

            foreach (var roleMatrix in roleMatrices)
            {
                var isAllowed = (bool)roleMatrix.GetType().GetProperty(propUserAction).GetValue(roleMatrix, null);
                if (isAllowed)
                    return isAllowed;
            }

            return false;
        }

        public async Task<List<ResponseUserMatrix>> Get(int userId)
        {
            await ValidateInputAsync(userId);
            if (!await userRepository.CheckIfExist(userId))
            {
                var message = await GetMessage(LangCodes.NotFound);
                throw new ApiException($"{message}. User ID {userId}");
            }

            var result = await userMatrixRepository.GetAsync(userId);
            var menus = await languageRepository.GetAsync(x => x.Type == "Menu");
            if (menus == null || menus.Count == 0)
                return result;


            foreach (var item in result)
            {
                var lang = menus.FirstOrDefault(x => x.Code == item.MenuId);
                if (lang == null)
                    continue;

                item.Id = lang.Id;
                item.En = lang.En;
            }

            return result;
        }

        public async Task UpsertDelete(RequestUserMatrix request)
        {
            await ValidateInputRequestAsync(request);

            var hasMenuDuplicate = request.Matrices.GroupBy(x => x.MenuId).Any(g => g.Count() > 1);
            if (hasMenuDuplicate)
            {
                var message = await GetMessage(LangCodes.Duplicate);
                throw new ApiException($"{message}. @Menu ID");
            }

            var menuValid = await menuRepository.CheckIfExist(request.Matrices.Select(x => x.MenuId).ToList());
            if (!menuValid)
            {
                var message = await GetMessage(LangCodes.InputInvalid);
                throw new ApiException($"{message}. @Menu ID");
            }

            var entities = new List<UserMatrix>();
            foreach (var m in request.Matrices)
            {
                var um = new UserMatrix
                {
                    UserId = request.UserId,
                    MenuId = m.MenuId,
                    IsInsert = m.IsInsert,
                    IsUpdate = m.IsUpdate,
                    IsDelete = m.IsDelete,
                    IsRead = m.IsRead,
                };

                entities.Add(um);
            }

            await userMatrixRepository.DeleteInsert(entities);
        }
    }
}