using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalApp.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        [Display(Name = "Data odbioru")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "Data zwrotu")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Koszt całkowity")]
        public decimal TotalCost { get; set; }

        // Relacja: Rezerwacja dotyczy Auta
        public int CarId { get; set; }
        public virtual Car? Car { get; set; }

        // Tu będziemy wpisywać ID użytkownika po zalogowaniu
        public string? UserId { get; set; }
    }
}