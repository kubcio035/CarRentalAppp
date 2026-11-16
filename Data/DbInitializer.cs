using Microsoft.AspNetCore.Identity;
using CarRentalApp.Models;
using Bogus;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarRentalApp.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Pobieramy kontekst bazy danych potrzebny do aut i kategorii
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Tworzymy Role, jeśli nie istnieją
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Tworzymy Administratora (jeśli nie istnieje)
            var adminEmail = "admin@admin.pl";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var newAdmin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                var result = await userManager.CreateAsync(newAdmin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }

            // --- SEKCJA BOGUS: GENEROWANIE I CZYSZCZENIE DANYCH ---

            // DODANE: Czyścimy stare rezerwacje i auta przy każdym starcie
            if (context.Reservations.Any())
            {
                context.Reservations.RemoveRange(context.Reservations);
            }
            if (context.Cars.Any())
            {
                context.Cars.RemoveRange(context.Cars);
            }
            await context.SaveChangesAsync();


            // 3. Sprawdzamy czy są kategorie. Jeśli nie, dodajemy podstawowe.
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Osobowe" },
                    new Category { Name = "SUV" },
                    new Category { Name = "Dostawcze" },
                    new Category { Name = "Sportowe" }
                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // 4. Ponieważ wyczyściliśmy auta wyżej, teraz na pewno wygenerują się na nowo.
            if (!context.Cars.Any())
            {
                var categoryIds = await context.Categories.Select(c => c.Id).ToListAsync();

                // Pula prawdziwych aut
                var carTemplates = new[]
                {
                    new { Make = "Toyota", Model = "Corolla" },
                    new { Make = "Volkswagen", Model = "Golf" },
                    new { Make = "Ford", Model = "Mustang" },
                    new { Make = "BMW", Model = "M3" },
                    new { Make = "Audi", Model = "A4" },
                    new { Make = "Mercedes-Benz", Model = "Klasa C" },
                    new { Make = "Skoda", Model = "Octavia" },
                    new { Make = "Porsche", Model = "911" }
                };

                // Konfiguracja generatora Bogus
                var carRules = new Faker<Car>("pl")
                    .RuleFor(c => c.Year, f => f.Random.Int(2015, 2024))           // Losowy rok
                    .RuleFor(c => c.PricePerDay, f => Math.Round(f.Random.Decimal(150, 800), 2)) // Losowa cena
                    .RuleFor(c => c.CategoryId, f => f.PickRandom(categoryIds))    // Losowa kategoria
                                                                                   // ZMIANA: Przypisujemy tylko markę i model. ImageUrl zostawiamy jako puste (null)
                    .FinishWith((f, c) =>
                    {
                        var template = f.PickRandom(carTemplates);
                        c.Make = template.Make;
                        c.Model = template.Model;
                        c.ImageUrl = null;
                    });

                // Generujemy 20 spójnych samochodów
                var fakeCars = carRules.Generate(20);

                context.Cars.AddRange(fakeCars);
                await context.SaveChangesAsync();
            }
        }
    }
}