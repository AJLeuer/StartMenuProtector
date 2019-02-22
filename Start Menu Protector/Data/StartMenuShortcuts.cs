using System;
using System.IO;
using StartMenuProtector.IO;

namespace StartMenuProtector.Data
{
    public static class StartMenuShortcuts
    {
        public static EnhancedDirectoryInfo SystemStartMenuShortcuts { get; }
        public static EnhancedDirectoryInfo UserStartMenuShortcuts { get; }

        static StartMenuShortcuts()
        {
            String systemStartMenuShortcutsPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)}\Programs";
            String userStartMenuShortcutsPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)}\Programs";
            
            SystemStartMenuShortcuts = new EnhancedDirectoryInfo(systemStartMenuShortcutsPath);
            UserStartMenuShortcuts = new EnhancedDirectoryInfo(userStartMenuShortcutsPath);
        }
    }
}