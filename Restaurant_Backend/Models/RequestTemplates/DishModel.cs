using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Restaurant_Backend.Models.RequestTemplates
{
    public class DishModel
    {
        public string Title { get; set; } = "";
        public string Type { get; set; } = "";
        public string Description { get; set; } = "";
        public IFormFile? Image { get; set; }
    }
}
