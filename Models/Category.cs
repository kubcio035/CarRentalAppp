using System.ComponentModel.DataAnnotations;

namespace CarRentalApp.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Podaj nazwę kategorii")]
        [Display(Name = "Nazwa Kategorii")]
        public string Name { get; set; } = "";

        // Relacja: Jedna kategoria -> Wiele aut
        public virtual ICollection<Car>? Cars { get; set; }
    }
}