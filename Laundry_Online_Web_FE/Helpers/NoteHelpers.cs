using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Laundry_Online_Web_FE.Helpers
{
    public static class NotesHelper
    {
        public static string FormatNotesForDisplay(string notes)
        {
            if (string.IsNullOrEmpty(notes))
                return "No notes";

            // Parse và format các log entries
            var formattedNotes = ParseNotesEntries(notes);

            // Trả về summary ngắn gọn cho table view
            if (formattedNotes.UserNotes.Any())
                return formattedNotes.UserNotes.First();

            if (formattedNotes.SystemLogs.Any())
                return formattedNotes.SystemLogs.Last().DisplayText;

            return "No notes";
        }

        public static string GetNotesTooltip(string notes)
        {
            if (string.IsNullOrEmpty(notes))
                return "No notes available";

            var formattedNotes = ParseNotesEntries(notes);
            var tooltip = new List<string>();

            // Thêm user notes
            if (formattedNotes.UserNotes.Any())
            {
                tooltip.Add("📝 Your Notes:");
                tooltip.AddRange(formattedNotes.UserNotes.Select(n => "• " + n));
                tooltip.Add("");
            }

            // Thêm system logs (chỉ hiển thị 3 log gần nhất)
            if (formattedNotes.SystemLogs.Any())
            {
                tooltip.Add("📋 Booking History:");
                // ✅ SỬA: Thay TakeLast bằng Skip + Take
                var recentLogs = formattedNotes.SystemLogs.Skip(Math.Max(0, formattedNotes.SystemLogs.Count - 3));
                tooltip.AddRange(recentLogs.Select(log => $"• {log.Date:dd/MM HH:mm} - {log.DisplayText}"));
            }

            return string.Join("\\n", tooltip);
        }

        public static NotesFormat ParseNotesEntries(string notes)
        {
            if (string.IsNullOrEmpty(notes))
                return new NotesFormat();

            var result = new NotesFormat
            {
                UserNotes = new List<string>(),
                SystemLogs = new List<LogEntry>()
            };

            // Split by lines và parse từng entry
            var lines = notes.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                if (string.IsNullOrEmpty(trimmedLine))
                    continue;

                // Parse system logs
                if (trimmedLine.StartsWith("[") && trimmedLine.Contains("]"))
                {
                    var logEntry = ParseSystemLog(trimmedLine);
                    if (logEntry != null)
                        result.SystemLogs.Add(logEntry);
                }
                else
                {
                    // User notes
                    if (!string.IsNullOrWhiteSpace(trimmedLine))
                        result.UserNotes.Add(trimmedLine);
                }
            }

            return result;
        }

        private static LogEntry ParseSystemLog(string logLine)
        {
            try
            {
                // Parse [ACTION] timestamp: message
                var match = Regex.Match(
                    logLine,
                    @"\[([^\]]+)\]\s*(\d{2}/\d{2}/\d{4}\s*\d{2}:\d{2}):\s*(.+)"
                );

                if (match.Success)
                {
                    var action = match.Groups[1].Value;
                    var dateStr = match.Groups[2].Value;
                    var message = match.Groups[3].Value;

                    if (DateTime.TryParseExact(dateStr, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime date))
                    {
                        return new LogEntry
                        {
                            Action = action,
                            Date = date,
                            Message = message,
                            DisplayText = GetFriendlyLogMessage(action, message)
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing log entry: {ex.Message}");
            }

            return null;
        }

        private static string GetFriendlyLogMessage(string action, string message)
        {
            switch (action.ToUpper().Trim())
            {
                case "CREATED":
                    return "Booking created";
                case "UPDATED":
                    if (message.Contains("Changed appointment time"))
                        return "Appointment time changed";
                    return "Booking updated";
                case "AUTO-CANCELLED":
                case "AUTO CANCELLED":
                    return "Auto-cancelled (no confirmation)";
                case "ADMIN UPDATE":
                    return "Status updated by staff";
                default:
                    return action.ToLower().Replace("-", " ");
            }
        }

        public static string ExtractUserNotes(string notes)
        {
            if (string.IsNullOrEmpty(notes)) return "";

            var lines = notes.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var userNotes = lines.Where(line => !line.Trim().StartsWith("[")).ToList();

            return string.Join(" ", userNotes).Trim();
        }

        public static string ExtractSystemLogs(string notes)
        {
            if (string.IsNullOrEmpty(notes)) return "";

            var lines = notes.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var systemLogs = lines.Where(line => line.Trim().StartsWith("[")).ToList();

            return string.Join("\n", systemLogs);
        }

        public static string FormatNotesForSaving(string userNotes, string existingNotes, string systemLog)
        {
            var result = new StringBuilder();

            // Add existing notes if any
            if (!string.IsNullOrEmpty(existingNotes))
            {
                result.Append(existingNotes);
            }

            // Add user notes if provided
            if (!string.IsNullOrEmpty(userNotes))
            {
                if (result.Length > 0) result.Append("\n");
                result.Append(userNotes);
            }

            // Add system log if provided
            if (!string.IsNullOrEmpty(systemLog))
            {
                if (result.Length > 0) result.Append("\n");
                result.Append(systemLog);
            }

            // ✅ NO LIMITS: Return full string
            return result.ToString();
        }

        // ✅ THÊM: Helper method để lấy N items cuối cùng (thay thế TakeLast)
        public static IEnumerable<T> TakeLastItems<T>(IEnumerable<T> source, int count)
        {
            var list = source.ToList();
            return list.Skip(Math.Max(0, list.Count - count));
        }
    }
}