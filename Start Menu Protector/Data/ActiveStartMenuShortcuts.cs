using System;
using StartMenuProtector.IO;

namespace StartMenuProtector.Data
{
    public static class ActiveStartMenuShortcuts
    {
        public static EnhancedDirectoryInfo SystemStartMenuShortcuts { get; }
        public static EnhancedDirectoryInfo UserStartMenuShortcuts { get; }

        static ActiveStartMenuShortcuts()
        {
            String systemStartMenuShortcutsPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)}\Programs";
            String userStartMenuShortcutsPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)}\Programs";
            
            SystemStartMenuShortcuts = new EnhancedDirectoryInfo(systemStartMenuShortcutsPath);
            UserStartMenuShortcuts = new EnhancedDirectoryInfo(userStartMenuShortcutsPath);
        }


    }
}