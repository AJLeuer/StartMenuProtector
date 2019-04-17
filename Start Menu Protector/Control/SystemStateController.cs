using System;
using System.Collections.Generic;
using StartMenuProtector.Data;

namespace StartMenuProtector.Control
{
    public enum StartMenuShortcutsLocation
    {
        System, 
        User
    }
    
    public class SystemStateController
    {
        public Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> LoadSystemAndUserStartMenuProgramShortcutsFromDisk()
        {
            String systemStartMenuShortcutsPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)}\Programs";
            String userStartMenuShortcutsPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)}\Programs";
            
            var systemStartMenuShortcuts = new EnhancedDirectoryInfo(systemStartMenuShortcutsPath);
            var userStartMenuShortcuts = new EnhancedDirectoryInfo(userStartMenuShortcutsPath);
            
            var startMenuProgramShortcuts = new Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo>
            {
                {StartMenuShortcutsLocation.System, systemStartMenuShortcuts},
                {StartMenuShortcutsLocation.User, userStartMenuShortcuts}
            };

            return startMenuProgramShortcuts;
        }
    }
}