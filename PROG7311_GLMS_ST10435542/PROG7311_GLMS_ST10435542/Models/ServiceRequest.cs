using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROG7311_GLMS_ST10435542.Models
{
    public class ServiceRequest
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Contract")]
        [Required]
        public int ContractId { get; set; }
        public virtual Contract? Contract { get; set; }
        public int ClientId { get; set; } // to link the service request to the client for easier querying in the controller
        public virtual Client? Client { get; set; } // navigation property to access client details directly from the service request

        [Required]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; } // ZAR amount
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalCost { get; set; } // USD amount

        [Required]
        public string Status { get; set; } = "Pending"; // default status when a service request is created

        public DateTime RequestDate { get; set; } // the date and time when the service request was created

        [Required]
        public string PickupAddress { get; set; } // where the driver picks up the package

        [Required]
        public string DeliveryAddress { get; set; } // where the driver drops off the package

        [Required]
        public string RecipientName { get; set; } // the person receiving the package

        [Required]
        public string RecipientPhone { get; set; } // contact number for the recipient

        [Required]
        public string SlaType { get; set; } = "Standard"; // determines if this is normal or express delivery

        [Required]
        public string PackageSizeCategory { get; set; } // size category of the package (1 - 5kg, 5 - 10kg, 10kg+)
        // Im using this approach because allowing the user to enter an amount is not logical, so instead they select a size category which then determines the cost of the delivery in the currency strategy

        public string? AssignedDriverId { get; set; } // id of the driver assigned to this delivery (nullable until admin assigns one)
    }
}