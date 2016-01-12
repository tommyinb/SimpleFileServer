using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFileServer
{
    public static class HttpListenerUtil
    {
        public static async Task WriteTextAsync(this HttpListenerResponse response, string text)
        {
            response.AddHeader("Charset", Encoding.UTF8.WebName);

            response.ContentEncoding = Encoding.UTF8;

            var bytes = Encoding.UTF8.GetBytes(text);

            response.ContentLength64 = bytes.LongLength;

            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        }

        public static void WriteResult(this HttpListenerResponse response, HttpStatusCode statusCode, string text)
        {
            response.StatusCode = (int)statusCode;

            response.AddHeader("Charset", Encoding.UTF8.WebName);

            response.ContentEncoding = Encoding.UTF8;

            var bytes = Encoding.UTF8.GetBytes(text);

            response.ContentLength64 = bytes.LongLength;

            response.OutputStream.Write(bytes, 0, bytes.Length);
        }
        public static async Task WriteResultAsync(this HttpListenerResponse response, HttpStatusCode statusCode, string text)
        {
            response.StatusCode = (int)statusCode;

            await WriteTextAsync(response, text);
        }

        public static string MapFilePath(this HttpListenerRequest request, string rootDirectory)
        {
            var relativeFilePath = request.Url.LocalPath.TrimStart('/').Replace("/", @"\");

            return Path.Combine(rootDirectory, relativeFilePath);
        }
    }
}
