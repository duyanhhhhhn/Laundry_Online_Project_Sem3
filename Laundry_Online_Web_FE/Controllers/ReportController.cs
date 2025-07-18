using System;
using System.Web.Mvc;
using System.Linq;
using Laundry_Online_Web_FE.Models.Dao;
using Laundry_Online_Web_FE.Models.ModelViews;

namespace OnlineLaundry.Controllers 
{
    public class ReportController : Controller
    {
        private ReportServiceDao _reportService = new ReportServiceDao();

        [Route("Admin/DailyCollection")]
        public ActionResult DailyCollection(DateTime? reportDate)
        {
            DateTime selectedDate = reportDate ?? DateTime.Today;

            var viewModel = _reportService.GetDailyCollectionReport(selectedDate);

            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");

            return View(viewModel);
        }
        [Route("Admin/GarmentCollection")]

        public ActionResult GarmentCollection(DateTime? startDate, DateTime? endDate)
        {
            DateTime start = startDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime end = endDate ?? DateTime.Today;

            var viewModel = _reportService.GetGarmentCollectionReport(start, end);

            ViewBag.StartDate = start.ToString("yyyy-MM-dd");
            ViewBag.EndDate = end.ToString("yyyy-MM-dd");

            return View(viewModel);
        }
        [Route("Admin/PaymentTypeTotals")]
        public ActionResult PaymentTypeTotals(DateTime? startDate, DateTime? endDate)
        {
            DateTime start = startDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime end = endDate ?? DateTime.Today;

            PaymentTypeTotalReportModel viewModel = _reportService.GetPaymentTypeTotals(start, end);

            ViewBag.StartDate = start.ToString("yyyy-MM-dd");
            ViewBag.EndDate = end.ToString("yyyy-MM-dd");

            return View(viewModel);
        }

    }
}