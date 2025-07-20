using System;

namespace Laundry_Online_Web_FE.Helpers
{
    public class LogEntry
    {
        public string Action { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }
        public string DisplayText { get; set; }
    }
}