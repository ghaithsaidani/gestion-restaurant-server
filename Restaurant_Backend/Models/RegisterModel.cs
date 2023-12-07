using System.ComponentModel.DataAnnotations.Schema;

namespace Server_Side.Models
{
    public class RegisterModel
    {
        public string Nom { get; set; } = "";
        public string Prenom { get; set; } = "";
        public string Telephone { get; set; } = "";
        public string Adresse { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
