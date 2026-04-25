using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PuestoWeb.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "DataSource=app.db";

// Soporte para SQLite en deploy si no hay Postgres configurado
var postgresConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL"); // Común en servicios de hosting

if (!string.IsNullOrEmpty(postgresConnectionString))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(postgresConnectionString));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
}

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();
builder.Services.AddControllers(); // Habilitar API Controllers

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<PuestoWeb.Services.CartService>();
builder.Services.AddScoped<PuestoWeb.Services.AIService>();
builder.Services.AddScoped<PuestoWeb.Services.OrderProcessorService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// LOG DE RED: Ver qué llega al servidor
app.Use(async (context, next) =>
{
    if (context.Request.Method == "POST")
    {
        Console.WriteLine($"[POST-DETECTADO] Ruta: {context.Request.Path}");
    }
    await next();
});

// Inicializar base de datos
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    
    try 
    {
        // Ejecutar migraciones pendientes automáticamente
        if (!app.Environment.IsDevelopment())
        {
            await context.Database.MigrateAsync();
        }
        
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await DbInitializer.Initialize(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al migrar o inicializar la base de datos.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Asegurar que las APIs no redirijan al login
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 401 && context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync("Acceso denegado a la API. Falta [AllowAnonymous] o configuración.");
    }
});

app.MapRazorPages();
app.MapControllers();

app.Run();
