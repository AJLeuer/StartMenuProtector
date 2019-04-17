using System.Collections.Generic;
using System.Threading;
using StartMenuProtector.Data;
using static StartMenuProtector.Configuration.Globals;


namespace StartMenuProtector.Control
{
    public class StartMenuDataController
    {
        public SystemStateController SystemStateController { get; set; }

        public virtual Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> ActiveProgramShortcuts { get; set; } = new Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> 
            {
                {StartMenuShortcutsLocation.System, ActiveSystemProgramShortcuts}, 
                {StartMenuShortcutsLocation.User, ActiveUserProgramShortcuts}
            };

        public StartMenuDataController(SystemStateController systemStateController)
        {
            this.SystemStateController = systemStateController;
            new Thread(this.LoadCurrentProgramShortcutsState).Start();
        }
        
        private void LoadCurrentProgramShortcutsState()
        {
            Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> startMenuPrograms = SystemStateController.LoadSystemAndUserStartMenuProgramShortcutsFromDisk();
            startMenuPrograms[StartMenuShortcutsLocation.System].Copy(ActiveProgramShortcuts[StartMenuShortcutsLocation.System].Self);
            startMenuPrograms[StartMenuShortcutsLocation.User].Copy(ActiveProgramShortcuts[StartMenuShortcutsLocation.User].Self);
        }
    }
}