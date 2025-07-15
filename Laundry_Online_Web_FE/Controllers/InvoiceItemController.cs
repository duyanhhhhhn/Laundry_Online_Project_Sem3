using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laundry_Online_Web_FE.Models.ModelViews;
using Laundry_Online_Web_FE.Models.Repositories;

namespace Laundry_Online_Web_FE.Controllers
{
    public class InvoiceItemController : Controller
    {
        // GET: InvoiceItem
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult InVoiceItemList()
        {
            var items = InvoiceItemRepo.Instance.GetAll(); //list<>
            return View(items);
        }
        public ActionResult Details(int id)
        {
            var item = InvoiceItemRepo.Instance.GetInvoiceItemById(id);
            if (item == null)
                return HttpNotFound();
            return View(item);
        }
        public ActionResult CreateBarcodeByInvoiceItem(int invoiceItemId) // tạo mã vạch cho item
        {
            var item = InvoiceItemRepo.Instance.GetInvoiceItemById(invoiceItemId);
            if (item == null)
                return HttpNotFound();
            // Lấy thông tin dịch vụ để tạo mã vạch
            var service = ServiceRepository.Instance.GetById(item.ServiceId);
            if (service == null)
                return HttpNotFound();
            GenerateBarcodesForItem(item, service);
            InvoiceItemRepo.Instance.UpdateInvoiceItem(item);
            return RedirectToAction("Details", new { id = item.Id });
        }
        // Xử lý tạo barcode và lưu vào BarCode field (1 hoặc nhiều)
        public void GenerateBarcodesForItem(InvoiceItemView item, ServiceView service)
        {
            List<string> barcodeList = new List<string>();

            if (service.Unit.ToLower().Contains("kg"))
            {
                // Tính theo kg → chỉ cần 1 mã vạch
                string code = $"INV{item.InvoiceId}-S{item.ServiceId}-{DateTime.Now.Ticks}";
                barcodeList.Add(code);
                BarcodeHelper.SaveBarcodeToFile(code, BarcodeHelper.GenerateBarcode(code));
            }
            else
            {
                // Tính theo item → tạo nhiều mã vạch theo Quantity
                int qty = (int)item.Quantity;
                for (int i = 1; i <= qty; i++)
                {
                    string code = $"INV{item.InvoiceId}-S{item.ServiceId}-#{i}-{DateTime.Now.Ticks + i}";
                    barcodeList.Add(code);
                    BarcodeHelper.SaveBarcodeToFile(code, BarcodeHelper.GenerateBarcode(code));
                }
            }
            item.BarCode = string.Join("|", barcodeList);
        }

        // Xuất PDF chứa tất cả barcode của một item
        public ActionResult PrintBarcodes(int InvoiceItemId) // xuất ra file PDF
        {
            var item = InvoiceItemRepo.Instance.GetInvoiceItemById(InvoiceItemId);
            if (item == null || string.IsNullOrEmpty(item.BarCode))
                return HttpNotFound();

            var barcodeList = item.BarCode.Split('|').ToList();
            var pdfUrl = BarcodeHelper.GenerateBarcodePdf(barcodeList);
            return Redirect(pdfUrl);
        }

        // Giao diện nhập barcode từ máy quét
        public ActionResult ScanBarcodeForm()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ScanBarcodeForm(string scannedCode)
        {
            var item = InvoiceItemRepo.Instance.GetItemByBarCode(scannedCode);
            if (item == null)
            {
                ViewBag.Error = "Không tìm thấy mã vạch này.";
                return View();
            }
            return RedirectToAction("Details", new { id = item.Id });
        }
    }

}