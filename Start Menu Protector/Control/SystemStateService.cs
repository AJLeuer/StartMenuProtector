using System.Collections.Generic;
using StartMenuProtector.Data;
using static StartMenuProtector.Configuration.FilePaths;

namespace StartMenuProtector.Control
{

    public enum StartMenuProtectorViewType
    {
        Active,
        Saved,
        Quarantine,
        Excluded
    }
    public enum StartMenuShortcutsLocation
    {
        System, 
        User
    }
    
    public class SystemStateService
    {

        private Dictionary<StartMenuShortcutsLocation, Directory> osEnvironmentStartMenuItems = null;
        public readonly object OSEnvironmentStartMenuItemsLock = new object();
        public virtual Dictionary<StartMenuShortcutsLocation, Directory> OSEnvironmentStartMenuItems
        {
            get
            {
                lock (OSEnvironmentStartMenuItemsLock)
                {
                    if (osEnvironmentStartMenuItems == null)
                    {
                        LoadSystemAndUserStartMenuItemsFromOSEnvironment();
                    }

                    return osEnvironmentStartMenuItems;
                }
            }
        }
        
        private void LoadSystemAndUserStartMenuItemsFromOSEnvironment()
        {
            var systemStartMenuItems = new Directory(StartMenuItemsPath[StartMenuShortcutsLocation.System]);
            var userStartMenuItems = new Directory(StartMenuItemsPath[StartMenuShortcutsLocation.User]);
        
            var startMenuItems = new Dictionary<StartMenuShortcutsLocation, Directory>
            {
                { StartMenuShortcutsLocation.System, systemStartMenuItems },
                { StartMenuShortcutsLocation.User, userStartMenuItems }
            };

            osEnvironmentStartMenuItems = startMenuItems;
        }
        
    }
}