using System.IO;
using StartMenuProtector.Configuration;
using File = StartMenuProtector.Data.File;

namespace StartMenuProtector.Util
{
    public static class LogManager
    {
        public static File LogFile { get; private set; }
        public static StreamWriter LogWriter = null;
        public static NodaTime.SystemClock Clock;

        public static void Start()
        {
            LogFile   = new File(Path.Combine(Globals.LogsDirectory.Path, "Log.txt"));
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
            LogWriter?.FlushAsync();
        }
    }
}