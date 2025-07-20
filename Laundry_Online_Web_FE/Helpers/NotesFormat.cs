using System.Collections.Generic;

namespace Laundry_Online_Web_FE.Helpers
{
    public class NotesFormat
    {
        public List<string> UserNotes { get; set; } = new List<string>();
        public List<LogEntry> SystemLogs { get; set; } = new List<LogEntry>();
    }
}