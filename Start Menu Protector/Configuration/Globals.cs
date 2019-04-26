using System.IO;
using StartMenuProtector.Data;
using Syroot.Windows.IO;

namespace StartMenuProtector.Configuration
{
    public static class Globals
    {
        public const string ApplicationName              = "Start Menu Protector";
        public const string SystemShortcutsDirectoryName = "System Shortcuts";
        public const string UserShortcutsDirectoryName   = "User Shortcuts";

        public static readonly EnhancedDirectoryInfo UserAppData                = new EnhancedDirectoryInfo(new KnownFolder(KnownFolderType.RoamingAppData).Path);
        public static readonly EnhancedDirectoryInfo StartMenuProtectorAppData  = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(UserAppData.Path, ApplicationName)));
        
        public static readonly EnhancedDirectoryInfo ActiveStartMenuItems       = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(StartMenuProtectorAppData.Path, "Active")));
        public static readonly EnhancedDirectoryInfo ActiveSystemStartMenuItems = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(ActiveStartMenuItems.Path, SystemShortcutsDirectoryName)));
        public static readonly EnhancedDirectoryInfo ActiveUserStartMenuItems   = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(ActiveStartMenuItems.Path, UserShortcutsDirectoryName)));
        
        public static readonly EnhancedDirectoryInfo SavedStartMenuItems        = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(StartMenuProtectorAppData.Path, "Saved")));
        public static readonly EnhancedDirectoryInfo SavedSystemStartMenuItems  = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(SavedStartMenuItems.Path, SystemShortcutsDirectoryName)));
        public static readonly EnhancedDirectoryInfo SavedUserStartMenuItems    = new EnhancedDirectoryInfo(Directory.CreateDirectory(Path.Combine(SavedStartMenuItems.Path, UserShortcutsDirectoryName)));
    }
}