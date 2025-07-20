using System;
using System.Collections.Generic;
using Laundry_Online_Web_FE.Models.ModelViews; // sửa nếu namespace khác
using Laundry_Online_Web_FE.Models.ModelViews.DTO;

public static class BarcodeGeneratorHelper
{
    public static void GenerateBarcodesForItem(InvoiceItemView item, ServiceView service)
    {
        try
        {
            var barcodeList = new List<string>();

            // Debug: Log service info
            System.Diagnostics.Debug.WriteLine($"Service Unit: {service.Unit}, Service ID: {service.Id}");

            if (service.Unit != null && service.Unit.ToLower().Contains("kg"))
            {
                // Chỉ 1 mã cho item theo kg
                string code = $"INV{item.InvoiceId}-S{item.ServiceId}-{DateTime.Now.Ticks}";
                barcodeList.Add(code);

                // Debug: Log generated code
                System.Diagnostics.Debug.WriteLine($"Generated KG barcode: {code}");

                try
                {
                    BarcodeHelper.SaveBarcodeToFile(code, BarcodeHelper.GenerateBarcode(code));
                    System.Diagnostics.Debug.WriteLine($"Successfully saved barcode: {code}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving barcode {code}: {ex.Message}");
                    throw;
                }
            }
            else
            {
                // Mỗi đơn vị 1 mã (ví dụ: 3 áo → 3 mã)
                int qty = (int)item.Quantity;
                System.Diagnostics.Debug.WriteLine($"Generating {qty} individual barcodes");

                for (int i = 1; i <= qty; i++)
                {
                    string code = $"INV{item.InvoiceId}-S{item.ServiceId}-{Guid.NewGuid()}";
                    barcodeList.Add(code);

                    // Debug: Log each generated code
                    System.Diagnostics.Debug.WriteLine($"Generated individual barcode {i}: {code}");

                    try
                    {
                        BarcodeHelper.SaveBarcodeToFile(code, BarcodeHelper.GenerateBarcode(code));
                        System.Diagnostics.Debug.WriteLine($"Successfully saved barcode {i}: {code}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error saving barcode {code}: {ex.Message}");
                        throw;
                    }
                }
            }

            // Gán kết quả vào property BarCode
            item.BarCode = string.Join("|", barcodeList);
            System.Diagnostics.Debug.WriteLine($"Final BarCode property: {item.BarCode}");

        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GenerateBarcodesForItem: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw new Exception($"Failed to generate barcodes: {ex.Message}", ex);
        }
    }
}

