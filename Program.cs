using CarRentalApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// --- TO NAPRAWIA B£¥D DATY W POSTGRESQL ---
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
// ------------------------------------------

var builder = WebApplication.CreateBuilder(args);

// 1. Konfiguracja Bazy Danych
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Konfiguracja Identity (U¿ytkownicy i Role)
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false; // U³atwienie: nie wymagamy potwierdzenia emaila
    options.Password.RequireDigit = false;          // U³atwienie: proste has³a 
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3;
})
    .AddRoles<IdentityRole>() // WA¯NE: W³¹czamy obs³ugê ról (Admin/User)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 3. Konfiguracja potoku HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 4. W³¹czamy uwierzytelnianie (Kto to jest?) i autoryzacjê (Co mo¿e robiæ?)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// To uruchamia gotowe widoki logowania/rejestracji
app.MapRazorPages();
// --- SEKCJA INICJALIZACJI DANYCH ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Wywo³ujemy nasz¹ metodê tworz¹c¹ role i admina
        await DbInitializer.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Wyst¹pi³ b³¹d podczas tworzenia ról i admina.");
    }
}
// -----------------------------------

app.Run(); 

