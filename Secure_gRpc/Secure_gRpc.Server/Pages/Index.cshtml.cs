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
        private readonly DuplexService _duplexService;

        public IndexModel(DuplexService duplexService)
        {
            _duplexService = duplexService;
        }

        public void OnGet()
        {
            
        }
    }
}
