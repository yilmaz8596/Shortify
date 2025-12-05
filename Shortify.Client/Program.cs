using Shortify.Data;
using Microsoft.EntityFrameworkCore;
using Shortify.Client.Data;
using Shortify.Data.Services;
using Microsoft.AspNetCore.Identity;
using Shortify.Data.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure the AppDbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container
builder.Services.AddScoped<IUrlService, UrlService>();
builder.Services.AddScoped<IUserService, UserService>();

// Fix AutoMapper registration
builder.Services.AddAutoMapper(typeof(Program));

// Configure Authentication 
// 1. Add identity service with configuration
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Lockout settings 
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);

    // Signin settings 
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// 2. Add External Authentication Providers (Google, GitHub)
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
        options.CallbackPath = "/signin-google";
    })
     .AddGitHub(options =>
     {
         options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"] ?? "";
         options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"] ?? "";
         options.CallbackPath = "/signin-github";
         options.Scope.Add("user:email");
     });

// 3. Configure the application cookie 
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Authentication/Login";
    options.SlidingExpiration = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Add Authentication before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

DbSeeder.SeedDefaultUsersAndRolesAsync(app).Wait();

app.Run();
