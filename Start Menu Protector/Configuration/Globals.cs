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
        
        public static readonly EnhancedDirectoryInfo ActiveStartMenuShortcuts = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(StartMenuProtectorAppData.Path, "Active")));
        public static readonly EnhancedDirectoryInfo ActiveSystemStartMenuShortcuts = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(ActiveStartMenuShortcuts.Path, SystemShortcutsDirectoryName)));
        public static readonly EnhancedDirectoryInfo ActiveUserStartMenuShortcuts = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(ActiveStartMenuShortcuts.Path, UserShortcutsDirectoryName)));
        
        public static readonly EnhancedDirectoryInfo SavedStartMenuShortcuts = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(StartMenuProtectorAppData.Path, "Saved")));
        public static readonly EnhancedDirectoryInfo SavedSystemStartMenuShortcuts = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(SavedStartMenuShortcuts.Path, SystemShortcutsDirectoryName)));
        public static readonly EnhancedDirectoryInfo SavedUserStartMenuShortcuts = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(SavedStartMenuShortcuts.Path, UserShortcutsDirectoryName)));
    }
}