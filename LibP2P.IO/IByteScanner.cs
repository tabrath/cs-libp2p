namespace LibP2P.IO
{
    public interface IByteScanner : IByteReader
    {
        void UnreadByte();
    }
}