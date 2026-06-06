using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROG7311_GLMS_ST10435542.Models
{
    public class Contract
    {
        [Key]
        public int Id { get; set; }

        public int ClientId { get; set; }
        public Client? Client { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string ServiceLevel { get; set; }

        [Required]
        public string Status { get; set; }

        [NotMapped] // tells the db not to create a column for this property
        public IFormFile? ContractFile { get; set; }

        public string? SignedAgreementFilePath { get; set; }
        public List<ServiceRequest> ServiceRequests { get; set; } = new();
    }
}