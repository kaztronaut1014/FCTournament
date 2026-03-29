using FCTournament.Models;
using FCTournament.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FCTournament.Controllers
{
    [Authorize]
    public class TournamentController : Controller
    {
        private readonly ITournamentRepository _tournamentRepo;
        private readonly ILocationRepository _locationRepo;
        private readonly IFormatRepository _formatRepo;
        private readonly ITypeRepository _typeRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ITeamRepository _teamRepo;
        private readonly ITournamentTeamRepository _tournamentTeamRepo;
        private readonly IMatchRepository _matchRepo;
        private readonly EFOrganizerRepository _organizerRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBillRepository _billRepo;
        private readonly IBillDetailsRepository _billDetailsRepo;

        public TournamentController(
            ITournamentRepository tournamentRepo,
            ILocationRepository locationRepo,
            IFormatRepository formatRepo,
            ITypeRepository typeRepo,
            ITeamRepository teamRepo,
            IMatchRepository matchRepo,
            IWebHostEnvironment webHostEnvironment,
            ITournamentTeamRepository tournamentTeamRepo,
            UserManager<ApplicationUser> userManager,
            EFOrganizerRepository organizerRepo,
            IBillRepository billRepo,
            IBillDetailsRepository billDetailsRepo)
        {
            _tournamentRepo = tournamentRepo;
            _locationRepo = locationRepo;
            _formatRepo = formatRepo;
            _typeRepo = typeRepo;
            _matchRepo = matchRepo;
            _webHostEnvironment = webHostEnvironment;
            _tournamentTeamRepo = tournamentTeamRepo;
            _userManager = userManager;
            _organizerRepo = organizerRepo;
            _teamRepo = teamRepo;
            _billRepo = billRepo;
            _billDetailsRepo = billDetailsRepo;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var tournaments = await _tournamentRepo.GetAllTournamentsAsync();
            return View(tournaments);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var tournament = await _tournamentRepo.GetTournamentByIdAsync(id);
            if (tournament == null || tournament.IsDeleted) return NotFound();
            return View(tournament);
        }

        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);
            int maxAllowed = 1;

            var org = await _organizerRepo.GetOrganizerByUserIdAsync(userId);
            if (org != null)
            {
                if (org.SubscriptionId == 3) maxAllowed = 2;
                else if (org.SubscriptionId == 4) maxAllowed = 4;
            }

            var allTournaments = await _tournamentRepo.GetAllTournamentsAsync();
            int currentCount = allTournaments.Count(t => t.ApplicationUserId == userId && !t.IsDeleted && !t.IsFinished);

            if (User.IsInRole("Admin"))
            {
                maxAllowed = 9999;
            }

            if (currentCount >= maxAllowed)
            {
                TempData["ErrorMessage"] = $"Gói đăng ký hiện tại của bạn chỉ cho phép tạo tối đa {maxAllowed} giải đấu đang hoạt động cùng lúc. Vui lòng nâng cấp gói hoặc kết thúc giải cũ để tạo thêm!";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tournament tournament, IFormFile? logoFile, IFormFile? bannerFile)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            if (tournament.StartDate <= today)
            {
                ModelState.AddModelError("StartDate", "Ngày khai mạc phải sau ngày hiện tại ít nhất 1 ngày.");
            }

            var userId = _userManager.GetUserId(User);
            int maxAllowed = 1;

            var org = await _organizerRepo.GetOrganizerByUserIdAsync(userId);
            if (org != null)
            {
                if (org.SubscriptionId == 3) maxAllowed = 2;
                else if (org.SubscriptionId == 4) maxAllowed = 4;
            }

            var allTournaments = await _tournamentRepo.GetAllTournamentsAsync();
            int currentCount = allTournaments.Count(t => t.ApplicationUserId == userId && !t.IsDeleted && !t.IsFinished);

            if (User.IsInRole("Admin"))
            {
                maxAllowed = 9999;
            }

            if (currentCount >= maxAllowed)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi: Bạn đã đạt giới hạn {maxAllowed} giải đấu đang hoạt động.");
                await LoadDropdownData(tournament);
                return View(tournament);
            }

            if (ModelState.IsValid)
            {
                tournament.ApplicationUserId = userId;

                if (logoFile != null) tournament.LogoUrl = await UploadImage(logoFile, "tournaments/logos");
                if (bannerFile != null) tournament.BannerUrl = await UploadImage(bannerFile, "tournaments/banners");

                tournament.IsDeleted = false;
                await _tournamentRepo.AddTournamentAsync(tournament);
                TempData["SuccessMessage"] = "Tạo giải đấu thành công!";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownData(tournament);
            return View(tournament);
        }

        private async Task LoadDropdownData(Tournament? tournament = null)
        {
            var locations = await _locationRepo.GetAllLocationsAsync();
            var formats = await _formatRepo.GetAllFormatsAsync();
            var types = await _typeRepo.GetAllTypeAsync();

            ViewBag.LocationId = new SelectList(locations, "Id", "Name", tournament?.LocationId);
            ViewBag.FormatId = new SelectList(formats, "Id", "Name", tournament?.FormatId);
            ViewBag.TypeId = new SelectList(types, "Id", "Name", tournament?.TypeId);
        }

        private async Task<string> UploadImage(IFormFile file, string folder)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", folder);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create)) { await file.CopyToAsync(fileStream); }
            return $"/images/{folder}/{uniqueFileName}";
        }

        public async Task<IActionResult> Edit(int id)
        {
            var tournament = await _tournamentRepo.GetTournamentByIdAsync(id);
            if (tournament == null || tournament.IsDeleted) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (tournament.ApplicationUserId != userId)
            {
                TempData["ErrorMessage"] = "Truy cập bị từ chối! Bạn không phải là chủ sở hữu của giải đấu này.";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownData(tournament);
            return View(tournament);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tournament tournament, IFormFile? logoFile, IFormFile? bannerFile)
        {
            if (id != tournament.Id) return NotFound();

            var userId = _userManager.GetUserId(User);
            var existingTournament = await _tournamentRepo.GetTournamentByIdAsync(id);

            if (existingTournament == null || existingTournament.ApplicationUserId != userId) return NotFound();

            // FIX: Xóa các Navigation Properties khỏi ModelState để tránh lỗi "False Valid"
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("Location");
            ModelState.Remove("Format");
            ModelState.Remove("Type");
            ModelState.Remove("TournamentTeams");
            ModelState.Remove("Matches");

            if (ModelState.IsValid)
            {
                if (logoFile != null) existingTournament.LogoUrl = await UploadImage(logoFile, "tournaments/logos");
                if (bannerFile != null) existingTournament.BannerUrl = await UploadImage(bannerFile, "tournaments/banners");

                existingTournament.Name = tournament.Name;
                existingTournament.StartDate = tournament.StartDate;
                existingTournament.DurationDays = tournament.DurationDays;
                existingTournament.TypeId = tournament.TypeId;
                existingTournament.Description = tournament.Description;
                existingTournament.LocationId = tournament.LocationId;
                existingTournament.Organizer = tournament.Organizer;
                existingTournament.Rules = tournament.Rules;
                existingTournament.Prize = tournament.Prize;
                existingTournament.Fees = tournament.Fees;
                existingTournament.Size = tournament.Size;
                existingTournament.FormatId = tournament.FormatId;
                existingTournament.needApproved = tournament.needApproved;

                await _tournamentRepo.UpdateTournamentAsync(existingTournament);
                TempData["SuccessMessage"] = "Đã cập nhật thông tin giải đấu thành công!";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownData(tournament);
            return View(tournament);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var tournament = await _tournamentRepo.GetTournamentByIdAsync(id);

            if (tournament == null || tournament.ApplicationUserId != userId)
            {
                TempData["ErrorMessage"] = "Truy cập bị từ chối! Bạn không có quyền xóa giải đấu này.";
                return RedirectToAction(nameof(Index));
            }

            tournament.IsDeleted = true;
            await _tournamentRepo.UpdateTournamentAsync(tournament);

            TempData["SuccessMessage"] = "Đã xóa giải đấu thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RegisterTeam(int tournamentId, int teamId)
        {
            var userId = _userManager.GetUserId(User);

            var tournament = await _tournamentRepo.GetTournamentByIdAsync(tournamentId);
            if (tournament != null && (tournament.IsStarted || tournament.IsFinished))
            {
                TempData["ErrorMessage"] = "❌ Giải đấu đã khởi tranh hoặc kết thúc, không thể ghi danh thêm!";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            var team = await _teamRepo.GetTeamByIdAsync(teamId);
            if (team == null || team.ApplicationUserId != userId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền đăng ký cho đội bóng này!";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            bool isAlreadyRegistered = await _teamRepo.IsTeamRegisteredInTournamentAsync(teamId, tournamentId);
            if (isAlreadyRegistered)
            {
                TempData["ErrorMessage"] = $"Đội bóng '{team.FullName}' đã có tên trong giải đấu này rồi!";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            if (tournament.TournamentTeams.Count(t => !tournament.needApproved || t.IsApproved) >= tournament.Size)
            {
                TempData["ErrorMessage"] = "❌ Giải đấu đã đủ số lượng đội tham gia!";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            var tournamentTeam = new TournamentTeam
            {
                TournamentId = tournamentId,
                TeamId = teamId,
                IsApproved = !tournament.needApproved
            };

            await _tournamentTeamRepo.AddTournamentTeamAsync(tournamentTeam);

            team.inTournament += 1;
            await _teamRepo.UpdateTeamAsync(team);

            string msg = tournament.needApproved
                ? "Đăng ký thành công! Vui lòng chờ Ban tổ chức phê duyệt."
                : $"Đăng ký thành công! Đội '{team.ShortName}' đã gia nhập giải.";

            TempData["SuccessMessage"] = msg;
            return RedirectToAction("Details", new { id = tournamentId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ApproveTeam(int tournamentId, int teamId)
        {
            var userId = _userManager.GetUserId(User);
            var tournament = await _tournamentRepo.GetTournamentByIdAsync(tournamentId);

            if (tournament == null || tournament.ApplicationUserId != userId) return Forbid();

            if (tournament.TournamentTeams.Count(t => !tournament.needApproved || t.IsApproved) > tournament.Size)
            {
                TempData["ErrorMessage"] = "❌ Không thể phê duyệt! Giải đấu đã đủ số lượng đội tham gia.";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            if (tournament.Fees > 0)
            {
                // A. TẠO HÓA ĐƠN CHÍNH (BILL)
                var bill = new Bill
                {
                    TournamentId = tournamentId,
                    TeamId = teamId,
                    Fee = tournament.Fees, // Tổng tiền
                    isPaid = false,
                    DateCreate = DateTime.Now
                };
                await _billRepo.AddBillAsync(bill);

            }

            await _tournamentTeamRepo.ApproveTeamAsync(tournamentId, teamId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Đã phê duyệt!" });
            }

            return RedirectToAction("Details", new { id = tournamentId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AutoSchedule(int tournamentId, DateTime startDate, List<string> playDays, TimeSpan dailyStartTime, int gameDuration, int dailyGames, int playoffTeams = 4)
        {
            var userId = _userManager.GetUserId(User);
            var tournament = await _tournamentRepo.GetTournamentByIdAsync(tournamentId);

            if (tournament == null || tournament.ApplicationUserId != userId) return Unauthorized();

            var teams = tournament.TournamentTeams.Where(tt => !tournament.needApproved || tt.IsApproved).Select(tt => tt.TeamId).ToList();
            if (teams.Count < 2)
            {
                TempData["ErrorMessage"] = "Cần ít nhất 2 đội để xếp lịch thi đấu!";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            await _matchRepo.DeleteMatchesByTournamentAsync(tournamentId);

            var matchesToSchedule = new List<(int Home, int Away)>();
            var rng = new Random();

            if (tournament.FormatId == 1 || tournament.Format?.Name == "Round Robin")
            {
                var firstLeg = new List<(int Home, int Away)>();
                var secondLeg = new List<(int Home, int Away)>();
                for (int i = 0; i < teams.Count; i++)
                {
                    for (int j = i + 1; j < teams.Count; j++)
                    {
                        firstLeg.Add((teams[i], teams[j]));
                        secondLeg.Add((teams[j], teams[i]));
                    }
                }
                matchesToSchedule.AddRange(firstLeg.OrderBy(a => rng.Next()));
                matchesToSchedule.AddRange(secondLeg.OrderBy(a => rng.Next()));
            }
            else if (tournament.FormatId == 2 || tournament.Format?.Name == "Knockout")
            {
                var shuffledTeams = teams.OrderBy(a => rng.Next()).ToList();
                for (int i = 0; i < shuffledTeams.Count - 1; i += 2)
                {
                    matchesToSchedule.Add((shuffledTeams[i], shuffledTeams[i + 1]));
                }
            }
            else if (tournament.FormatId == 3 || tournament.Format?.Name == "Group Stage + Knockout")
            {
                for (int i = 0; i < teams.Count; i++)
                {
                    for (int j = i + 1; j < teams.Count; j++)
                    {
                        matchesToSchedule.Add((teams[i], teams[j]));
                    }
                }
                matchesToSchedule = matchesToSchedule.OrderBy(a => rng.Next()).ToList();
            }

            var allowedDays = new List<DayOfWeek>();
            if (playDays != null && playDays.Any())
            {
                foreach (var day in playDays)
                {
                    if (Enum.TryParse<DayOfWeek>(day, true, out var dayOfWeek))
                        allowedDays.Add(dayOfWeek);
                }
            }
            if (!allowedDays.Any()) allowedDays.Add(DayOfWeek.Sunday);

            DateTime currentSlot = startDate.Date.Add(dailyStartTime);
            int gamesScheduledToday = 0;
            int totalMatchesCreated = 0;

            foreach (var pair in matchesToSchedule)
            {
                while (!allowedDays.Contains(currentSlot.DayOfWeek))
                {
                    currentSlot = currentSlot.AddDays(1).Date.Add(dailyStartTime);
                    gamesScheduledToday = 0;
                }

                // LƯỢT ĐI VÒNG 1
                var match = new Match { TournamentId = tournament.Id, HomeId = pair.Home, AwayId = pair.Away, StartDate = currentSlot, StatusId = 1, Round = 1 };
                await _matchRepo.AddMatchAsync(match);
                totalMatchesCreated++;

                // LƯỢT VỀ VÒNG 1
                if (tournament.FormatId == 2 || tournament.Format?.Name == "Knockout")
                {
                    var secondLegMatch = new Match { TournamentId = tournament.Id, HomeId = pair.Away, AwayId = pair.Home, StartDate = currentSlot.AddDays(7), StatusId = 1, Round = 2 };
                    await _matchRepo.AddMatchAsync(secondLegMatch);
                    totalMatchesCreated++;
                }

                gamesScheduledToday++;
                if (gamesScheduledToday >= dailyGames)
                {
                    currentSlot = currentSlot.Date.AddDays(1).Add(dailyStartTime);
                    gamesScheduledToday = 0;
                }
                else { currentSlot = currentSlot.AddMinutes(gameDuration); }
            }

            TempData["SuccessMessage"] = $"Tạo thành công {totalMatchesCreated} trận đấu. Đã xóa lịch cũ (nếu có)!";
            return RedirectToAction("Details", new { id = tournamentId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveTeam(int tournamentId, int teamId)
        {
            var userId = _userManager.GetUserId(User);
            var tournament = await _tournamentRepo.GetTournamentByIdAsync(tournamentId);

            if (tournament == null || tournament.ApplicationUserId != userId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện hành động này!";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            if (tournament.IsStarted || tournament.IsFinished)
            {
                TempData["ErrorMessage"] = "❌ Giải đấu đã khởi tranh, không thể kích đội bóng ra khỏi giải!";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            await _tournamentTeamRepo.RemoveTournamentTeamAsync(tournamentId, teamId);

            var team = await _teamRepo.GetTeamByIdAsync(teamId);
            if (team != null && team.inTournament > 0)
            {
                team.inTournament -= 1;
                await _teamRepo.UpdateTeamAsync(team);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    TempData["SuccessMessage"] = $"Đã loại đội '{team.FullName}' khỏi giải đấu!";
                }
            }

            return RedirectToAction("Details", new { id = tournamentId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateScore(int tournamentId, int matchId, int homeGoals, int awayGoals, int statusId,
            List<int> homePlayerIds, List<int> homeMinutes, List<int> homeProgressIds,
            List<int> awayPlayerIds, List<int> awayMinutes, List<int> awayProgressIds)
        {
            var userId = _userManager.GetUserId(User);
            var tournament = await _tournamentRepo.GetTournamentByIdAsync(tournamentId);

            if (tournament == null || tournament.ApplicationUserId != userId) return Unauthorized();

            await _matchRepo.UpdateScoreAsync(matchId, homeGoals, awayGoals, statusId, homePlayerIds, homeMinutes, homeProgressIds, awayPlayerIds, awayMinutes, awayProgressIds);

            TempData["SuccessMessage"] = "Đã cập nhật kết quả và diễn biến trận đấu!";
            return RedirectToAction("Details", new { id = tournamentId });
        }

        // HÀM MỚI CHỌN ĐỘI ĐI TIẾP KHI HÒA TỔNG TỈ SỐ
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ResolveTie(int matchId, int advancingTeamId)
        {
            var match = await _matchRepo.GetMatchByIdAsync(matchId);
            if (match != null)
            {
                match.AdvancingTeamId = advancingTeamId;
                await _matchRepo.UpdateMatchAsync(match);
                return RedirectToAction("Details", new { id = match.TournamentId });
            }
            return NotFound();
        }

        [Authorize]
        public async Task<IActionResult> MyTournaments()
        {
            var userId = _userManager.GetUserId(User);
            var myTournaments = await _tournamentRepo.GetTournamentsByPlayerUserIdAsync(userId);
            return View(myTournaments);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> FinishTournament(int id)
        {
            var userId = _userManager.GetUserId(User);
            var tournament = await _tournamentRepo.GetTournamentByIdAsync(id);

            if (tournament == null || tournament.ApplicationUserId != userId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện hành động này!";
                return RedirectToAction(nameof(Index));
            }

            tournament.IsFinished = true;
            await _tournamentRepo.UpdateTournamentAsync(tournament);

            TempData["SuccessMessage"] = "Đã đánh dấu KẾT THÚC giải đấu thành công!";
            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ReopenTournament(int id)
        {
            var userId = _userManager.GetUserId(User);
            var tournament = await _tournamentRepo.GetTournamentByIdAsync(id);

            if (tournament == null || tournament.ApplicationUserId != userId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện hành động này!";
                return RedirectToAction(nameof(Index));
            }

            var allTournaments = await _tournamentRepo.GetAllTournamentsAsync();
            var activeCount = allTournaments.Count(t => t.ApplicationUserId == userId && !t.IsDeleted && !t.IsFinished);

            int maxAllowed = 1;
            var orgProfile = await _organizerRepo.GetOrganizerByUserIdAsync(userId);
            if (orgProfile != null)
            {
                if (orgProfile.SubscriptionId == 3) maxAllowed = 2;
                else if (orgProfile.SubscriptionId == 4) maxAllowed = 4;
            }

            if (activeCount >= maxAllowed)
            {
                TempData["ErrorMessage"] = $"❌ Lỗi: Bạn đã đạt giới hạn {maxAllowed} giải đấu đang hoạt động. Vui lòng kết thúc hoặc xóa một giải khác trước khi mở lại giải này!";
                return RedirectToAction("Details", new { id = id });
            }

            tournament.IsFinished = false;
            await _tournamentRepo.UpdateTournamentAsync(tournament);

            TempData["SuccessMessage"] = "✅ Đã MỞ LẠI giải đấu thành công!";
            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> StartTournament(int id)
        {
            var userId = _userManager.GetUserId(User);
            var tournament = await _tournamentRepo.GetTournamentByIdAsync(id);

            if (tournament == null || tournament.ApplicationUserId != userId) return Forbid();

            if (tournament.TournamentTeams == null || tournament.TournamentTeams.Count < 2)
            {
                TempData["ErrorMessage"] = "❌ Cần có ít nhất 2 đội ghi danh để bắt đầu giải đấu!";
                return RedirectToAction("Details", new { id = id });
            }

            tournament.IsStarted = true;
            await _tournamentRepo.UpdateTournamentAsync(tournament);

            TempData["SuccessMessage"] = "⚽ Giải đấu đã chính thức BẮT ĐẦU! Các đội không thể đăng ký hay rời giải được nữa.";
            return RedirectToAction("Details", new { id = id });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> LeaveTournament(int tournamentId, int teamId)
        {
            var userId = _userManager.GetUserId(User);
            var tournament = await _tournamentRepo.GetTournamentByIdAsync(tournamentId);

            if (tournament != null && tournament.IsStarted)
            {
                TempData["ErrorMessage"] = "❌ Giải đấu đã khởi tranh, đội bóng không thể rời giải!";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            var team = await _teamRepo.GetTeamByIdAsync(teamId);
            if (team == null || team.ApplicationUserId != userId)
            {
                TempData["ErrorMessage"] = "❌ Bạn không có quyền rút đội bóng này khỏi giải!";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            await _tournamentTeamRepo.RemoveTournamentTeamAsync(tournamentId, teamId);

            if (team.inTournament > 0)
            {
                team.inTournament -= 1;
                await _teamRepo.UpdateTeamAsync(team);
            }

            TempData["SuccessMessage"] = "Đã rút đội bóng khỏi giải đấu thành công!";
            return RedirectToAction("Details", new { id = tournamentId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ScheduleNextRound(int tournamentId)
        {
            var userId = _userManager.GetUserId(User);
            var tournament = await _tournamentRepo.GetTournamentByIdAsync(tournamentId);

            if (tournament == null || tournament.ApplicationUserId != userId) return Unauthorized();

            var matches = await _matchRepo.GetMatchesByTournamentAsync(tournamentId);
            if (matches == null || !matches.Any())
            {
                TempData["ErrorMessage"] = "Chưa có trận đấu nào ở Vòng 1. Vui lòng xếp lịch Vòng 1 trước!";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            int currentRound = matches.Max(m => m.Round);
            var currentRoundMatches = matches.Where(m => m.Round == currentRound).ToList();

            if (currentRoundMatches.Any(m => m.StatusId != 3))
            {
                TempData["ErrorMessage"] = "⚠️ Tất cả các trận đấu ở vòng hiện tại phải KẾT THÚC thì mới có thể bốc thăm vòng tiếp theo!";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            var losingTeams = new List<int>();
            var handledMatchPairs = new HashSet<int>();

            foreach (var match in matches.Where(m => m.StatusId == 3))
            {
                if (handledMatchPairs.Contains(match.Id)) continue;

                var partner = matches.FirstOrDefault(m => m.Id != match.Id && m.StatusId == 3 &&
                              ((m.HomeId == match.HomeId && m.AwayId == match.AwayId) ||
                               (m.HomeId == match.AwayId && m.AwayId == match.HomeId)));

                if (partner != null)
                {
                    int homeTotal = (match.MatchStats?.FirstOrDefault(ms => ms.TeamId == match.HomeId)?.Goals ?? 0) +
                                    (partner.MatchStats?.FirstOrDefault(ms => ms.TeamId == match.HomeId)?.Goals ?? 0);
                    int awayTotal = (match.MatchStats?.FirstOrDefault(ms => ms.TeamId == match.AwayId)?.Goals ?? 0) +
                                    (partner.MatchStats?.FirstOrDefault(ms => ms.TeamId == match.AwayId)?.Goals ?? 0);

                    if (homeTotal > awayTotal) losingTeams.Add(match.AwayId);
                    else if (awayTotal > homeTotal) losingTeams.Add(match.HomeId);
                    else
                    {
                        if (match.AdvancingTeamId.HasValue)
                            losingTeams.Add(match.AdvancingTeamId == match.HomeId ? match.AwayId : match.HomeId);
                        else if (partner.AdvancingTeamId.HasValue)
                            losingTeams.Add(partner.AdvancingTeamId == match.HomeId ? match.AwayId : match.HomeId);
                        else
                        {
                            TempData["ErrorMessage"] = "⚠️ Có trận HÒA TỔNG TỈ SỐ chưa được phân định! Vui lòng chọn đội đi tiếp trước khi bốc thăm.";
                            return RedirectToAction("Details", new { id = tournamentId });
                        }
                    }
                    handledMatchPairs.Add(match.Id);
                    handledMatchPairs.Add(partner.Id);
                }
            }

            var allApprovedTeams = tournament.TournamentTeams.Where(t => !tournament.needApproved || t.IsApproved).Select(t => t.TeamId).ToList();
            var aliveTeams = allApprovedTeams.Except(losingTeams).ToList();

            if (aliveTeams.Count < 2)
            {
                TempData["SuccessMessage"] = "🎉 Giải đấu đã tìm ra nhà Vô địch! Lịch thi đấu đã hoàn tất.";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            var rng = new Random();
            var shuffledTeams = aliveTeams.OrderBy(a => rng.Next()).ToList();

            DateTime nextStartDate = currentRoundMatches.Max(m => m.StartDate).AddDays(1);
            int matchesCreated = 0;

            for (int i = 0; i < shuffledTeams.Count - 1; i += 2)
            {
                var match = new Match { TournamentId = tournament.Id, HomeId = shuffledTeams[i], AwayId = shuffledTeams[i + 1], StartDate = nextStartDate, StatusId = 1, Round = currentRound + 1 };
                await _matchRepo.AddMatchAsync(match);
                matchesCreated++;

                var secondLegMatch = new Match { TournamentId = tournament.Id, HomeId = shuffledTeams[i + 1], AwayId = shuffledTeams[i], StartDate = nextStartDate.AddDays(7), StatusId = 1, Round = currentRound + 2 };
                await _matchRepo.AddMatchAsync(secondLegMatch);
                matchesCreated++;

                nextStartDate = nextStartDate.AddMinutes(90);
            }

            TempData["SuccessMessage"] = $"✅ Đã bốc thăm thành công {matchesCreated} trận đấu cho VÒNG TIẾP THEO!";
            return RedirectToAction("Details", new { id = tournamentId });
        }

        [Authorize]
        public async Task<IActionResult> Checkout(int tournamentId, int teamId)
        {
            var tournament = await _tournamentRepo.GetTournamentByIdAsync(tournamentId);
            var team = await _teamRepo.GetTeamByIdAsync(teamId);

            if (tournament == null || team == null) return NotFound();

            // Nếu giải miễn phí -> Bỏ qua thanh toán, gọi thẳng hàm RegisterTeam cũ
            if (tournament.Fees <= 0 || tournament.needApproved)
            {
                return await RegisterTeam(tournamentId, teamId);
            }

            ViewBag.TeamId = teamId;
            ViewBag.TeamName = team.FullName;

            return View(tournament);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ProcessPayment(int tournamentId, int teamId)
        {
            var userId = _userManager.GetUserId(User);
            var tournament = await _tournamentRepo.GetTournamentByIdAsync(tournamentId);
            var team = await _teamRepo.GetTeamByIdAsync(teamId);

            // ... (Các bước kiểm tra giải đầy, đã khởi tranh tương tự hàm RegisterTeam) ...
            if (tournament.TournamentTeams.Count(t => !tournament.needApproved || t.IsApproved) >= tournament.Size)
            {
                TempData["ErrorMessage"] = "❌ Giải đấu đã đủ số lượng đội tham gia!";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            // A. TẠO HÓA ĐƠN CHÍNH (BILL)
            var bill = new Bill
            {
                TournamentId = tournamentId,
                TeamId = teamId,
                Fee = tournament.Fees, // Tổng tiền
                isPaid = false,        // Mới trả 50% nên chưa hoàn tất
                DateCreate = DateTime.Now
            };
            await _billRepo.AddBillAsync(bill);

            // B. TẠO LỊCH SỬ THANH TOÁN CỌC 50% (BILL DETAILS)
            var depositAmount = tournament.Fees / 2; // Tính 50%
            var billDetail = new BillDetails
            {
                BillId = bill.Id,
                FeePaid = depositAmount,
                DatePay = DateTime.Now
            };
            await _billDetailsRepo.AddAsync(billDetail);

            // C. LƯU ĐỘI VÀO GIẢI ĐẤU
            var tournamentTeam = new TournamentTeam
            {
                TournamentId = tournamentId,
                TeamId = teamId,
                IsApproved = !tournament.needApproved
            };
            await _tournamentTeamRepo.AddTournamentTeamAsync(tournamentTeam);

            team.inTournament += 1;
            await _teamRepo.UpdateTeamAsync(team);

            TempData["SuccessMessage"] = $"Đã thanh toán thành công {depositAmount:N0} VNĐ (50% cọc). Đội bóng đã được ghi danh!";
            return RedirectToAction("Details", new { id = tournamentId });
        }
    }
}