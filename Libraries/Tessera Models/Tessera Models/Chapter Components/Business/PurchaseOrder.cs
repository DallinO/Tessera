using System.ComponentModel.DataAnnotations;
using Tessera.CodeGenerators;

namespace Tessera.Models
{
    public class PurchaseOrderEntity
    {
        [Key]
        public int Id { get; set; }
        // FOREIGN KEY
        public int ServiceCallId { get; set; }
        public string OrderId { get; set; } = "PC-" + (DateTime.Now.Year % 100) + CodeGen.StringOfDigits(3);
        public string Description { get; set; }
        public decimal Cost { get; set; }
    }

    public class PurchaseOrderModel
    {
        [Required(ErrorMessage = "Description is required")]
        [StringLength(999, ErrorMessage = "Description cannot be greater than 1000 characters")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Cost is required")]
        [Range(0, 999999999.99)]
        public decimal Cost { get; set; }
    }

    public class PurchaseOrderDto
    {
        public string OrderId { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
    }

}
