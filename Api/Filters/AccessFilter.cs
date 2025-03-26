using Api.Domain;
using Api.Domain.Attributes;
using Api.Services.Masters;
using Api.Services.Systems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Docubase.api.Filters
{
    public class AccessFilter(
        IHttpContextAccessor accessor,
        IConfiguration configuration,
        UserMatrixService matrixService
        ) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            var anonymAct = Attribute.IsDefined(actionDescriptor.MethodInfo, typeof(AllowAnonymousAttribute));
            var anonymCnt = Attribute.IsDefined(context.Controller.GetType(), typeof(AllowAnonymousAttribute));
            var bypassAct = Attribute.IsDefined(actionDescriptor.MethodInfo, typeof(BypassAccessAttribute));
            var bypassCnt = Attribute.IsDefined(context.Controller.GetType(), typeof(BypassAccessAttribute));

            if (anonymAct || anonymCnt || bypassAct || bypassCnt)
            {
                await next();
                return;
            }

            //if (!Debugger.IsAttached)
            //    CheckHeaders();

            var menuActionAttribute = context.ActionDescriptor.EndpointMetadata.FirstOrDefault(em => em.GetType() == typeof(MenuAttribute)) as MenuAttribute;
            var menuControllerAttribute = context.Controller.GetType().GetCustomAttributes(typeof(MenuAttribute), false).FirstOrDefault() as MenuAttribute;

            var menuId = menuControllerAttribute?.MenuId;
            if (!string.IsNullOrWhiteSpace(menuActionAttribute?.MenuId)) //the winner is always menu from action instead of controller
                menuId = menuActionAttribute.MenuId;

            if (string.IsNullOrWhiteSpace(menuId))
            {
                context.HttpContext.Response.StatusCode = 500;
                context.HttpContext.Response.ContentType = "application/json";
                context.Result = new JsonResult(new { status = 500, message = "Menu ID is null or empty." });
                return;
            }

            var userActionAttribute = context.ActionDescriptor.EndpointMetadata.FirstOrDefault(em => em.GetType() == typeof(UserActionAttribute)) as UserActionAttribute;//(context.ActionDescriptor as ControllerActionDescriptor).MethodInfo.Attributes.GetAttributeOfType<UserActionAttribute>();
            if (userActionAttribute == null)
            {
                context.HttpContext.Response.StatusCode = 500;
                context.HttpContext.Response.ContentType = "application/json";
                context.Result = new JsonResult(new { status = 500, message = "User action is null or empty." });
                return;
            }

            var name = accessor?.HttpContext?.User?.Identity?.Name;
            var id = name == null ? 0 : int.Parse(name);
            var hasAccess = await matrixService.HasAccess(id, menuId, userActionAttribute.Action);
            if (!hasAccess)
            {
                context.HttpContext.Response.StatusCode = 403;
                context.HttpContext.Response.ContentType = "application/json";
                context.Result = new JsonResult(new { status = 403, message = "unauthorized-access" });
                return;
            }

            await next();
        }

        private void CheckHeaders()
        {
            if (!accessor.HttpContext.Request.Headers.TryGetValue("x-api-key", out var apiKey) || string.IsNullOrWhiteSpace(apiKey))
                throw new ApiException("api-key");

            var key = configuration["Authentication:APIKey"];
            if (apiKey.ToString() != key)
                throw new ApiException("api-key");

            //if (!accessor.HttpContext.Request.Headers.TryGetValue("x-tenant-id", out var tenantId) || string.IsNullOrWhiteSpace(tenantId))
            //    throw new ApiException("Invalid tenant");
        }
    }
}
