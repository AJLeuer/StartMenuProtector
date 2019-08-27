using System.Collections.Generic;
using StartMenuProtector.Data;
using static StartMenuProtector.Configuration.Globals;

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
        readonly object osEnvironmentStartMenuItemsLock = new object();
        public virtual Dictionary<StartMenuShortcutsLocation, Directory> OSEnvironmentStartMenuItems
        {
            get
            {
                lock (osEnvironmentStartMenuItemsLock)
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
            var systemStartMenuItems = new Directory(ProductionStartMenuItemsPath[StartMenuShortcutsLocation.System]);
            var userStartMenuItems = new Directory(ProductionStartMenuItemsPath[StartMenuShortcutsLocation.User]);
        
            var startMenuItems = new Dictionary<StartMenuShortcutsLocation, Directory>
            {
                { StartMenuShortcutsLocation.System, systemStartMenuItems },
                { StartMenuShortcutsLocation.User, userStartMenuItems }
            };

            osEnvironmentStartMenuItems = startMenuItems;
        }
        
    }
}