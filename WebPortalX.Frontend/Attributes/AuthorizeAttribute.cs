using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class AuthorizeAttribute : TypeFilterAttribute
{
    public AuthorizeAttribute() : base(typeof(AuthorizeFilter))
    {
    }
}

public class AuthorizeFilter : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.User.Identity.IsAuthenticated)
        {
            context.Result = new RedirectToPageResult("/Account/Login");
        }
    }
} 