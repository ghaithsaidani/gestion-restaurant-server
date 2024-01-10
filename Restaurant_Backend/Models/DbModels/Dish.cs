using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant_Backend.Models.DbModels
{
    public class Dish
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(250)")]
        public string Title { get; set; } = "";
        [Required]
        [Column(TypeName = "nvarchar(250)")]
        public string Type { get; set; } = "";
        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Description { get; set; } = "";
        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Image { get; set; } = "";


    }
}
