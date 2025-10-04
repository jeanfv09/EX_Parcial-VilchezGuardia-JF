using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using EX_Parcial_VilchezGuardia_JF.Data;

var builder = WebApplication.CreateBuilder(args);

// -----------------
// Services
// -----------------
builder.Services.AddControllersWithViews();

// DbContext con SQLite (usa ApplicationDbContext)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=parcial.db"));

// Identity con Roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Sesiones + Cache
builder.Services.AddDistributedMemoryCache(); // fallback en local
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

// Redis (si está configurado)
var redisConn = builder.Configuration["Redis__ConnectionString"];
if (!string.IsNullOrEmpty(redisConn))
{
    builder.Services.AddStackExchangeRedisCache(options =>
        options.Configuration = redisConn);
}

var app = builder.Build();

// -----------------
// DB + Roles iniciales
// -----------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Ejecutar seeding
    if (!await roleManager.RoleExistsAsync("Coordinador"))
    {
        await roleManager.CreateAsync(new IdentityRole("Coordinador"));
    }

    var adminEmail = "coordinador@uni.com";
    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        var u = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(u, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(u, "Coordinador");
        }
    }
}

// -----------------
// Middleware
// -----------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// Rutas
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

