using System;
using System.Linq;
using System.Web.Mvc;
using Laundry_Online_Web_BE.Models.Repositories;
using Laundry_Online_Web_FE.Models.ModelViews;
using Laundry_Online_Web_FE.Models.Repositories;

namespace Laundry_Online_Web_FE.Controllers
{
    public class BookingController : Controller
    {
        // GET: My Bookings - Danh sách tất cả lịch đặt của khách hàng
        public ActionResult MyBookings()
        {
            // Kiểm tra đăng nhập
            if (Session["customer"] == null)
            {
                TempData["Message"] = "Bạn cần đăng nhập để xem lịch đặt.";
                return RedirectToAction("Login", "Home");
            }

            var customer = Session["customer"] as CustomerView;

            // Lấy tất cả bookings của khách hàng
            var allBookings = InvoiceRepository.Instance.GetByCustomerId(customer.Id);

            // Sắp xếp theo ngày tạo mới nhất
            var sortedBookings = allBookings.OrderByDescending(b => b.Invoice_Date).ToList();

            return View(sortedBookings);
        }

        // GET: Edit Booking
        public ActionResult Edit(int id)
        {
            // Kiểm tra đăng nhập
            if (Session["customer"] == null)
            {
                TempData["Message"] = "Bạn cần đăng nhập để chỉnh sửa lịch đặt.";
                return RedirectToAction("Login", "Home");
            }

            var customer = Session["customer"] as CustomerView;
            var booking = InvoiceRepository.Instance.GetById(id);

            // Kiểm tra quyền sở hữu
            if (booking == null || booking.Customer_Id != customer.Id)
            {
                TempData["ErrorMessage"] = "Không tìm thấy lịch đặt hoặc bạn không có quyền chỉnh sửa.";
                return RedirectToAction("MyBookings");
            }

            // Kiểm tra booking phải có order_status = 0 (đang chờ xử lý)
            if (booking.Order_Status != 0)
            {
                TempData["ErrorMessage"] = "Chỉ có thể chỉnh sửa những lịch đặt đang chờ xác nhận.";
                return RedirectToAction("MyBookings");
            }

            // Kiểm tra có thể chỉnh sửa không (trước 24h)
            if (!CanEditBooking(booking))
            {
                TempData["ErrorMessage"] = "Không thể chỉnh sửa lịch đặt này. Lịch đặt phải được chỉnh sửa trước 24 giờ.";
                return RedirectToAction("MyBookings");
            }

            return View(booking);
        }

