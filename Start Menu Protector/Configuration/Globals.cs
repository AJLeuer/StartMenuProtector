using System.IO;
using StartMenuProtector.Data;
using Syroot.Windows.IO;

namespace StartMenuProtector.Configuration
{
    public static class Globals
    {
        private const string SystemShortcutsDirectoryName = "System Shortcuts";
        private const string UserShortcutsDirectoryName = "User Shortcuts";

        public static readonly EnhancedDirectoryInfo UserAppData = new EnhancedDirectoryInfo(new KnownFolder(KnownFolderType.RoamingAppData).Path);
        public static readonly EnhancedDirectoryInfo StartMenuProtectorAppData = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(UserAppData.Path, "Start Menu Protector")));
        
        public static readonly EnhancedDirectoryInfo ActiveProgramShortcuts = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(StartMenuProtectorAppData.Path, "Active")));
        public static readonly EnhancedDirectoryInfo ActiveSystemProgramShortcuts = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(ActiveProgramShortcuts.Path, SystemShortcutsDirectoryName)));
        public static readonly EnhancedDirectoryInfo ActiveUserProgramShortcuts = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(ActiveProgramShortcuts.Path, UserShortcutsDirectoryName)));
        
        public static readonly EnhancedDirectoryInfo SavedProgramShortcuts = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(StartMenuProtectorAppData.Path, "Saved")));
        public static readonly EnhancedDirectoryInfo SavedSystemProgramShortcuts = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(SavedProgramShortcuts.Path, SystemShortcutsDirectoryName)));
        public static readonly EnhancedDirectoryInfo SavedUserProgramShortcuts = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(SavedProgramShortcuts.Path, UserShortcutsDirectoryName)));
    }
}