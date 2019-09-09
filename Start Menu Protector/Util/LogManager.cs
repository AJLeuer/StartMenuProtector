using System.IO;
using static StartMenuProtector.Configuration.FilePaths;
using File = StartMenuProtector.Data.File;

namespace StartMenuProtector.Util
{
    public static class LogManager
    {
        public  static File                 LogFile { get; set; }
        private static StreamWriter         LogWriter = null;
        private static NodaTime.SystemClock Clock;

        public static void Start()
        {
            LogFile   = new File(LogFilePath);
            LogWriter = new StreamWriter(LogFile.Path);
            Clock     = NodaTime.SystemClock.Instance;
        }

        public static void Stop()
        {
            LogWriter?.Close();
        }

        public static void Log<OutputType>(OutputType output)
        {
            LogWriter?.Write($"{Clock.GetCurrentInstant().ToString()} ");
            LogWriter?.WriteLine(output);
            LogWriter?.Flush();
        }
    }
}