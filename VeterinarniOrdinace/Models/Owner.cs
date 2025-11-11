using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeterinarniOrdinace.Models
{
    public class Owner
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone {  get; set; }
        public string? Email { get; set; }
        public string? Address {  get; set; }
    }
}
