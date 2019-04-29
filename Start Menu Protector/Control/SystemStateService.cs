using System;
using System.Collections.Generic;
using StartMenuProtector.Data;

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
            String systemStartMenuItemsPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)}\Programs";
            String userStartMenuItemsPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)}\Programs";
        
            var systemStartMenuItems = new Directory(systemStartMenuItemsPath);
            var userStartMenuItems = new Directory(userStartMenuItemsPath);
        
            var startMenuItems = new Dictionary<StartMenuShortcutsLocation, Directory>
            {
                {StartMenuShortcutsLocation.System, systemStartMenuItems},
                {StartMenuShortcutsLocation.User, userStartMenuItems}
            };

            osEnvironmentStartMenuItems = startMenuItems;
        }
        
    }
}