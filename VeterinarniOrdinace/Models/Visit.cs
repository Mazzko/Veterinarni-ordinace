using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeterinarniOrdinace.Models
{
    public class Visit
    {
        public int Id { get; set; }
        public int AnimalId { get; set; } //FK
        public DateTime Date { get; set; } = DateTime.Now;
        public string Reason { get; set; } = String.Empty;
        public string? Diagnosis { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = "Naplánováno"; // Naplánováno / Dokončeno / Zrušeno
    }
}
