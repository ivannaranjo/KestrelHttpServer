// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Server.Kestrel.Http
{
    class FrameResponseStream : Stream
    {
        private readonly FrameContext _context;
        private bool _stopped;
        private bool _aborted;

        public FrameResponseStream(FrameContext context)
        {
            _context = context;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Position { get; set; }

        public override void Flush()
        {
            if (_stopped)
            {
                throw new ObjectDisposedException(nameof(FrameResponseStream));
            }
            if (_aborted)
            {
                throw new IOException("The request has been aborted.");
            }

            _context.FrameControl.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            if (_stopped)
            {
                throw new ObjectDisposedException(nameof(FrameResponseStream));
            }
            if (_aborted)
            {
                throw new IOException("The request has been aborted.");
            }

            return _context.FrameControl.FlushAsync(cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_stopped)
            {
                throw new ObjectDisposedException(nameof(FrameResponseStream));
            }
            if (_aborted)
            {
                throw new IOException("The request has been aborted.");
            }

            _context.FrameControl.Write(new ArraySegment<byte>(buffer, offset, count));
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_stopped)
            {
                throw new ObjectDisposedException(nameof(FrameResponseStream));
            }
            if (_aborted)
            {
                throw new IOException("The request has been aborted.");
            }

            return _context.FrameControl.WriteAsync(new ArraySegment<byte>(buffer, offset, count), cancellationToken);
        }

        public void StopAcceptingWrites()
        {
            // Can't use dispose (or close) as can be disposed too early by user code
            // As exampled in EngineTests.ZeroContentLengthNotSetAutomaticallyForCertainStatusCodes
            _stopped = true;
        }

        public void Abort()
        {
            // We don't want to throw an ODE until the app func actually completes.
            // If the request is aborted, we throw an IOException instead.
            _aborted = true;
        }
    }
}
