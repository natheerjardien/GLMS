using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        // NOTE (Part 3): the IFormFile upload property lives only in the frontend's model now.
        // Keeping IFormFile here made [ApiController] infer the whole parameter as [FromForm],
        // which silently broke JSON binding on POST /api/contracts.
        public string? SignedAgreementFilePath { get; set; }
        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}