# LibP2P.Utilities (cs-libp2p-utils)

[![](https://img.shields.io/badge/project-libp2p-blue.svg?style=flat-square)](https://github.com/libp2p/libp2p)
[![](https://img.shields.io/badge/freenode-%23ipfs-blue.svg?style=flat-square)](https://webchat.freenode.net/?channels=%23ipfs)
[![Travis CI](https://img.shields.io/travis/libp2p/cs-libp2p-utils.svg?style=flat-square&branch=master)](https://travis-ci.org/libp2p/cs-libp2p-utils)
[![AppVeyor](https://img.shields.io/appveyor/ci/tabrath/cs-libp2p-utils/master.svg?style=flat-square)](https://ci.appveyor.com/project/tabrath/cs-libp2p-utils)
[![NuGet](https://buildstats.info/nuget/LibP2P.Utilities)](https://www.nuget.org/packages/LibP2P.Utilities/)
[![](https://img.shields.io/badge/readme%20style-standard-brightgreen.svg?style=flat-square)](https://github.com/RichardLitt/standard-readme)
[![Codecov](https://img.shields.io/codecov/c/github/libp2p/cs-libp2p-utils/master.svg?style=flat-square)](https://codecov.io/gh/libp2p-utils/cs-libp2p-utils)
[![Libraries.io](https://img.shields.io/librariesio/github/libp2p/cs-libp2p-utils.svg?style=flat-square)](https://libraries.io/github/libp2p/cs-libp2p-utils)

>Various utilities used in C# LibP2P.

## Table of Contents

- [Install](#install)
- [Usage](#usage)
- [Maintainers](#maintainers)
- [Contribute](#contribute)
- [License](#license)

## Install

    PM> Install-Package LibP2P.Utilities

## Usage

### Extensions

#### Array

- `T[] Slice<T>(this T[] array, int offset, int? count = null)`

  Returns a slice of an array, from offset and count (or the remaining lengt).

- `T[] Append<T>(this T[] array, params T[] items)`
  
  Returns a new array containing the given array and all the items in sequential order.

- `int Copy<T>(this T[] src, T[] dst, int offset, int? count = null)`

  Copies `count` items from `src` at given `offset` and returns the actual count of items copied.


#### Byte Array

- `byte[] Append(this byte[] bytes, params byte[][] arrays)`

  Returns a new byte array containing the given array and all the arrays in sequential order.

- `int Compare(this byte[] a, byte[] b)`

  Compare `a` to `b` by length and content.

- `byte[] XOR(this byte[] a, byte[] b)`

  Returns a new byte array of the xor'ed result of `a` and `b`.

- `byte[] ComputeHash(this byte[] bytes)`

  Returns the hash digest of the given bytes using the default algorithm (`SHA2_256`).

#### Multiaddress

- `bool IsIPLoopback(this Multiaddress addr)`
  
  Is the given Multiaddress pointing to a loopback address? Catches both IPv4 and IPv6.

- `bool IsFDCostlyTransport(this Multiaddress addr)`

  Does the given Multiaddress contain a transport that require file descriptors, like sockets?
  Catches only TCP for now.

#### Multihash

- `int Compare(this Multihash a, Multihash b)`

  Compares two Multihashes by length and content.

#### Protocol Buffers

- `byte[] SerializeToBytes<T>(this T obj)`

  Serializes the given object to a byte array using Protocol Buffers.
  The given object must be a valid protobuf-net contract.

- `T Deserialize<T>(this byte[] bytes)`

  Deserializes the given byte array to an instance of `T`.
  The given type must be a valid protobuf-net contract type.

#### ReaderWriterLockSlim

- `void Read(this ReaderWriterLockSlim rwls, Action action, int timeout = Timeout.Infinite)`

  Aqcuire a read lock with optional timeout.

- `T Read<T>(this ReaderWriterLockSlim rwls, Func<T> func, int timeout = TImeout.Infinite)`

  Acquire a read lock and return a value with optional timeout.

- `void Write(this ReaderWriterLockSlim rwls, Action action, int timeout = Timeout.Infinite)`

  Acquire a write lock with optional timeout.

- `T Write<T>(this ReaderWriterLockSlim rwls, Func<T> func, int timeout = TImeout.Infinite)`

  Acquire a write lock and return a value with optional timeout.

#### SemaphoreSlim

- `void Lock(this SemaphoreSlim sl, Action action)`
  
  Acquire a lock and perform an action.

- `Task LockAsync(this SemaphoreSlim sl, Action action, CancellationToken cancellationToken`

  Acquire a lock and perform an action asynchronously.
 
- `T Lock<T>(this SemaphoreSim sl, Func<T> func)`

  Acquire a lock and return a value.
  
- `Task<T> LockAsync<T>(this SemaphoreSlim sl, Func<T> func, CancellationToken cancellationToken)`

  Acquire a lock and return a value asynchronously.

#### Stream

- `IReader AsReader(this Stream stream)`

  Get an `IReader` from a `Stream`.

- `IWriter AsWriter(this Stream stream)`

  Get an `IWriter` from a `Stream`.

- `Stream AsSystemStream(this IReader)`
- `Stream AsSystemStream(this IWriter)`
- `Stream AsSystemStream(this ISeeker)`
- `Stream AsSystemStream(this IReadWriter)`
- `Stream AsSystemStream(this IReadWriteSeeker)`
- `Stream AsSystemStream(this IReadWriteCloser)`

  Get a `Stream` from a LibP2P interface reader/writer/seeker/etc.

- `int CopyTo(this IReader reader, IWriter writer, int bufferSize = 4096)`

  Copy everything from reader to writer with a given buffer size. Returns bytes copied.

- `Task<int> CopyToAsync(this IReader reader, IWriter writer, int bufferSize = 4096, CancellationToken cancellationToken = default(CancellationToken))`

  Copy everything from reader to writer with a given buffer size asynchronously. Returns bytes copied.

- `int ReadFull(this IReader reader, byte[] buffer, int offset = 0, int count = -1)`

  Read from reader until given count is reached or the size of the buffer minus offset.

- `Task<int> ReadFullAsync(this IReader reader, byte[] buffer, int offset = 0, int count = -1, CancellationToken cancellationToken = default(CancellationToken))`

  Read from reader until given count is reached or the size of the buffer minus offset asynchronously.

#### Tuple

- `Tuple<T, T> Swap<T>(this Tuple<T, T> tuple)`

  Swaps the values of a tuple.

### Classes

#### ConcurrentList

- `ConcurrentList<T> : IList<T>, IDisposable`

  `IList` with built in `ReaderWriterLockSlim`.

#### SyncMutex

- `abstract class SyncMutex : IDisposable`

  Base class for usage with one `ReaderWriterLockSlim`.

## Maintainers

Captain: [@tabrath](https://github.com/tabrath).

## Contribute

Contributions welcome. Please check out [the issues](https://github.com/multiformats/cs-multihash/issues).

Check out our [contributing document](https://github.com/multiformats/multiformats/blob/master/contributing.md) for more information on how we work, and about contributing in general. Please be aware that all interactions related to multiformats are subject to the IPFS [Code of Conduct](https://github.com/ipfs/community/blob/master/code-of-conduct.md).

Small note: If editing the README, please conform to the [standard-readme](https://github.com/RichardLitt/standard-readme) specification.

## License

[MIT](LICENSE) © 2016 Trond Bråthen
