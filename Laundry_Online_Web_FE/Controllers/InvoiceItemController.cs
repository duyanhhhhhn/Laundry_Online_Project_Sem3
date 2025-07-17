using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laundry_Online_Web_BE.Models.Repositories;
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
        [Route("Admin/InvoiceItemList")]

        public ActionResult InVoiceItemList()
        {
            // Lấy danh sách tất cả các InvoiceItem từ kho dữ liệu
            var model = InvoiceItemRepo.Instance.GetAll(); //list<>
            var serviceList = ServiceRepository.Instance.All(); // lấy danh sách dịch vụ
            // Kết hợp thông tin dịch vụ vào từng InvoiceItem
            foreach (var item in model)
            {
                var service = serviceList.FirstOrDefault(s => s.Id == item.ServiceId);
                if (service != null)
                {
                    item.ItemName = service.Title; // Gán tên dịch vụ vào ItemName
                    item.UnitPrice = service.Price ?? 0; // Gán giá dịch vụ vào UnitPrice
                }
            }
            // Trả về view với danh sách InvoiceItem đã kết hợp thông tin dịch vụ
            ViewBag.ServiceList = serviceList; // Lưu danh sách dịch vụ vào ViewBag để sử dụng trong view
            var invoiceList = InvoiceRepository.Instance.GetAll();
            ViewBag.InvoiceList = invoiceList; // Lưu danh sách hóa đơn vào ViewBag
            return View(model);
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

        //chuyển đổi mã vạch sang ảnh PNG và lưu vào thư mục
        public ActionResult GenerateBarcodeImage(int invoiceItemId)
        {
            var item = InvoiceItemRepo.Instance.GetInvoiceItemById(invoiceItemId);
            if (item == null || string.IsNullOrEmpty(item.BarCode))
                return HttpNotFound();
            // Tách các mã vạch từ chuỗi
            var barcodeList = item.BarCode.Split('|').ToList();
            foreach (var code in barcodeList)
            {
                var barcodeData = BarcodeHelper.GenerateBarcode(code);
                BarcodeHelper.SaveBarcodeToFile(code, barcodeData);
            }
            return RedirectToAction("Details", new { id = item.Id });
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
        [Route("Admin/ItemDetail/{invoiceId:int}", Name = "Admin_ItemDetail")]
        public ActionResult ItemDetail(int invoiceId)
        {
            // Lấy danh sách InvoiceItem theo InvoiceId
            var model = InvoiceItemRepo.Instance.GetItemsByInvoiceId(invoiceId);
            if (model == null)
            {
                return HttpNotFound("Not found Item.");
            }
            return View(model);
        }
        // Hiển thị danh sách mã vạch của một Invoice
        [Route("Admin/BarcodeList/{invoiceId:int}", Name = "Admin_BarcodeList")]
        public ActionResult BarcodeList(int invoiceId) 
        {
            // Lấy danh sách InvoiceItem theo InvoiceId
            var model = InvoiceItemRepo.Instance.GetItemsByInvoiceId(invoiceId);
            if (model == null || !model.Any())
            {
                return HttpNotFound("No found item by this invoice.");
            }
            // Tạo danh sách mã vạch từ các InvoiceItem
            var barcodeList = new List<string>();
            foreach (var item in model)
            {
                if (!string.IsNullOrEmpty(item.BarCode))
                {
                    barcodeList.AddRange(item.BarCode.Split('|'));
                }
            }
            if (!barcodeList.Any())
            {
                return HttpNotFound("No Barcode for this invoice Item.");
            }
            //lấy list ảnh barcode
            foreach (var code in barcodeList)
            {
                var barcodeData = BarcodeHelper.GenerateBarcode(code);
                BarcodeHelper.SaveBarcodeToFile(code, barcodeData);
            }
            var Invoice = InvoiceRepository.Instance.GetById(invoiceId);
            ViewBag.Invoice = Invoice; // Lưu thông tin hóa đơn vào ViewBag
            ViewBag.InvoiceItem = model; // Lưu danh sách InvoiceItem vào ViewBag
            ViewBag.ImageBC = barcodeList.Select(code => Url.Content($"~/Content/Barcodes/{code}.png")).ToList(); // Lưu đường dẫn ảnh mã vạch vào ViewBag
            return View(barcodeList);
        }
    }
}