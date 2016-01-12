using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleFileServer
{
    public class MultipartFormData
    {
        public Dictionary<string, string> Headers { get; private set; }
        public byte[] Data { get; private set; }

        public static async Task<MultipartFormData[]> ParseAsync(HttpListenerRequest request)
        {
            var contentTypeParts = request.ContentType.Split(new[] { "; ", ";" }, StringSplitOptions.None);

            var boundaryMatch = contentTypeParts
                .Select(t => Regex.Match(t, "^boundary=(?<boundary>.+)$", RegexOptions.IgnoreCase))
                .First(t => t.Success);
            var boundaryText = boundaryMatch.Groups["boundary"].Value;

            var contentEncoding = request.ContentEncoding ?? Encoding.UTF8;
            var boundayBytes = contentEncoding.GetBytes(boundaryText);

            var contentBytes = await request.InputStream.ReadToEndAsync();
            var partsBytes = Split(contentBytes, boundayBytes);

            return partsBytes.Select(partBytes =>
            {
                var headerEndBytes = contentEncoding.GetBytes("\r\n\r\n");
                var headerEndIndex = GetSplitIndexes(partBytes, headerEndBytes).FirstOrDefault();

                if (headerEndIndex <= 0) return null;

                var multipartFormData = new MultipartFormData { Headers = new Dictionary<string, string>() };

                var headerContentBytes = new byte[headerEndIndex];
                Array.Copy(partBytes, headerContentBytes, headerEndIndex);

                using (var headerStream = new MemoryStream(headerContentBytes))
                using (var headerReader = new StreamReader(headerStream, contentEncoding))
                {
                    while (true)
                    {
                        var headerLine = headerReader.ReadLine();
                        if (string.IsNullOrEmpty(headerLine)) break;

                        var headerMatch = Regex.Match(headerLine, "^(?<key>[^:]+):(?<value>.+)$");
                        if (headerMatch.Success)
                        {
                            var headerKey = headerMatch.Groups["key"].Value;
                            var headerValue = headerMatch.Groups["value"].Value;
                            multipartFormData.Headers.Add(headerKey, headerValue);
                        }
                    }
                }

                var dataStartIndex = headerEndIndex + headerEndBytes.Length;
                var dataLength = partBytes.Length - dataStartIndex;

                multipartFormData.Data = new byte[dataLength];
                Array.Copy(partBytes, dataStartIndex, multipartFormData.Data, 0, dataLength);

                return multipartFormData;
            })
            .Where(t => t != null).ToArray();
        }
        private static IEnumerable<byte[]> Split(byte[] allBytes, byte[] boundary)
        {
            var splitIndexes = GetSplitIndexes(allBytes, boundary).ToArray();

            if (splitIndexes.Any() == false)
            {
                yield return allBytes;
                yield break;
            }

            var startIndexes = new[] { 0 }.Concat(splitIndexes.Select(t => t + boundary.Length + "\r\n".Length)).ToArray();
            var endIndexes = splitIndexes.Select(t => t - "\r\n--".Length).Concat(new[] { allBytes.Length }).ToArray();

            for (int i = 0; i < startIndexes.Length; i++)
            {
                var boundIndex = new Func<int, int>(t => Math.Min(Math.Max(0, t), allBytes.Length));
                var startIndex = boundIndex(startIndexes[i]);
                var endIndex = boundIndex(endIndexes[i]);
                var partLength = endIndex - startIndex;
                
                var partBytes = new byte[partLength];
                Array.Copy(allBytes, startIndex, partBytes, 0, partLength);
                yield return partBytes;
            }
        }
        private static IEnumerable<int> GetSplitIndexes(byte[] allBytes, byte[] splitter)
        {
            var checkLength = allBytes.Length - splitter.Length;
            for (int i = 0; i < checkLength; i++)
            {
                var matched = true;

                for (int j = 0; j < splitter.Length; j++)
                {
                    if (allBytes[i + j] != splitter[j])
                    {
                        matched = false;
                        break;
                    }
                }

                if (matched)
                {
                    yield return i;

                    i += splitter.Length;
                }
            }
        }
    }
}
