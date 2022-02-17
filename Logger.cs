using System;
using System.IO;
using System.Text;

namespace ToDoLi
{
    internal class Logger
    {

#if DEBUG
        private static string _filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ToDoLi\\debug\\logs");
#else
        private static string _filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ToDoLi\\logs");
#endif

        private static int _maxAgeInDays = 7;

        /// <summary>
        /// Gets or sets the file path for logs
        /// </summary>
        public static string FilePath { get => _filePath; set => _filePath = value; }

        /// <summary>
        /// Gets or sets max age for log files in days (min = 1, max = 365).
        /// </summary>
        public static int MaxAgeInDays
        {
            get => _maxAgeInDays;
            set
            {
                if (value >= 1 && value <= 365)
                {
                    _maxAgeInDays = value;
                }
            }
        }

        /// <summary>
        /// Creates a new log entry in the target file.
        /// </summary>
        /// <param name="entry">Log message</param>
        /// <returns>Returns true if successful, otherwise false.</returns>
        public static bool WriteEntry(string entry)
        {
            try
            {
                Directory.CreateDirectory(FilePath);
                StreamWriter sw = File.AppendText(GetFileName());
                sw.WriteLine(GetFormattedEntry(entry));
                sw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a log entry for each item in entries.
        /// </summary>
        /// <param name="entries">>Log messages</param>
        /// <returns>Returns true if successful, otherwise false.</returns>
        public static bool WriteEntries(string[] entries)
        {
            try
            {
                Directory.CreateDirectory(FilePath);
                StreamWriter sw = File.AppendText(GetFileName());

                foreach (string e in entries)
                {
                    sw.WriteLine(GetFormattedEntry(e));
                }

                sw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }

        // Returns a formatted string, including the datetime, for the log entry.
        private static string GetFormattedEntry(string msg)
        {
            DateTime dateTime = DateTime.Now;
            dateTime.ToLocalTime();

            StringBuilder sb = new StringBuilder(dateTime.ToString("dd/MM/yyyy HH:mm:ss"));
            sb.Append("\t");
            sb.Append(msg);

            return sb.ToString();
        }

        // Returns the full file name and path for the current log.
        private static string GetFileName()
        {
            DateTime dateTime = DateTime.Today;
            string fullName = Path.Combine(FilePath, dateTime.ToString());
            fullName = Path.ChangeExtension(fullName, ".log");
            return fullName;
        }

        /// <summary>
        /// Deletes all log files older than maxAge days.
        /// </summary>
        /// <returns>Returns true if successful, otherwise false.</returns>
        public static bool RemoveOldLogs()
        {
            try
            {
                string[] files = Directory.GetFiles(FilePath, $"*.log");

                DateTime now = DateTime.Now;

                foreach (string file in files)
                {
                    DateTime deleteDate = System.IO.File.GetCreationTime(file).AddDays(MaxAgeInDays);

                    if (DateTime.Compare(now, deleteDate) > 0)
                    {
                        System.IO.File.Delete(file);
                        WriteEntry($"Old log file deleted: {file}");
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }
    }
}
