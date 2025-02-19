using WebPortalX.Frontend.Middleware;
using WebPortalX.Frontend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configuration du client HTTP
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("http://localhost:5165/");  // Port de l'API
});

// Configuration CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowApiOrigin",
        policyBuilder => policyBuilder
            .WithOrigins("http://localhost:5165")  // Port de l'API
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.Services.AddScoped<IApiService, ApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowApiOrigin");
app.UseRouting();

// Ajout de notre middleware d'authentification personnalisé
app.UseCustomAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
