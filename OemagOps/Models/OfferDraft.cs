namespace OemagOps.Models
{
    public class OfferDraft
    {
        public string CustomerName { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal ProductTotal { get; set; }

        public decimal DeliveryCost { get; set; }

        public decimal GrandTotal { get; set; }
    }
}