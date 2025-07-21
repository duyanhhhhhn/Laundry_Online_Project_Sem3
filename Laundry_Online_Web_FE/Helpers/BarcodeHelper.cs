using System.Drawing.Imaging;
using System.IO;
using ZXing;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections.Generic;
using System.Web;
using System.Text.RegularExpressions;
using System;


public class BarcodeHelper
{
    // Sinh mã vạch dạng ảnh PNG, trả về mảng byte
    public static byte[] GenerateBarcode(string content)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.CODE_128,
            Options = new ZXing.Common.EncodingOptions
            {
                Height = 80,
                Width = 300,
                Margin = 2
            }
        };

        using (var bitmap = writer.Write(content))
        using (var ms = new MemoryStream())
        {
            bitmap.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
    }

    // Lưu ảnh mã vạch vào thư mục ~/Content/Barcodes


    public static void SaveBarcodeToFile(string barcode, byte[] data)
    {
        string folder = HttpContext.Current.Server.MapPath("~/Content/Barcodes/");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string safeFileName = Regex.Replace(barcode, @"[^a-zA-Z0-9_\-]", "_");
        string path = Path.Combine(folder, safeFileName + ".png");

        // Ghi đè nếu file tồn tại
        if (File.Exists(path))
        {
            try
            {
                File.Delete(path);
            }
            catch (IOException ioEx)
            {
                System.Diagnostics.Debug.WriteLine($"Cannot overwrite file {path}: {ioEx.Message}");
                // Đổi tên tạm thời
                path = Path.Combine(folder, safeFileName + "_" + Guid.NewGuid() + ".png");
            }
        }

        File.WriteAllBytes(path, data);
    }



    // Tạo PDF chứa danh sách mã vạch (mỗi trang 1 mã)
    public static string GenerateBarcodePdf(List<string> barcodeList)
    {
        string folder = HttpContext.Current.Server.MapPath("~/Content/Barcodes/");
        string pdfPath = Path.Combine(folder, "barcodes_output.pdf");

        using (var fs = new FileStream(pdfPath, FileMode.Create))
        using (var doc = new Document(PageSize.A4))
        using (var writer = PdfWriter.GetInstance(doc, fs))
        {
            doc.Open();
            foreach (var code in barcodeList)
            {
                var imgPath = Path.Combine(folder, code + ".png");
                if (File.Exists(imgPath))
                {
                    iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imgPath);
                    img.ScalePercent(80f);
                    img.Alignment = Element.ALIGN_CENTER;
                    doc.Add(new Paragraph(code) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(img);
                    doc.NewPage();
                }
            }
            doc.Close();
        }

        return "/Content/Barcodes/barcodes_output.pdf";
    }
}