using API.Middleware;
using API.SignalR;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<StoreContext>(opt => {
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure());
});
builder.Services.AddScoped<IProductRepository,ProductsRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUniteOfWork, UnitOfWork>();
builder.Services.AddCors();
builder.Services.AddSingleton<IConnectionMultiplexer>(config =>
{
    var connString = builder.Configuration.GetConnectionString("Redis");
    if (connString == null) throw new Exception("cannot get redis connection string");
    var configuration = ConfigurationOptions.Parse(connString, true);
    return ConnectionMultiplexer.Connect(configuration);

});
builder.Services.AddSingleton<ICartService, CartService>();
builder.Services.AddSingleton<IResponseCacheService, ResponseCacheService>();
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<AppUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<StoreContext>();


builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddSignalR();
builder.Services.AddScoped<ICouponService, CouponService>();


//for logging

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.WebHost
    .CaptureStartupErrors(true)
    .UseSetting("detailedErrors", "true");

var app = builder.Build();



app.UseMiddleware<ExceptionMiddleware>();

app.UseCors(x=>x.AllowAnyHeader().AllowAnyMethod().AllowCredentials()
    .WithOrigins("http://localhost:4200","https://localhost:4200","https://skinet-kg.azurewebsites.net/"));

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();


app.MapControllers();
app.MapGroup("api").MapIdentityApi<AppUser>(); //api/login
app.MapHub<NotificationHub>("/hub/notifications");

app.MapFallbackToController("Index","Fallback");

try
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<StoreContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();


    const int maxRetries = 3;
    var retries = 0;
    bool migrated = false;

    while (!migrated && retries < maxRetries)
    {
        try
        {
            await context.Database.MigrateAsync();
            await StoreContextSeed.SeedAsync(context, userManager, roleManager);
            migrated = true;
        }
        catch (Exception ex)
        {
            retries++;
            Console.WriteLine($"Migration failed (attempt {retries}): {ex.Message}");
            await Task.Delay(2000); // wait 2 seconds before retry
        }
    }

    if (!migrated)
    {
        Console.WriteLine("Migration failed after retries. Skipping...");
    }

}
catch (Exception ex)
{
    Console.WriteLine("Critical startup error: " + ex);

}

app.Run();
