using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFileServer
{
    public class GetFileResponse : IServerResponse
    {
        public GetFileResponse(string directory)
        {
            this.directory = directory;
        }
        private string directory;

        public static async Task Response(HttpListenerResponse response, string localFilePath)
        {
            var fileExtension = Path.GetExtension(localFilePath);

            var responseText = new Func<string, Task>(contentType => ResponseText(response, localFilePath, contentType));
            var responseBytes = new Func<string, Task>(contentType => ResponseBytes(response, localFilePath, contentType));

            switch (fileExtension)
            {
                case ".html": await responseText(MediaTypeNames.Text.Html); break;
                case ".txt": await responseText(MediaTypeNames.Text.Plain); break;
                case ".xml": await responseText(MediaTypeNames.Text.Xml); break;
                case ".css": await responseText("text/css"); break;
                case ".js": await responseText("text/javascript"); break;
                case ".json": await responseText("application/json"); break;

                case ".exe": await responseBytes(MediaTypeNames.Application.Octet); break;
                case ".zip": await responseBytes(MediaTypeNames.Application.Zip); break;
                case ".7z": await responseBytes("application/x-7z-compressed"); break;
                case ".rar": await responseBytes("application/x-rar-compressed"); break;
                case ".pdf": await responseBytes(MediaTypeNames.Application.Pdf); break;

                case ".png": await responseBytes("image/png"); break;
                case ".jpeg": await responseBytes(MediaTypeNames.Image.Jpeg); break;
                case ".jpg": await responseBytes(MediaTypeNames.Image.Jpeg); break;
                case ".gif": await responseBytes(MediaTypeNames.Image.Gif); break;
                case ".tiff": await responseBytes(MediaTypeNames.Image.Tiff); break;
                case ".bmp": await responseBytes("image/bmp"); break;
                case ".svg": await responseBytes("image/svg+xml"); break;
                case ".ico": await responseBytes("image/x-icon"); break;

                case ".eot": await responseBytes("application/vnd.ms-fontobject"); break;
                case ".otf": await responseBytes("application/font-sfnt"); break;
                case ".ttf": await responseBytes("application/font-sfnt"); break;
                case ".woff": await responseBytes("application/font-woff"); break;

                default: await ResponseRaw(response, localFilePath); break;
            }
        }
        public static async Task ResponseText(HttpListenerResponse response, string localFilePath, string contentType)
        {
            var text = File.ReadAllText(localFilePath);

            response.ContentType = contentType;
            response.ContentEncoding = Encoding.UTF8;
            response.AddHeader("Charset", Encoding.UTF8.WebName);

            var textBytes = Encoding.UTF8.GetBytes(text);
            response.ContentLength64 = textBytes.LongLength;
            await response.OutputStream.WriteAsync(textBytes, 0, textBytes.Length);
        }
        public static async Task ResponseBytes(HttpListenerResponse response, string localFilePath, string contentType)
        {
            var bytes = File.ReadAllBytes(localFilePath);

            response.ContentType = contentType;

            response.ContentLength64 = bytes.LongLength;
            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        }
        public static async Task ResponseRaw(HttpListenerResponse response, string localFilePath)
        {
            var bytes = File.ReadAllBytes(localFilePath);

            response.ContentLength64 = bytes.LongLength;
            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        }

        public bool IsValid(HttpListenerRequest request)
        {
            if (request.HttpMethod != "GET") return false;

            var localFilePath = request.MapFilePath(directory);
            return File.Exists(localFilePath);
        }
        public async Task Response(HttpListenerContext context)
        {
            var localFilePath = context.Request.MapFilePath(directory);
            await Response(context.Response, localFilePath);
        }
    }
}
