namespace Server.Model
{
    public record QuoteMessage
    {
        public long Id { get; set; }
        public double Value { get; set; }
    }
}
