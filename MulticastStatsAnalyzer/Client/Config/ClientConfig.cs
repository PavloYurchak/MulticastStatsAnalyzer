namespace Client.Config
{
    public class ClientConfig
    {
        public required string MulticastAddress { get; set; }
        public int Port { get; set; }
        public int BufferSize { get; set; }
        public int ReceiveTimeoutMs { get; set; }
    }
}