        // POST: Update Booking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, string ServiceTime, string ServiceDate, string Notes)
        {
            try
            {
                if (Session["customer"] == null)
                {
                    return Json(new { success = false, message = "Phiên đăng nhập hết hạn." });
                }

                var customer = Session["customer"] as CustomerView;
                var booking = InvoiceRepository.Instance.GetById(id);

                if (booking == null || booking.Customer_Id != customer.Id)
                {
                    return Json(new { success = false, message = "Không tìm thấy lịch đặt." });
                }

                // Kiểm tra order_status phải là 0
                if (booking.Order_Status != 0)
                {
                    return Json(new { success = false, message = "Chỉ có thể chỉnh sửa lịch đặt đang chờ xác nhận." });
                }

                if (!CanEditBooking(booking))
                {
                    return Json(new { success = false, message = "Không thể chỉnh sửa lịch đặt này." });
                }

                // Parse new date and time
                DateTime newServiceDateTime;
                if (!DateTime.TryParse(ServiceDate, out newServiceDateTime))
                {
                    return Json(new { success = false, message = "Ngày không hợp lệ." });
                }

                // Combine date and time
                if (!string.IsNullOrEmpty(ServiceTime))
                {
                    if (TimeSpan.TryParse(ServiceTime, out TimeSpan timeSpan))
                    {
                        newServiceDateTime = newServiceDateTime.Date.Add(timeSpan);
                    }
                }

                // Kiểm tra ngày mới phải trong tương lai
                if (newServiceDateTime <= DateTime.Now.AddHours(24))
                {
                    return Json(new { success = false, message = "Thời gian đặt lịch phải sau ít nhất 24 giờ." });
                }

                // Cập nhật thông tin
                booking.Delivery_Date = newServiceDateTime;
                booking.Pickup_Date = newServiceDateTime;

                // Thêm log vào Notes
                var updateLog = $"\n[CẬP NHẬT] {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}: Thay đổi thời gian thành {newServiceDateTime.ToString("dd/MM/yyyy HH:mm")}";
                booking.Notes = (booking.Notes ?? "") + updateLog;

                // Thêm notes mới nếu có
                if (!string.IsNullOrEmpty(Notes) && Notes != booking.Notes)
                {
                    booking.Notes += $"\n[GHI CHÚ THÊM]: {Notes}";
                }

                bool success = InvoiceRepository.Instance.Update(booking);

                if (success)
                {
                    TempData["SuccessMessage"] = "Cập nhật lịch đặt thành công!";
                    return Json(new { success = true, message = "Cập nhật thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Edit Booking Error: " + ex.Message);
                return Json(new { success = false, message = "Đã xảy ra lỗi. Vui lòng thử lại." });
            }
        }

        // POST: Cancel Booking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Cancel(int id, string cancelReason)
        {
            try
            {
                if (Session["customer"] == null)
                {
                    return Json(new { success = false, message = "Phiên đăng nhập hết hạn." });
                }

                var customer = Session["customer"] as CustomerView;
                var booking = InvoiceRepository.Instance.GetById(id);

                if (booking == null || booking.Customer_Id != customer.Id)
                {
                    return Json(new { success = false, message = "Không tìm thấy lịch đặt." });
                }

                // Kiểm tra order_status phải là 0
                if (booking.Order_Status != 0)
                {
                    return Json(new { success = false, message = "Chỉ có thể hủy lịch đặt đang chờ xác nhận." });
                }

                if (!CanCancelBooking(booking))
                {
                    return Json(new { success = false, message = "Không thể hủy lịch đặt này." });
                }

                // Cập nhật trạng thái hủy
                booking.Order_Status = 99; // 99 = Cancelled
                booking.Notes = (booking.Notes ?? "") + $"\n[HỦY LỊCH] {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}: {(cancelReason ?? "Khách hàng hủy lịch")}";

                bool success = InvoiceRepository.Instance.Update(booking);

                if (success)
                {
                    return Json(new { success = true, message = "Hủy lịch đặt thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi hủy lịch." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Cancel Booking Error: " + ex.Message);
                return Json(new { success = false, message = "Đã xảy ra lỗi. Vui lòng thử lại." });
            }
        }

        // Helper methods
        private bool CanEditBooking(InvoiceView booking)
        {
            // Chỉ cho phép chỉnh sửa nếu:
            // 1. Order_Status = 0 (đang chờ xử lý)
            // 2. Lịch đặt chưa bắt đầu (delivery_date > now + 24h)

            if (booking.Order_Status != 0) return false; // Chỉ cho phép edit status = 0

            var deliveryTime = booking.Delivery_Date ?? DateTime.MaxValue;
            return deliveryTime > DateTime.Now.AddHours(12);
        }

        private bool CanCancelBooking(InvoiceView booking)
        {
            // Chỉ cho phép hủy nếu:
            // 1. Order_Status = 0 (đang chờ xử lý)
            // 2. Lịch đặt chưa bắt đầu (trước 2h)

            if (booking.Order_Status != 0) return false; // Chỉ cho phép cancel status = 0

            var deliveryTime = booking.Delivery_Date ?? DateTime.MaxValue;
            return deliveryTime > DateTime.Now.AddHours(2); // Cho phép hủy trước 2h
        }
    }
}