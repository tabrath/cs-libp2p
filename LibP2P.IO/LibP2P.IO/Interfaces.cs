namespace LibP2P.IO
{
    public interface IReadWriter : IReader, IWriter { }
    public interface IReadCloser : IReader, ICloser { }
    public interface IWriteCloser : IWriter, ICloser { }
    public interface IReadWriteCloser : IReader, IWriter, ICloser { }
    public interface IReadSeeker : IReader, ISeeker { }
    public interface IWriteSeeker : IWriter, ISeeker { }
    public interface IReadWriteSeeker : IReader, IWriter, ISeeker { }
}
