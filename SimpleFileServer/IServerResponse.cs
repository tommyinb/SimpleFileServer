using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFileServer
{
    public interface IServerResponse
    {
        bool IsValid(HttpListenerRequest request);

        Task Response(HttpListenerContext context);
    }
}
