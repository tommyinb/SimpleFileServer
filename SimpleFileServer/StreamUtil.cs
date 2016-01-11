using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFileServer
{
    public static class StreamUtil
    {
        public static async Task<byte[]> ReadToEndAsync(this Stream stream)
        {
            var buffer = new byte[1024 * 1024];

            using (var memoryStream = new MemoryStream())
            {
                while (true)
                {
                    var read = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (read <= 0) break;

                    await memoryStream.WriteAsync(buffer, 0, read);
                }

                return memoryStream.ToArray();
            }
        }
    }
}
