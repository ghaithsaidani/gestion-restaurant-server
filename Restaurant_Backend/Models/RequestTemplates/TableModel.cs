using System.ComponentModel.DataAnnotations;

namespace Restaurant_Backend.Models.RequestTemplates
{
    public class TableModel
    {
        public int? Places { get; set; }
        public bool? IsReserved { get; set; }
    }
}
