using EX_Parcial_VilchezGuardia_JF.Data;
using EX_Parcial_VilchezGuardia_JF.Services;
using StackExchange.Redis; // 👈 agregado para probar conexión a Redis
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // 👈 necesario para UseSqlite

var builder = WebApplication.CreateBuilder(args);

// -----------------
// Services
// -----------------
builder.Services.AddControllersWithViews();
// Razor Pages (necessary for Identity UI pages)
builder.Services.AddRazorPages();

// DbContext con SQLite (usa ApplicationDbContext)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=app.db"));

// Identity con Roles y cuentas
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Sesiones + Cache en memoria (fallback si Redis no está configurado)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// Register a no-op email sender for Identity UI when no SMTP is configured
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EX_Parcial_VilchezGuardia_JF.Services.NullEmailSender>();

// -----------------
// Redis (si está configurado) - sólo registrar si la conexión funciona
// -----------------
var redisConn = builder.Configuration["Redis:ConnectionString"] ?? builder.Configuration["Redis__ConnectionString"];

// 👇 NUEVO: construir connection string si tenemos Host/Port/Password separados en appsettings.json
if (string.IsNullOrEmpty(redisConn))
{
    var redisHost = builder.Configuration["Redis:Host"];
    var redisPort = builder.Configuration["Redis:Port"];
    var redisUser = builder.Configuration["Redis:User"];
    var redisPassword = builder.Configuration["Redis:Password"];

    if (!string.IsNullOrEmpty(redisHost) && !string.IsNullOrEmpty(redisPort) && !string.IsNullOrEmpty(redisPassword))
    {
        redisConn = $"{redisHost}:{redisPort},password={redisPassword},user={redisUser},ssl=True,abortConnect=False";
    }
}

if (!string.IsNullOrEmpty(redisConn))
{
    try
    {
        // Parsear la cadena para ajustar opciones (SSL, timeouts) si es necesario.
        var cfg = ConfigurationOptions.Parse(redisConn);
        cfg.AbortOnConnectFail = false;
        cfg.ConnectTimeout = 10000; // 10s
        cfg.SyncTimeout = 5000;

        if (!cfg.Ssl)
        {
            cfg.Ssl = true; // Redis Cloud siempre requiere TLS
        }

        var testMuxer = ConnectionMultiplexer.Connect(cfg);
        if (testMuxer != null && testMuxer.IsConnected)
        {
            builder.Services.AddSingleton<IConnectionMultiplexer>(testMuxer);

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cfg.ToString();
            });

            Console.WriteLine("[Startup] Redis disponible: usando Redis para IDistributedCache.");
        }
        else
        {
            Console.WriteLine("[Startup] No se pudo conectar a Redis: falló la validación inicial. Usando caché en memoria.");
            testMuxer?.Dispose();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Redis NO configurado (falló conexión de prueba): {ex.GetType().Name}: {ex.Message}");
    }
}

// -----------------
//  Redis por Host/Port/User/Password
// -----------------
var redisSection = builder.Configuration.GetSection("Redis");
var host = redisSection.GetValue<string>("Host");
var port = redisSection.GetValue<int>("Port");
var user = redisSection.GetValue<string>("User");
var password = redisSection.GetValue<string>("Password");

if (!string.IsNullOrEmpty(host) && port > 0 && !string.IsNullOrEmpty(password))
{
    var redisConfig = new ConfigurationOptions
    {
        AbortOnConnectFail = false,
        Ssl = true,             // Redis Cloud requiere TLS
        ConnectTimeout = 10000,
        SyncTimeout = 5000
    };
    redisConfig.EndPoints.Add(host, port);
    if (!string.IsNullOrEmpty(user)) redisConfig.User = user;
    redisConfig.Password = password;

    try
    {
        var muxer = ConnectionMultiplexer.Connect(redisConfig);
        if (muxer.IsConnected)
        {
            builder.Services.AddSingleton<IConnectionMultiplexer>(muxer);
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.ConfigurationOptions = redisConfig;
            });
            Console.WriteLine("[Startup] Redis configurado desde Host/Port/User/Password en appsettings.json.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Redis NO configurado (falló conexión de prueba con Host/Port): {ex.GetType().Name}: {ex.Message}");
    }
}

var app = builder.Build();

// -----------------
// DB + Roles iniciales
// -----------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        db.Database.Migrate();
    }
    catch (InvalidOperationException ex) when (ex.Message?.Contains("PendingModelChangesWarning") == true || ex.Message?.Contains("pending changes") == true)
    {
        Console.WriteLine("[Startup] WARNING: Hay cambios pendientes en el modelo de datos. No se aplicaron migraciones automáticas.");
        Console.WriteLine($"[Startup] Detalle: {ex.Message}");
        Console.WriteLine("[Startup] Para resolverlo localmente ejecuta: dotnet ef migrations add NombreDeLaMigracion && dotnet ef database update");
    }

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

// -----------------
// Rutas
// -----------------
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 🔑 IMPORTANTE: habilitar Razor Pages (para Identity UI)
app.MapRazorPages();

// -----------------
// Test de Redis en modo desarrollo
// -----------------
if (app.Environment.IsDevelopment() && !string.IsNullOrEmpty(redisConn))
{
    try
    {
        var muxer = ConnectionMultiplexer.Connect(redisConn);
        var db = muxer.GetDatabase();
        db.StringSet("foo", "bar");
        var result = db.StringGet("foo");
        Console.WriteLine($"[Redis Test] foo = {result}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Redis Test] No se pudo conectar a Redis: {ex.GetType().Name}: {ex.Message}");
    }
}

// -----------------
// 🔐 Login rápido para desarrollo
// -----------------
app.MapGet("/login-coordinador", async (UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, HttpContext ctx) =>
{
    var user = await userManager.FindByEmailAsync("coordinador@uni.com");
    if (user != null)
    {
        await signInManager.SignInAsync(user, isPersistent: true);
        await ctx.Response.WriteAsync("Sesión iniciada como coordinador@uni.com ✅. Ve a /Coordinador/Cursos");
    }
    else
    {
        await ctx.Response.WriteAsync("Usuario coordinador no encontrado ❌.");
    }
});

app.Run();



