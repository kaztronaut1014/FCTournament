using FCTournament.Models;
using FCTournament.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FCTournament.Controllers
{
    [Authorize] // Bắt buộc đăng nhập mới được Quản lý đội bóng
    public class TeamController : Controller
    {
        private readonly ITeamRepository _teamRepo;
        private readonly ILocationRepository _locationRepo;
        private readonly EFManagerRepository _managerRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPlayerRepository _playerRepo;

        public TeamController(
            ITeamRepository teamRepo,
            ILocationRepository locationRepo,
            EFManagerRepository managerRepo,
            IPlayerRepository playerRepo,
            IWebHostEnvironment webHostEnvironment,
            UserManager<ApplicationUser> userManager)
        {
            _teamRepo = teamRepo;
            _locationRepo = locationRepo;
            _managerRepo = managerRepo;
            _playerRepo = playerRepo;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        // 1. DANH SÁCH ĐỘI BÓNG CỦA TÔI
        // 1. DANH SÁCH ĐỘI BÓNG CỦA TÔI
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // 1. Lấy danh sách các đội do user này làm Ông bầu (Quản lý)
            var myManagedTeams = await _teamRepo.GetTeamsByUserIdAsync(userId);

            // 2. Lấy thông tin đội bóng mà user này đang đầu quân (Cầu thủ)
            var playerProfile = await _playerRepo.GetPlayerByUserIdAsync(userId);
            Team myPlayingTeam = null;

            if (playerProfile != null && playerProfile.TeamId.HasValue)
            {
                // Dùng hàm GetTeamByIdAsync để lấy luôn cả Location và Players cho đội bóng
                myPlayingTeam = await _teamRepo.GetTeamByIdAsync(playerProfile.TeamId.Value);
            }

            // Truyền đội bóng đang thi đấu sang View thông qua ViewBag
            ViewBag.MyPlayingTeam = myPlayingTeam;

            return View(myManagedTeams);
        }

        // 2. XEM CHI TIẾT ĐỘI BÓNG (Ai cũng xem được)
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var team = await _teamRepo.GetTeamByIdAsync(id);
            if (team == null || team.IsDeleted) return NotFound();

            return View(team);
        }

        // 3. TẠO ĐỘI BÓNG MỚI (GET)
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);

            // BẢO MẬT: Kiểm tra xem người dùng có hồ sơ Manager không?
            var managerProfile = await _managerRepo.GetManagerByUserIdAsync(userId);
            if (managerProfile == null)
            {
                var manager = new Manager { ApplicationUserId = userId };
                await _managerRepo.AddManagerAsync(manager);
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Team team, IFormFile? logoFile)
        {
            var userId = _userManager.GetUserId(User);
            var managerProfile = await _managerRepo.GetManagerByUserIdAsync(userId);

            if (managerProfile == null) return RedirectToAction(nameof(Index));

            // BẢO HỆ THỐNG BỎ QUA KIỂM TRA CÁC TRƯỜNG MÌNH TỰ GÁN
            ModelState.Remove("ManagerId");
            ModelState.Remove("ApplicationUserId");
            ModelState.Remove("Manager");
            ModelState.Remove("ApplicationUser");

            if (ModelState.IsValid)
            {
                // Tự động gán các thông tin chìm
                team.ApplicationUserId = userId;
                team.ManagerId = managerProfile.ApplicationUserId;
                team.EstablishDate = DateOnly.FromDateTime(DateTime.Now); // Lấy luôn ngày hôm nay làm ngày thành lập
                team.inTournament = 0;
                team.IsDeleted = false;

                if (logoFile != null)
                {
                    team.Logo = await UploadImage(logoFile, "teams/logos");
                }

                await _teamRepo.AddTeamAsync(team);
                TempData["SuccessMessage"] = "Thành lập đội bóng thành công!";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownData(team);
            return View(team);
        }

        // 4. CHỈNH SỬA ĐỘI BÓNG (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var team = await _teamRepo.GetTeamByIdAsync(id);
            if (team == null || team.IsDeleted) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (team.ApplicationUserId != userId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa đội bóng này!";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownData(team);
            return View(team);
        }

        // 4. CHỈNH SỬA ĐỘI BÓNG (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Team team, IFormFile? logoFile)
        {
            if (id != team.Id) return NotFound();

            var userId = _userManager.GetUserId(User);
            var existingTeam = await _teamRepo.GetTeamByIdAsync(id);

            if (existingTeam == null || existingTeam.ApplicationUserId != userId) return NotFound();

            // TƯƠNG TỰ: BỎ QUA KIỂM TRA LỖI KHI EDIT
            ModelState.Remove("ManagerId");
            ModelState.Remove("ApplicationUserId");
            ModelState.Remove("Manager");
            ModelState.Remove("ApplicationUser");

            if (ModelState.IsValid)
            {
                if (logoFile != null)
                {
                    existingTeam.Logo = await UploadImage(logoFile, "teams/logos");
                }

                existingTeam.FullName = team.FullName;
                existingTeam.ShortName = team.ShortName;
                existingTeam.LocationId = team.LocationId;
                // Không cho phép sửa ngày thành lập (EstablishDate) để đảm bảo tính lịch sử

                await _teamRepo.UpdateTeamAsync(existingTeam);
                TempData["SuccessMessage"] = "Cập nhật thông tin đội bóng thành công!";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownData(team);
            return View(team);
        }

        // 5. GIẢI TÁN ĐỘI BÓNG (Xóa mềm)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var team = await _teamRepo.GetTeamByIdAsync(id);

            if (team == null || team.ApplicationUserId != userId)
            {
                TempData["ErrorMessage"] = "Không thể giải tán đội bóng này!";
                return RedirectToAction(nameof(Index));
            }

            team.IsDeleted = true;
            await _teamRepo.UpdateTeamAsync(team);

            TempData["SuccessMessage"] = "Đã giải tán đội bóng!";
            return RedirectToAction(nameof(Index));
        }

        // HÀM HỖ TRỢ LẤY LIST DROPDOWN
        private async Task LoadDropdownData(Team? team = null)
        {
            var locations = await _locationRepo.GetAllLocationsAsync();
            ViewBag.LocationId = new SelectList(locations, "Id", "Name", team?.LocationId);
        }

        // HÀM HỖ TRỢ UPLOAD ẢNH
        private async Task<string> UploadImage(IFormFile file, string folder)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", folder);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/images/{folder}/{uniqueFileName}";
        }

        [HttpPost]
        public async Task<IActionResult> AddPlayer(int teamId, int playerId)
        {
            var team = await _teamRepo.GetTeamByIdAsync(teamId);
            var userId = _userManager.GetUserId(User);

            if (team == null || team.ApplicationUserId != userId) return NotFound();

            var player = await _playerRepo.GetPlayerByIdAsync(playerId);

            if (player == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy cầu thủ này.";
                return RedirectToAction("Edit", new { id = teamId });
            }

            if (player.TeamId != null)
            {
                TempData["ErrorMessage"] = $"Cầu thủ '{player.NickName}' đang thi đấu cho một đội bóng khác!";
                return RedirectToAction("Edit", new { id = teamId });
            }

            player.TeamId = teamId;
            await _playerRepo.UpdatePlayerAsync(player);

            TempData["SuccessMessage"] = $"Đã chiêu mộ thành công {player.NickName} vào đội!";
            return RedirectToAction("Edit", new { id = teamId });
        }

        [HttpGet]
        public async Task<IActionResult> SearchPlayers(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            term = term.ToLower();

            // Lấy danh sách (đã bao gồm dữ liệu bảng User và Team nhờ EFPlayerRepository)
            var allPlayers = await _playerRepo.GetPlayersAsync();

            var results = allPlayers.Where(p =>
                // Tìm trong NickName
                (p.NickName != null && p.NickName.ToLower().Contains(term)) ||
                // Hoặc tìm trong FullName của bảng User (Giả định bảng ApplicationUser của bạn có cột FullName)
                (p.User != null && p.User.FullName != null && p.User.FullName.ToLower().Contains(term))
            ).Select(p => {

                // Logic hiển thị Tên: Ưu tiên Nickname. Nếu Nickname trống thì dùng FullName. 
                // Nếu cả 2 đều trống thì để là "Người chơi ẩn danh"
                string displayName = p.NickName;
                if (string.IsNullOrWhiteSpace(displayName))
                {
                    displayName = p.User?.FullName;
                }
                if (string.IsNullOrWhiteSpace(displayName))
                {
                    displayName = "Người chơi ẩn danh";
                }

                return new
                {
                    id = p.Id,
                    nickName = displayName, // Trả tên chuẩn về cho Giao diện
                    teamName = p.Team?.FullName ?? "Tự do", // Đội hiện tại
                    isAvailable = p.TeamId == null // Đánh dấu xem có mời được không
                };
            }).Take(10).ToList();

            return Json(results);
        }

        // 7. MANAGER ĐUỔI CẦU THỦ KHỎI ĐỘI
        [HttpPost]
        public async Task<IActionResult> RemovePlayer(int teamId, int playerId)
        {
            var team = await _teamRepo.GetTeamByIdAsync(teamId);
            var userId = _userManager.GetUserId(User);

            if (team == null || team.ApplicationUserId != userId) return NotFound();

            var player = await _playerRepo.GetPlayerByIdAsync(playerId);
            if (player != null && player.TeamId == teamId)
            {
                player.TeamId = null; // Cắt đứt quan hệ
                await _playerRepo.UpdatePlayerAsync(player);
                TempData["SuccessMessage"] = $"Đã khai trừ {player.NickName} khỏi đội bóng.";
            }

            return RedirectToAction("Edit", new { id = teamId });
        }

        // 8. CẦU THỦ TỰ RỜI ĐỘI BÓNG
        [HttpPost]
        public async Task<IActionResult> LeaveTeam(int teamId)
        {
            var userId = _userManager.GetUserId(User);
            var player = await _playerRepo.GetPlayerByUserIdAsync(userId);

            if (player != null && player.TeamId == teamId)
            {
                player.TeamId = null; // Tự do
                await _playerRepo.UpdatePlayerAsync(player);
                TempData["SuccessMessage"] = "Bạn đã chính thức rời khỏi đội bóng.";
            }
            return RedirectToAction("Details", new { id = teamId });
        }

    }
}