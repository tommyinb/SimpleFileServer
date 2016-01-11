using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFileServer
{
    public static class HttpListenerUtil
    {
        public static void WriteResult(this HttpListenerResponse response, HttpStatusCode statusCode, string text)
        {
            response.StatusCode = (int)statusCode;
            WriteText(response, text);
        }

        public static void WriteText(this HttpListenerResponse response, string text)
        {
            WriteText(response, text, Encoding.UTF8);
        }
        public static void WriteText(this HttpListenerResponse response, string text, Encoding encoding)
        {
            response.AddHeader("Charset", Encoding.UTF8.WebName);

            response.ContentEncoding = encoding;

            var bytes = encoding.GetBytes(text);

            response.ContentLength64 = bytes.LongLength;

            response.OutputStream.Write(bytes, 0, bytes.Length);
        }

        public static async Task WriteTextAsync(this HttpListenerResponse response, string text)
        {
            await WriteTextAsync(response, text, Encoding.UTF8);
        }
        public static async Task WriteTextAsync(this HttpListenerResponse response, string text, Encoding encoding)
        {
            response.AddHeader("Charset", Encoding.UTF8.WebName);

            response.ContentEncoding = encoding;

            var bytes = encoding.GetBytes(text);

            response.ContentLength64 = bytes.LongLength;

            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}
