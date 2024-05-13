using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tourism;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<TourismDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString(nameof(TourismDbContext))));
builder.Services.AddIdentity<User, IdentityRole<int> >(options=>
{
    var allowed = options.User.AllowedUserNameCharacters 
                  + "АаБбВвГгҐґДдЕеЄєЖжЗзИиІіЇїЙйКкЛлМмНнОоПпРрСсТтУуФфХхЦцЧчШшЩщЬьЮюЯя/*()# ";
    options.User.AllowedUserNameCharacters = allowed;
}).AddEntityFrameworkStores<TourismDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/AccessDenied/Index";
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<User>>();
        var rolesManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        await RoleInitializer.InitializeAsync(userManager, rolesManager);
    }
    catch(Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database."+DateTime.Now.ToString());
    }
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

app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    
    endpoints.MapControllerRoute(
        name: "AccessDeniedRoute",
        pattern: "AccessDenied",
        defaults: new { controller = "AccessDenied", action = "Index" }
    );
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
