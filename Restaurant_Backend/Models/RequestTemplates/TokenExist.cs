using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant_Backend.Models.RequestTemplates
{
    public class TokenExist
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Token { get; set; } = "";
    }
}
