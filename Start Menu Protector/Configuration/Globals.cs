using System.IO;
using StartMenuProtector.Data;
using Syroot.Windows.IO;

namespace StartMenuProtector.Configuration
{
    public static class Globals
    {
        public static readonly EnhancedDirectoryInfo UserAppData = new EnhancedDirectoryInfo(new KnownFolder(KnownFolderType.RoamingAppData).Path);
        public static readonly EnhancedDirectoryInfo StartMenuProtectorAppData = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(UserAppData.Path, "Start Menu Protector")));
        public static readonly EnhancedDirectoryInfo ActiveSystemProgramShortcuts = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(StartMenuProtectorAppData.Path, "System Shortcuts")));
        public static readonly EnhancedDirectoryInfo ActiveUserProgramShortcuts = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(StartMenuProtectorAppData.Path, "User Shortcuts")));
    }
}