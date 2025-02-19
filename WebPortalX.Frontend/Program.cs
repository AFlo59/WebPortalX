using Microsoft.AspNetCore.Authentication.Cookies;
using WebPortalX.Frontend.Middleware;
using WebPortalX.Frontend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Services de base
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// Configuration du client HTTP
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("http://localhost:5165/");
});

// Services personnalisés
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configuration des sessions
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Configuration de l'authentification
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// Configuration CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowApiOrigin",
        policyBuilder => policyBuilder
            .WithOrigins("http://localhost:5165")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var app = builder.Build();

// Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseCors("AllowApiOrigin");

app.UseRouting();

// Middleware d'authentification dans le bon ordre
app.UseAuthentication();
app.UseAuthorization();
app.UseCustomAuthentication();

app.UseErrorHandling();

app.MapRazorPages();

app.Run();
