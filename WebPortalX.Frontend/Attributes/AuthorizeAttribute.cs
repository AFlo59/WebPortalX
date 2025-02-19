using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

public class AuthorizeAttribute : Attribute, IAsyncPageFilter
{
    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var isAuthenticated = context.HttpContext.Items["IsAuthenticated"] as bool? ?? false;
        
        if (!isAuthenticated)
        {
            context.Result = new RedirectToPageResult("/Account/Login");
            return;
        }

        await next.Invoke();
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        return Task.CompletedTask;
    }
} 