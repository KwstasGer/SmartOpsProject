using SmartOps.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartOps.ViewModels
{
    public class InvoiceCreateVm
    {
        // Τύπος παραστατικού: "Items", "Services", "Purchases"
        [Required, StringLength(20)]
        public string InvoiceType { get; set; } = "Items";

        [Required]
        public string Series { get; set; } = "TIM";

        [Required]
        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; } = DateTime.Today;

        // Για πωλήσεις = Πελάτης, για αγορές = Προμηθευτής
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        // Αν θες να το χρησιμοποιήσεις στο μέλλον:
        // true = Απόδειξη Λιανικής, false = Τιμολόγιο
        public bool IsRetail { get; set; }

        // Γραμμές παραστατικού
        public List<LineVm> Lines { get; set; } = new()
        {
            new LineVm()   // μία default γραμμή
        };

        public class LineVm
        {
            // Πληροφοριακό μόνο (Item / Service)
            public string Type { get; set; } = "Item";

            // Id από Items ή Services, ανάλογα με το InvoiceType
            [Required]
            public int CatalogId { get; set; }

            [Range(0.0001, 999999)]
            public decimal Quantity { get; set; } = 1m;

            [Range(0, 999999)]
            public decimal UnitPrice { get; set; } = 0m;

            // ΦΠΑ ως ΠΟΣΟΣΤΟ (0–100). Π.χ. 24 = 24%
            [Range(0, 100)]
            public decimal VatRate { get; set; } = 24m;
        }

        // Λίστα ειδών/υπηρεσιών που εμφανίζονται στο dropdown
        public List<CatalogItemVm> CatalogItems { get; set; } = new();

        public class CatalogItemVm
        {
            public string Type { get; set; } = "Item"; // "Item" ή "Service"
            public int Id { get; set; }                // Id από Items ή Services
            public string Code { get; set; }           // ItemCode ή ServiceCode
            public string Description { get; set; }    // Περιγραφή

            // Τιμές από τα Items/Services
            public decimal? RetailPrice { get; set; }      // Τιμή Λιανικής
            public decimal? WholesalePrice { get; set; }   // Τιμή Χονδρικής

            // ΦΠΑ ως ποσοστό (0–100). Π.χ. 24 = 24%
            public decimal VatRate { get; set; }
        }
    }
}
