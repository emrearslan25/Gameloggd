using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthorization(options =>
{
    // Require authentication for all endpoints by default.
    // Individual actions/controllers can opt out with [AllowAnonymous].
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddDbContext<GameLoggd.Data.ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default")
        ?? "Data Source=App_Data/gameloggd.db";
    options.UseSqlite(connectionString);
});

builder.Services
    .AddIdentity<GameLoggd.Models.ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        // Keep password rules reasonable but not overly strict for this app.
        // The default Identity settings require non-alphanumeric/upper/lower/digit and can make "Create account" look like it does nothing
        // unless errors are surfaced in the UI.
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<GameLoggd.Data.ApplicationDbContext>()
    ;

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account";
    options.AccessDeniedPath = "/account";

    options.Events.OnValidatePrincipal = async context =>
    {
        var userManager = context.HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<GameLoggd.Models.ApplicationUser>>();
        var signInManager = context.HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.Identity.SignInManager<GameLoggd.Models.ApplicationUser>>();

        if (context.Principal is null) return;

        var userId = userManager.GetUserId(context.Principal);
        if (string.IsNullOrWhiteSpace(userId)) return;

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return;

        if (user.IsBanned)
        {
            context.RejectPrincipal();
            await signInManager.SignOutAsync();
        }
    };
});

var app = builder.Build();

// DB init + seed
using (var scope = app.Services.CreateScope())
{
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    var db = scope.ServiceProvider.GetRequiredService<GameLoggd.Data.ApplicationDbContext>();
    Directory.CreateDirectory(Path.Combine(env.ContentRootPath, "App_Data"));
    db.Database.Migrate();

    var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<GameLoggd.Models.ApplicationUser>>();

    const string adminRole = "Admin";
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole(adminRole));
    }

    var adminEmail = app.Configuration["SeedAdmin:Email"];
    var adminUsername = app.Configuration["SeedAdmin:Username"];
    var adminPassword = app.Configuration["SeedAdmin:Password"];

    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminUsername) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        var existing = await userManager.FindByEmailAsync(adminEmail);
        if (existing is null)
        {
            var adminUser = new GameLoggd.Models.ApplicationUser
            {
                UserName = adminUsername,
                Email = adminEmail,
                EmailConfirmed = true,
            };

            var create = await userManager.CreateAsync(adminUser, adminPassword);
            if (create.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }
        }
    }

    // Auto-fix missing slugs for existing games
    var gamesWithoutSlug = await db.Games.Where(g => g.Slug == "" || g.Slug == null).ToListAsync();
    if (gamesWithoutSlug.Any())
    {
        foreach (var game in gamesWithoutSlug)
        {
            game.Slug = GameLoggd.Controllers.AdminController.GenerateSlugStatic(game.Title);
        }
        await db.SaveChangesAsync();
        await db.SaveChangesAsync();
    }

    await GameLoggd.Data.DataSeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
