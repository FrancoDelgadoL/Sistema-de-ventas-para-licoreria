using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Ezel_Market.Data;
using Ezel_Market.Models;
using Microsoft.AspNetCore.Identity;

namespace Ezel_Market.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        private readonly UserManager<Usuarios> _userManager;



        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger, UserManager<Usuarios> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}