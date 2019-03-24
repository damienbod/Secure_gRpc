using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Secure_gRpc.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ServerGrpcSubscribers _serverGrpcSubscribers;

        public IndexModel(ServerGrpcSubscribers serverGrpcSubscribers)
        {
            _serverGrpcSubscribers = serverGrpcSubscribers;
        }

        public async Task OnGetAsync()
        {
            await _serverGrpcSubscribers.BroadcastMessageAsync(
                new Duplex.MyMessage { Message = "Hi from server", Name ="Server" });
        }
    }
}
