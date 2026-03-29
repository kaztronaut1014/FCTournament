using FCTournament.Models;
using FCTournament.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FCTournament.Controllers
{
    [Authorize]
    public class BillController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBillRepository _billRepo;
        private readonly IBillDetailsRepository _billDetailsRepo;

        public BillController(
            UserManager<ApplicationUser> userManager,
            IBillRepository billRepo,
            IBillDetailsRepository billDetailsRepo)
        {
            _userManager = userManager;
            _billRepo = billRepo;
            _billDetailsRepo = billDetailsRepo;
        }

        // 1. TRANG DANH SÁCH HÓA ĐƠN CỦA TÔI
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // Lấy các hóa đơn thuộc về đội bóng do User này quản lý
            var myBills = await _billRepo.GetBillsByUserIdAsync(userId);

            // Lấy tổng số tiền đã trả cho từng Bill (từ bảng BillDetails)
            var paidAmounts = await _billDetailsRepo.GetPaidAmountsByUserIdAsync(userId);

            ViewBag.PaidAmounts = paidAmounts;

            return View(myBills);
        }
        [HttpGet]
        public async Task<IActionResult> CheckoutRemaining(int billId)
        {
            var bill = await _billRepo.GetBillByIdAsync(billId);

            if (bill == null || bill.isPaid) return NotFound();

            var paidAmount = await _billDetailsRepo.GetTotalPaidAmountByBillIdAsync(billId);
            var remaining = bill.Fee - paidAmount;

            ViewBag.PaidAmount = paidAmount;
            ViewBag.Remaining = remaining;

            return View(bill);
        }

        // 2. XỬ LÝ NÚT THANH TOÁN 50% CÒN LẠI
        [HttpPost]
        public async Task<IActionResult> PayRemaining(int billId)
        {
            var bill = await _billRepo.GetBillByIdAsync(billId);
            if (bill == null || bill.isPaid) return NotFound();

            // Tính số tiền còn nợ
            var paidAmount = await _billDetailsRepo.GetTotalPaidAmountByBillIdAsync(billId);
            var remaining = bill.Fee - paidAmount;

            if (remaining > 0)
            {
                // Lưu lịch sử đóng tiền lần 2
                var detail = new BillDetails
                {
                    BillId = billId,
                    FeePaid = remaining,
                    DatePay = DateTime.Now
                };
                await _billDetailsRepo.AddAsync(detail);

                // Cập nhật trạng thái Hóa đơn thành Đã hoàn tất
                bill.isPaid = true;
                bill.DatePaid = DateTime.Now;
                await _billRepo.UpdateBillAsync(bill);
                TempData["SuccessMessage"] = $"Đã thanh toán nốt {remaining:N0} VNĐ thành công!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}