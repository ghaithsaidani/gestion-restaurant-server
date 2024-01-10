using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Restaurant_Backend.Models.DbModels
{
    public class Table
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public int Places { get; set; } = 0;
        [Required]
        public bool IsReserved { get; set; } = false;

    }
}
