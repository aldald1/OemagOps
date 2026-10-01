namespace OemagOps.Models
{
    public class IncomingRequest
    {
        public int Id { get; set; }

        public string Sender { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string Status { get; set; } = "NEW";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}