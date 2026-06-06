using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROG7311_GLMS_ST10435542.Models
{
    public class ServiceRequest
    {
        [Key]
        public int Id { get; set; }

        public int ContractId { get; set; }
        public Contract? Contract { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; } // ZAR amount
        public decimal OriginalCost { get; set; } // USD amount

        [Required]
        public string Status { get; set; }
    }
}