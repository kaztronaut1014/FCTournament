using FCTournament.Models;
using FCTournament.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
                                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
.AddDefaultTokenProviders()
.AddDefaultUI()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();
// Add services to the container.
builder.Services.AddControllersWithViews();

// Kích hoạt tính năng lưu Cache trên RAM
builder.Services.AddMemoryCache();

builder.Services.AddScoped<ILocationRepository, EFLocationRepository>();
builder.Services.AddScoped<IFormatRepository, EFFormatRepository>();
builder.Services.AddScoped<ITypeRepository, EFTypeRepository>();
builder.Services.AddScoped<ITournamentRepository, EFTournamentRepository>();
builder.Services.AddScoped<IProgressRepository, EFProgressRepository>();
builder.Services.AddScoped<IStatusRepository, EFStatusRepository>();
builder.Services.AddScoped<FCTournament.Repository.EFManagerRepository>();
builder.Services.AddScoped<FCTournament.Repository.EFOrganizerRepository>();
builder.Services.AddScoped<ITeamRepository, EFTeamRepository>();
builder.Services.AddScoped<IPlayerRepository, EFPlayerRepository>();
builder.Services.AddScoped<ITournamentTeamRepository, EFTournamentTeamRepository>();
builder.Services.AddScoped<IMatchRepository, EFMatchRepository>();

var app = builder.Build();

// --- BẮT ĐẦU ĐOẠN CODE KHỞI TẠO ADMIN ---
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Tạo các quyền (Roles)
    string[] roleNames = { "Admin", "User" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Tạo tài khoản Admin mặc định
    var adminEmail = "admin@gmail.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Quản Trị Hệ Thống",
            EmailConfirmed = true
        };
        // Mật khẩu cho Admin là: Admin@123
        var createPowerUser = await userManager.CreateAsync(adminUser, "Admin@123");
        if (createPowerUser.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())

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

app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
