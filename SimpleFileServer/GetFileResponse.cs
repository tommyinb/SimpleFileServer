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

            var addType = new Action<string, FileType, string>((extension, fileType, mimeType) =>
                extensionTypes.Add(extension, Tuple.Create(fileType, mimeType)));

            addType(".html", FileType.Text, MediaTypeNames.Text.Html);
            addType(".txt", FileType.Text, MediaTypeNames.Text.Plain);
            addType(".xml", FileType.Text, MediaTypeNames.Text.Xml);
            addType(".css", FileType.Text, "text/css");
            addType(".js", FileType.Text, "text/javascript");
            addType(".json", FileType.Text, "application/json");

            addType(".exe", FileType.Bytes, MediaTypeNames.Application.Octet);
            addType(".zip", FileType.Bytes, MediaTypeNames.Application.Zip);
            addType(".7z", FileType.Bytes, "application/x-7z-compressed");
            addType(".rar", FileType.Bytes, "application/x-rar-compressed");
            addType(".pdf", FileType.Bytes, MediaTypeNames.Application.Pdf);

            addType(".png", FileType.Bytes, "image/png");
            addType(".jpeg", FileType.Bytes, MediaTypeNames.Image.Jpeg);
            addType(".jpg", FileType.Bytes, MediaTypeNames.Image.Jpeg);
            addType(".gif", FileType.Bytes, MediaTypeNames.Image.Gif);
            addType(".tiff", FileType.Bytes, MediaTypeNames.Image.Tiff);
            addType(".bmp", FileType.Bytes, "image/bmp");
            addType(".svg", FileType.Bytes, "image/svg+xml");
            addType(".ico", FileType.Bytes, "image/x-icon");

            addType(".eot", FileType.Bytes, "application/vnd.ms-fontobject");
            addType(".otf", FileType.Bytes, "application/font-sfnt");
            addType(".ttf", FileType.Bytes, "application/font-sfnt");
            addType(".woff", FileType.Bytes, "application/font-woff");
        }
        private string directory;

        private Dictionary<string, Tuple<FileType, string>> extensionTypes =
            new Dictionary<string, Tuple<FileType, string>>();
        private enum FileType { Text, Bytes };

        public bool IsValid(HttpListenerRequest request)
        {
            if (request.HttpMethod != "GET") return false;

            var relativeFilePath = request.Url.LocalPath.TrimStart('/').Replace("/", @"\");
            var fileExtension = Path.GetExtension(relativeFilePath);

            var localFilePath = Path.Combine(directory, relativeFilePath);

            return File.Exists(localFilePath);
        }
        public async Task Response(HttpListenerContext context)
        {
            var relativeFilePath = context.Request.Url.LocalPath.TrimStart('/').Replace("/", @"\");
            var localFilePath = Path.Combine(directory, relativeFilePath);

            await Response(context.Response, localFilePath);
        }
        public async Task Response(HttpListenerResponse response, string localFilePath)
        {
            var fileExtension = Path.GetExtension(localFilePath);

            if (extensionTypes.ContainsKey(fileExtension))
            {
                var extensionType = extensionTypes[fileExtension];

                switch (extensionType.Item1)
                {
                    case FileType.Text:
                        var text = File.ReadAllText(localFilePath);

                        response.ContentType = extensionType.Item2;
                        response.ContentEncoding = Encoding.UTF8;
                        response.AddHeader("Charset", Encoding.UTF8.WebName);

                        var textBytes = Encoding.UTF8.GetBytes(text);
                        response.ContentLength64 = textBytes.LongLength;
                        await response.OutputStream.WriteAsync(textBytes, 0, textBytes.Length);

                        break;

                    case FileType.Bytes:
                        var bytes = File.ReadAllBytes(localFilePath);

                        response.ContentType = extensionType.Item2;

                        response.ContentLength64 = bytes.LongLength;
                        await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);

                        break;
                }
            }
            else
            {
                var fileBytes = File.ReadAllBytes(localFilePath);

                response.ContentLength64 = fileBytes.LongLength;
                await response.OutputStream.WriteAsync(fileBytes, 0, fileBytes.Length);
            }
        }
    }
}
