using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http; // needed for IFormFile

namespace PROG7311_GLMS_API.Models
{
    public class Contract
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Client")]
        public int? ClientId { get; set; }
        public virtual Client? Client { get; set; }

        [Required]
        public DateTime? StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string? ServiceLevel { get; set; }

        [Required]
        public string Status { get; set; } = "Draft"; // default status when a contract is created

        [NotMapped] // tells the db to ignore this property because it is just for handling the active upload stream
        public IFormFile? ContractFile { get; set; }

        public string? SignedAgreementFilePath { get; set; }
        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}