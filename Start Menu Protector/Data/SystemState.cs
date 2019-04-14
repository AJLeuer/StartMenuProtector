using System;
using System.Collections.Generic;

namespace StartMenuProtector.Data
{
    public enum StartMenuShortcutsLocation
    {
        System, 
        User
    }
    
    public static class SystemState
    {
        public static Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> ActiveStartMenuShortcuts;

        static SystemState()
        {
            String systemStartMenuShortcutsPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)}\Programs";
            String userStartMenuShortcutsPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)}\Programs";
            
            var systemStartMenuShortcuts = new EnhancedDirectoryInfo(systemStartMenuShortcutsPath);
            var userStartMenuShortcuts = new EnhancedDirectoryInfo(userStartMenuShortcutsPath);
            
            ActiveStartMenuShortcuts = new Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo>
            {
                {StartMenuShortcutsLocation.System, systemStartMenuShortcuts},
                {StartMenuShortcutsLocation.User, userStartMenuShortcuts}
            };
        }


    }
}