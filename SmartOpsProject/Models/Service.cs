using System.ComponentModel.DataAnnotations;

namespace SmartOpsProject.Models
{
    public class Service
    {
        public int ServiceId { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        // Σχέση με InvoiceLine (προαιρετική)
        // public ICollection<InvoiceLine>? InvoiceLines { get; set; }
    }

}
