using System;

namespace Laundry_Online_Web_FE.Models.ModelViews.Payments
{
    public class PaymentResponseModel
    {
        // Mã đơn hàng từ hệ thống (ví dụ: Id hóa đơn)
        public string OrderId { get; set; }

        // Mã giao dịch từ phía VNPay
        public string TransactionId { get; set; }

        // Số tiền thanh toán (VNĐ)
        public decimal Amount { get; set; }

        // Trạng thái giao dịch thành công hay không
        public bool Success { get; set; }

        // Mã phản hồi của VNPay (vnp_ResponseCode)
        public string VnPayResponseCode { get; set; }

        // Mô tả đơn hàng (hiển thị trên giao diện)
        public string OrderDescription { get; set; }

        // Mã phương thức thanh toán (nếu có phân biệt)
        public string PaymentMethod { get; set; } = "VNPay";

        // Mã thanh toán nội bộ (nếu cần lưu vào DB)
        public string PaymentId { get; set; }

        // Chuỗi Token hoặc mã bảo mật (nếu dùng cho redirect / verify thêm)
        public string Token { get; set; }

        // Thời gian thực hiện thanh toán
        public DateTime? PaymentTime { get; set; }
    }
}
