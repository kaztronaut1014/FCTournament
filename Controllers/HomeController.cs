using FCTournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FCTournament.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Pricing([FromServices] ApplicationDbContext context)
        {
            var subs = await context.Subscriptions.ToListAsync();
            return View(subs);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RequestUpgrade(int subId, [FromServices] ApplicationDbContext context, [FromServices] UserManager<ApplicationUser> userManager)
        {
            var user = await userManager.GetUserAsync(User);
            var org = await context.Organizers.FirstOrDefaultAsync(o => o.ApplicationUserId == user.Id);

            if (org != null)
            {
                if (org.SubscriptionId == subId)
                {
                    TempData["ErrorMessage"] = "Bạn đang sử dụng gói này rồi!";
                    return RedirectToAction(nameof(Pricing));
                }

                org.PendingSubscriptionId = subId;
                await context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã gửi yêu cầu nâng cấp! Vui lòng chờ Admin phê duyệt.";
            }
            else
            {
                TempData["ErrorMessage"] = "Lỗi: Bạn chưa có hồ sơ Ban Tổ Chức.";
            }
            return RedirectToAction(nameof(Pricing));
        }
    }
}
