namespace Server.Config
{
    public class ServerConfig
    {
        public required string MulticastAddress { get; set; }
        public int Port { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public int Decimals { get; set; }
        public int IntervalMicroseconds { get; set; }
    }
}
