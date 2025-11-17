public class InvoiceLineVm
{
    public int ItemId { get; set; }
    public decimal Quantity { get; set; } = 1m;
    public decimal Price { get; set; } = 0m;      // καθαρή τιμή (χωρίς ΦΠΑ)
    public decimal VatRate { get; set; } = 0.24m; // π.χ. 0.24 για 24%
    public string? Note { get; set; }             // <-- ΝΕΟ
    public decimal LineTotal => Math.Round(Quantity * Price * (1 + VatRate), 2);
}
