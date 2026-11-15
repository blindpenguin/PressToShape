using System;
using System.IO;
using System.Threading.Tasks;
using PressToShape.App.Services;

namespace PressToShape.App.Tests;

/// <summary>
/// A picked file kept in memory. Like the Android one, opening it for writing empties it at once. It can be told to fail after
/// a number of written bytes, to simulate a full disk, and to fail when it is opened.
/// </summary>
internal sealed class FakePickedFile : IPickedFile
{
    public FakePickedFile(string name, byte[]? content = null)
    {
        Name = name;
        Content = content ?? [];
    }

    public byte[] Content { get; set; }

    /// <summary>Writing fails after this many bytes of one write stream. <c>null</c> means it never fails.</summary>
    public int? FailWritingAfterBytes { get; set; }

    public Exception? OpenReadFailure { get; set; }

    public int WriteOpenCount { get; private set; }

    public string Name { get; }

    public Task<Stream> OpenReadAsync()
    {
        if (OpenReadFailure is not null)
        {
            return Task.FromException<Stream>(OpenReadFailure);
        }

        return Task.FromResult<Stream>(new MemoryStream(Content, writable: false));
    }

    public Task<Stream> OpenWriteAsync()
    {
        WriteOpenCount++;
        Content = [];
        return Task.FromResult<Stream>(new WriteStream(this));
    }

    private sealed class WriteStream : Stream
    {
        private readonly FakePickedFile _owner;
        private int _written;

        public WriteStream(FakePickedFile owner) => _owner = owner;

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_owner.FailWritingAfterBytes is { } limit && _written + count > limit)
            {
                var allowed = Math.Max(0, limit - _written);
                Append(buffer, offset, allowed);
                _written += allowed;
                throw new IOException("The disk is full.");
            }

            Append(buffer, offset, count);
            _written += count;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        private void Append(byte[] buffer, int offset, int count)
        {
            var combined = new byte[_owner.Content.Length + count];
            _owner.Content.CopyTo(combined, 0);
            Array.Copy(buffer, offset, combined, _owner.Content.Length, count);
            _owner.Content = combined;
        }
    }
}
