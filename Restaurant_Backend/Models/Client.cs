using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Server_Side.Models
{
    public class Client
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(250)")]
        public string Nom { get; set; } = "";
        [Required]
        [Column(TypeName = "nvarchar(250)")]
        public string Prenom { get; set; } = "";
        [Required]
        [Column(TypeName = "nvarchar(250)")]
        public string Telephone { get; set; } = "";
        [Required]
        [Column(TypeName = "nvarchar(250)")]
        public string Adresse { get; set; } = "";
        [Required]
        [Column(TypeName = "nvarchar(250)")]
        public string Email { get; set; } = "";
        [Required]
        [Column(TypeName = "nvarchar(250)")]
        public string Password { get; set; } = "";
    }
}
