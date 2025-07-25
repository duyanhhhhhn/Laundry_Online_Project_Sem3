﻿using Laundry_Online_Web_FE.Models.Entities;
using Laundry_Online_Web_FE.Models.ModelViews;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Laundry_Online_Web_FE.Models.Dao
{
    public class ReportServiceDao
    {
        public DailyCollectionReportModel GetDailyCollectionReport(DateTime reportDate)
        {
            var viewModel = new DailyCollectionReportModel();

            try
            {
                using (var context = new OnlineLaundryEntities())
                {
                    viewModel.Summary = (from inv in context.Invoices
                                         where inv.pickup_date.HasValue &&
                                               DbFunctions.TruncateTime(inv.pickup_date.Value) == DbFunctions.TruncateTime(reportDate) &&
                                               inv.order_status == 2
                                         group inv by DbFunctions.TruncateTime(inv.pickup_date.Value) into g
                                         select new DailyCollectionSummaryModel
                                         {
                                             ReportDate = g.Key.Value,
                                             TotalRevenue = g.Sum(inv => inv.total_amount),
                                             TotalInvoices = g.Count()
                                         }).FirstOrDefault();

                    if (viewModel.Summary == null)
                    {
                        viewModel.Summary = new DailyCollectionSummaryModel { ReportDate = reportDate, TotalRevenue = 0, TotalInvoices = 0 };
                    }

                    viewModel.Summary.PaymentTypeBreakdown = (from inv in context.Invoices
                                                              where inv.pickup_date.HasValue &&
                                                                    DbFunctions.TruncateTime(inv.pickup_date.Value) == DbFunctions.TruncateTime(reportDate) &&
                                                                    inv.order_status == 2
                                                              group inv by inv.payment_type into g
                                                              select new PaymentTypeCollectionView
                                                              {
                                                                  PaymentType = g.Key == 1 ? "Cash" :
                                                                                g.Key == 2 ? "VNPay" :
                                                                                g.Key == 3 ? "QR Code" : "Unknown",
                                                                  TotalAmount = g.Sum(inv => inv.total_amount),
                                                                  Count = g.Count()
                                                              }).ToList();

                    viewModel.Details = (from inv in context.Invoices
                                         join cust in context.Customers on inv.customer_id equals cust.customer_id
                                         where inv.pickup_date.HasValue &&
                                               DbFunctions.TruncateTime(inv.pickup_date.Value) == DbFunctions.TruncateTime(reportDate) &&
                                               inv.order_status == 2
                                         select new DailyCollectionItemModel
                                         {
                                             InvoiceId = inv.invoice_id,
                                             CustomerName = cust.first_name + " " + cust.last_name,
                                             PickupDate = inv.pickup_date.Value,
                                             TotalAmount = inv.total_amount,
                                             PaymentType = inv.payment_type == 1 ? "Cash" :
                                                           inv.payment_type == 2 ? "VNPay" :
                                                           inv.payment_type == 3 ? "QR Code" : "Unknown",
                                             Notes = inv.notes
                                         }).ToList();

                    if (!viewModel.Details.Any() && viewModel.Summary.TotalRevenue == 0)
                    {
                        viewModel.ErrorMessage = "No data found for the selected date.";
                    }
                }
            }
            catch (Exception ex)
            {
                viewModel.ErrorMessage = "An error occurred while loading data: " + ex.Message;
            }

            return viewModel;
        }

        public List<GarmentCollectionView> GetGarmentCollectionReport(DateTime startDate, DateTime endDate)
        {
            using (var context = new OnlineLaundryEntities())
            {
                var details = (from item in context.InvoiceItems
                               join service in context.Services on item.s_id equals service.s_id
                               join invoice in context.Invoices on item.invoice_id equals invoice.invoice_id
                               where invoice.order_status == 2 &&
                                     invoice.pickup_date.HasValue &&
                                     DbFunctions.TruncateTime(invoice.pickup_date.Value) >= DbFunctions.TruncateTime(startDate) &&
                                     DbFunctions.TruncateTime(invoice.pickup_date.Value) <= DbFunctions.TruncateTime(endDate)
                               group new { item, service } by new { service.s_title, item.item_name } into g
                               select new GarmentCollectionView
                               {
                                   ServiceName = g.Key.s_title ?? g.Key.item_name,
                                   ItemName = g.Key.item_name,
                                   TotalQuantity = g.Sum(x => x.item.quantity),
                                   TotalRevenue = g.Sum(x => x.item.sub_total)
                               }).OrderByDescending(x => x.TotalRevenue).ToList();

                return details;
            }
        }

        public PaymentTypeTotalReportModel GetPaymentTypeTotals(DateTime startDate, DateTime endDate)
        {
            var viewModel = new PaymentTypeTotalReportModel
            {
                StartDate = startDate,
                EndDate = endDate,
                PaymentSummaries = new List<PaymentTypeCollectionView>()
            };

            try
            {
                using (var context = new OnlineLaundryEntities())
                {
                    var summaries = (from inv in context.Invoices
                                     where inv.pickup_date.HasValue &&
                                           DbFunctions.TruncateTime(inv.pickup_date.Value) >= DbFunctions.TruncateTime(startDate) &&
                                           DbFunctions.TruncateTime(inv.pickup_date.Value) <= DbFunctions.TruncateTime(endDate) &&
                                           inv.order_status == 2
                                     group inv by inv.payment_type into g
                                     select new PaymentTypeCollectionView
                                     {
                                         PaymentType = g.Key == 1 ? "Cash" :
                                                       g.Key == 2 ? "VNPay" :
                                                       g.Key == 3 ? "QR Code" : "Unknown",
                                         TotalAmount = g.Sum(inv => inv.total_amount),
                                         Count = g.Count()
                                     }).ToList();

                    viewModel.PaymentSummaries.AddRange(summaries);

                    // Bổ sung nếu thiếu loại
                    if (!viewModel.PaymentSummaries.Any(s => s.PaymentType == "Cash"))
                        viewModel.PaymentSummaries.Add(new PaymentTypeCollectionView { PaymentType = "Cash", TotalAmount = 0, Count = 0 });

                    if (!viewModel.PaymentSummaries.Any(s => s.PaymentType == "VNPay"))
                        viewModel.PaymentSummaries.Add(new PaymentTypeCollectionView { PaymentType = "VNPay", TotalAmount = 0, Count = 0 });

                    if (!viewModel.PaymentSummaries.Any(s => s.PaymentType == "QR Code"))
                        viewModel.PaymentSummaries.Add(new PaymentTypeCollectionView { PaymentType = "QR Code", TotalAmount = 0, Count = 0 });

                    // Sắp xếp: Cash trước
                    viewModel.PaymentSummaries = viewModel.PaymentSummaries
                                                    .OrderBy(s => s.PaymentType == "Cash" ? 0 :
                                                                 s.PaymentType == "VNPay" ? 1 : 2)
                                                    .ToList();

                    if (!viewModel.PaymentSummaries.Any())
                    {
                        viewModel.ErrorMessage = "No payment data found for the selected period.";
                    }
                }
            }
            catch (Exception ex)
            {
                viewModel.ErrorMessage = "An error occurred while loading payment data: " + ex.Message;
            }

            return viewModel;
        }
    }
}
