using System.ComponentModel.DataAnnotations;

namespace PROG7311_GLMS_API.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? ContactDetails { get; set; }

        [Required]
        public string? Region { get; set; }
        public string? ApplicationUserId { get; set; } // to link the client profile to the Identity user account

        // navigation property to link one client to many contracts
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}