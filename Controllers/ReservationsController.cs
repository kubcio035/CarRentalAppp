using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CarRentalApp.Data;
using CarRentalApp.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CarRentalApp.Controllers
{
    [Authorize] // Każdy musi być zalogowany, żeby tu wejść
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // LOGIKA BIZNESOWA:
            // Jeśli Admin -> pobierz wszystkie rezerwacje
            // Jeśli Zwykły Użytkownik -> pobierz tylko MOJE rezerwacje
            IQueryable<Reservation> reservations = _context.Reservations.Include(r => r.Car);

            if (!isAdmin)
            {
                reservations = reservations.Where(r => r.UserId == userId);
            }

            return View(await reservations.ToListAsync());
        }

        // GET: Reservations/Create
        public IActionResult Create()
        {
            // Pozwalamy wybrać tylko auto
            ViewData["CarId"] = new SelectList(_context.Cars, "Id", "Make");
            // Model samochodu byłby lepszy w liście, ale na razie wystarczy Marka
            return View();
        }

        // POST: Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StartDate,EndDate,TotalCost,CarId")] Reservation reservation)
        {
            // AUTOMATYCZNE PRZYPISANIE UŻYTKOWNIKA
            reservation.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Prosta walidacja (dla punktów) - usuwamy walidację UserId, bo ustawiamy go ręcznie
            ModelState.Remove("UserId");
            ModelState.Remove("Car");

            if (ModelState.IsValid)
            {
                _context.Add(reservation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CarId"] = new SelectList(_context.Cars, "Id", "Make", reservation.CarId);
            return View(reservation);
        }

        // --- Pozostałe metody (Edit/Delete) zostawiamy dla Admina lub do usunięcia ---

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var reservation = await _context.Reservations
                .Include(r => r.Car)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null) return NotFound();

            return View(reservation);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null) _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // --- RAPORT FINANSOWY (Wymaga roli Admin) ---
        // Teraz jest poprawnie WEWNĄTRZ klasy
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Report()
        {
            // ZAAWANSOWANE ZAPYTANIE LINQ (To daje punkty!)
            // 1. Pobierz rezerwacje z autami i kategoriami
            // 2. Pogrupuj po nazwie kategorii
            // 3. Dla każdej grupy policz sumę kosztów i ilość rezerwacji

            var reportData = await _context.Reservations
                .Include(r => r.Car)
                .ThenInclude(c => c.Category)
                .GroupBy(r => r.Car.Category.Name)
                .Select(g => new ReportViewModel
                {
                    CategoryName = g.Key,
                    ReservationCount = g.Count(),
                    TotalRevenue = g.Sum(r => r.TotalCost)
                })
                .ToListAsync();

            return View(reportData);
        }
    } // <-- Tu zamykamy klasę
} // <-- Tu zamykamy namespace