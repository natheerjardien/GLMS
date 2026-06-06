using System.ComponentModel.DataAnnotations;

namespace PROG7311_GLMS_ST10435542.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string ContactDetails { get; set; }

        [Required]
        public string Region { get; set; }

        public List<Contract> Contracts { get; set; } = new();
    }
}