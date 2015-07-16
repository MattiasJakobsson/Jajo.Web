﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SuperGlue.Web.Http
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class GzipOutput
    {
        private readonly AppFunc _next;

        public GzipOutput(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var request = environment.GetRequest();

            string[] acceptedEncodings;

            request.Headers.RawHeaders.TryGetValue("Accept-Encoding", out acceptedEncodings);

            if (!(from encoding in (acceptedEncodings ?? new string[0]).SelectMany(x => x.Split(',')).Select(x => x.Trim())
                  where String.Equals(encoding, "gzip", StringComparison.Ordinal)
                  select encoding).Any())
            {
                await _next(environment);
                return;
            }

            var response = environment.GetResponse();

            var body = response.Body;
            response.Body = new BufferingStream(body, environment);

            try
            {
                await _next(environment);

                if (response.Body is BufferingStream)
                    await response.Body.FlushAsync();
            }
            finally
            {
                response.Body = body;
            }
        }

        private sealed class BufferingStream : MemoryStream
        {
            private Stream _stream;
            private readonly IDictionary<string, object> _environment;

            internal BufferingStream(Stream stream, IDictionary<string, object> environment)
            {
                _stream = stream;
                _environment = environment;
            }

            public override async Task FlushAsync(CancellationToken cancellationToken)
            {
                if (!(_stream is GZipStream))
                {
                    Seek(0, SeekOrigin.Begin);
                    await CopyToAsync(_stream, 8192, cancellationToken);
                    SetLength(0);

                    return;
                }

                _stream.Dispose();
            }

            public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                var request = _environment.GetRequest();

                if (_stream is GZipStream)
                {
                    await _stream.WriteAsync(buffer, offset, count, request.CallCancelled);
                    return;
                }

                if ((count + Length) < 4096)
                {
                    await base.WriteAsync(buffer, offset, count, cancellationToken);
                    return;
                }

                if (!string.Equals(request.Protocol, "HTTP/1.1", StringComparison.Ordinal))
                    throw new InvalidOperationException("The Transfer-Encoding: chunked mode can only be used with HTTP/1.1");

                var response = _environment.GetResponse();

                response.Headers.SetHeader("Content-Encoding", "gzip");
                response.Headers.SetHeader("Transfer-Encoding", "chunked");

                await base.WriteAsync(buffer, offset, count, cancellationToken);

                _stream = new GZipStream(_stream, CompressionMode.Compress, true);

                Seek(0, SeekOrigin.Begin);
                await CopyToAsync(_stream, 8192, request.CallCancelled);
            }
        }
    }
}