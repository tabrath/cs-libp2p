namespace LibP2P.Abstractions.Connection
{
    public class ConnectionOptions
    {
        public static readonly ConnectionOptions Default = new ConnectionOptions
        {
            EncryptConnections = true
        };

        public bool EncryptConnections { get; set; }
    }
}
