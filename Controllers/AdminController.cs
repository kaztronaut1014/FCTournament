using FCTournament.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace FCTournament.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // 1. DASHBOARD QUẢN TRỊ
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.TotalTeams = await _context.Teams.CountAsync(t => !t.IsDeleted);
            ViewBag.TotalTournaments = await _context.Tournaments.CountAsync(t => !t.IsDeleted);
            ViewBag.PendingRequests = await _context.Organizers.CountAsync(o => o.PendingSubscriptionId != null);
            ViewBag.TotalBills = await _context.Bills.CountAsync();

            return View();
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = await _context.UserRoles.ToListAsync();
            var roles = await _context.Roles.ToListAsync();

            // Sửa Dictionary để chứa List<string> thay vì string
            var userRoleMap = new System.Collections.Generic.Dictionary<string, List<string>>();
            foreach (var u in users)
            {
                var roleIds = userRoles.Where(ur => ur.UserId == u.Id).Select(ur => ur.RoleId).ToList();
                var roleNames = roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.Name).ToList();

                if (!roleNames.Any()) roleNames.Add("User"); // Mặc định nếu không có quyền gì
                userRoleMap[u.Id] = roleNames;
            }

            ViewBag.UserRoleMap = userRoleMap;
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                // Lưu ý: Việc xóa User có thể dính khóa ngoại (Foreign Key)
                // Trong thực tế, người ta thường dùng Cờ "LockoutEnabled" để khóa tài khoản thay vì xóa cứng.
                await _userManager.SetLockoutEndDateAsync(user, System.DateTimeOffset.MaxValue);
                TempData["SuccessMessage"] = "Đã khóa tài khoản thành công!";
            }
            return RedirectToAction(nameof(Users));
        }

        // 3. QUẢN LÝ GIẢI ĐẤU (Xem tất cả và Xóa)
        public async Task<IActionResult> Tournaments()
        {
            var tournaments = await _context.Tournaments.Include(t => t.ApplicationUser).Where(t => !t.IsDeleted).ToListAsync();
            return View(tournaments);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTournament(int id)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament != null)
            {
                tournament.IsDeleted = true; // Soft Delete
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa giải đấu thành công!";
            }
            return RedirectToAction(nameof(Tournaments));
        }

        // 4. QUẢN LÝ ĐỘI BÓNG (Xem tất cả và Xóa)
        public async Task<IActionResult> Teams()
        {
            var teams = await _context.Teams.Include(t => t.ApplicationUser).Where(t => !t.IsDeleted).ToListAsync();
            return View(teams);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team != null)
            {
                team.IsDeleted = true; // Soft Delete
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa đội bóng thành công!";
            }
            return RedirectToAction(nameof(Teams));
        }

        // 5. QUẢN LÝ YÊU CẦU NÂNG CẤP GÓI (SUBSCRIPTION)
        public async Task<IActionResult> Subscriptions()
        {
            var requests = await _context.Organizers
                .Include(o => o.User)
                .Include(o => o.Subscriptions) // Gói hiện tại
                .Where(o => o.PendingSubscriptionId != null)
                .ToListAsync();

            // Lấy tên gói muốn nâng cấp truyền ra View
            ViewBag.AllSubs = await _context.Subscriptions.ToListAsync();

            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveSubscription(int organizerId)
        {
            var org = await _context.Organizers.FindAsync(organizerId);
            if (org != null && org.PendingSubscriptionId != null)
            {
                org.SubscriptionId = org.PendingSubscriptionId.Value; // Cập nhật gói mới
                org.PendingSubscriptionId = null; // Xóa trạng thái chờ
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã phê duyệt gói nâng cấp!";
            }
            return RedirectToAction(nameof(Subscriptions));
        }

        [HttpPost]
        public async Task<IActionResult> RejectSubscription(int organizerId)
        {
            var org = await _context.Organizers.FindAsync(organizerId);
            if (org != null)
            {
                org.PendingSubscriptionId = null; // Hủy yêu cầu
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã từ chối yêu cầu nâng cấp!";
            }
            return RedirectToAction(nameof(Subscriptions));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRoles(string userId, List<string> newRoles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                if (User.Identity?.Name == user.Email)
                {
                    TempData["ErrorMessage"] = "Bạn không thể tự thay đổi quyền của chính mình!";
                    return RedirectToAction(nameof(Users));
                }

                // Chống lỗi: Nếu Admin lỡ bỏ tick sạch sẽ, tự động gán lại làm User thường
                if (newRoles == null || !newRoles.Any())
                {
                    newRoles = new List<string> { "User" };
                }

                // 2.1 Xóa sạch các quyền cũ
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                // 2.2 Vòng lặp cấp quyền mới & Tạo Profile tương ứng
                foreach (var role in newRoles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                    await _userManager.AddToRoleAsync(user, role);

                    // Tự động tạo Profile để họ không bị lỗi khi vào các trang chức năng
                    if (role == "Organizer" && !await _context.Organizers.AnyAsync(o => o.ApplicationUserId == user.Id))
                        _context.Organizers.Add(new Organizer { ApplicationUserId = user.Id, SubscriptionId = 2 });
                    else if (role == "Player" && !await _context.Players.AnyAsync(p => p.ApplicationUserId == user.Id))
                        _context.Players.Add(new Player { ApplicationUserId = user.Id });
                    else if (role == "Referee" && !await _context.Referees.AnyAsync(r => r.ApplicationUserId == user.Id))
                        _context.Referees.Add(new Referee { ApplicationUserId = user.Id });
                    else if (role == "Manager" && !await _context.Managers.AnyAsync(m => m.ApplicationUserId == user.Id))
                        _context.Managers.Add(new Manager { ApplicationUserId = user.Id });
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật đa quyền ({string.Join(", ", newRoles)}) cho {user.Email}!";
            }
            return RedirectToAction(nameof(Users));
        }

        // TRANG QUẢN LÝ TOÀN BỘ HÓA ĐƠN CHO ADMIN
        public async Task<IActionResult> Bills()
        {
            var allBills = await _context.Bills
                .Include(b => b.Tournament)
                .Include(b => b.Team)
                .OrderByDescending(b => b.DateCreate)
                .ToListAsync();

            var paidAmounts = await _context.BillDetails
                .GroupBy(bd => bd.BillId)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(bd => bd.FeePaid));

            ViewBag.PaidAmounts = paidAmounts;

            return View(allBills);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveBill(int billId)
        {
            var bill = await _context.Bills
                .FirstOrDefaultAsync(b => b.Id == billId);
            if (bill != null)
            {
                var relatedDetails = await _context.BillDetails.Where(bd => bd.BillId == billId).ToListAsync();
                if (relatedDetails.Any())
                {
                    _context.BillDetails.RemoveRange(relatedDetails);
                }
                _context.Bills.Remove(bill);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa hóa đơn!";
            }
            return RedirectToAction(nameof(Bills));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBill(int billId)
        {
            var existBill = await _context.Bills
                .FirstOrDefaultAsync(b => b.Id == billId);
            if (existBill != null)
            {
                existBill.isPaid = true;
                existBill.DatePaid = DateTime.Now;
                _context.Bills.Update(existBill);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thanh toán hóa đơn!";
            }
            return RedirectToAction(nameof(Bills));
        }
    }
}