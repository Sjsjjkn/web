using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Backend.Attributes
{
    /// <summary>
    /// 角色授权属性
    /// </summary>
    public class RoleAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string _role;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="role">需要的角色</param>
        public RoleAuthorizeAttribute(string role)
        {
            _role = role;
        }
        
        /// <summary>
        /// 授权过滤器方法
        /// </summary>
        /// <param name="context">授权上下文</param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }
            
            var roleClaim = user.FindFirst(ClaimTypes.Role);
            if (roleClaim == null)
            {
                context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();
                return;
            }
            
            // 管理员拥有所有权限
            if (roleClaim.Value == "Admin")
            {
                return;
            }
            
            if (roleClaim.Value != _role)
            {
                context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();
            }
        }
    }
}