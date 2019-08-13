using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SecureGrpc.ManagedClient.Models;

namespace SecureGrpc.ManagedClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly GrpcApiService _grpcApiService;

        public HomeController(GrpcApiService grpcApiService)
        {
            _grpcApiService = grpcApiService;
        }

        public async Task<IActionResult> Index()
        {
            await _grpcApiService.GetGrpcApiDataAsync();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
