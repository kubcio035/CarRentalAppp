using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalApp.Models
{
    public class Car
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Marka jest wymagana")]
        [Display(Name = "Marka")]
        public string Make { get; set; } = "";

        [Required(ErrorMessage = "Model jest wymagany")]
        [Display(Name = "Model")]
        public string Model { get; set; } = "";

        [Range(2000, 2100, ErrorMessage = "Rok: 2000-2100")]
        [Display(Name = "Rok produkcji")]
        public int Year { get; set; }

        [Range(1, 10000, ErrorMessage = "Cena musi być dodatnia")]
        [Column(TypeName = "decimal(18,2)")] // Wymagane dla waluty
        [Display(Name = "Cena za dobę")]
        public decimal PricePerDay { get; set; }

        [Display(Name = "Link do zdjęcia")]
        public string? ImageUrl { get; set; }

        // Relacja: Auto należy do Kategorii
        [Display(Name = "Kategoria")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }
    }
}