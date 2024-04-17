using FutureCareer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using FutureCareer.Services;
using FutureCareer.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Azure Storage service
builder.Services.AddSingleton<IAzureStorage, AzureStorage>();
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApp(builder.Configuration);

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.AddRazorPages().AddMvcOptions(options =>
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.Filters.Add(new AuthorizeFilter(policy));
}).AddMicrosoftIdentityUI();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{

}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
// Handle file uploads
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/Students/Upload") && context.Request.Method == "POST")
    {
        // Ensure the request has form content type
        if (!context.Request.HasFormContentType)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Request must be of type multipart/form-data.");
            return;
        }

        // Get the file from the request
        var form = await context.Request.ReadFormAsync();
        var file = form.Files["file"];

        // Check if file exists and is not empty
        if (file == null || file.Length == 0)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("No file selected or empty file.");
            return;
        }

        // Use the AzureStorage service to upload the file
        var azureStorage = context.RequestServices.GetRequiredService<IAzureStorage>();
        var response = await azureStorage.UploadAsync(file);

        // Check for errors during upload
        if (response.Error)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(response.Status);
            return;
        }

        // Return success response
        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.WriteAsync("File uploaded successfully.");
    }
    else
    {
        await next();
    }
});
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();