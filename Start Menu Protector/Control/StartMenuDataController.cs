using System.Collections.Generic;
using System.Threading;
using StartMenuProtector.Data;
using static StartMenuProtector.Configuration.Globals;


namespace StartMenuProtector.Control
{
    public abstract class StartMenuDataController
    {
        public SystemStateController SystemStateController { get; set; }

        public abstract Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> ProgramShortcuts { get; set; }

        public StartMenuDataController(SystemStateController systemStateController)
        {
            this.SystemStateController = systemStateController;
        }
    }

    public class ActiveStartMenuDataController : StartMenuDataController
    {
        public ActiveStartMenuDataController(SystemStateController systemStateController) 
            : base(systemStateController)
        {
            new Thread(this.LoadActiveProgramShortcutsState).Start();
        }

        public override Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> ProgramShortcuts { get; set; } = new Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> 
        {
            {StartMenuShortcutsLocation.System, ActiveSystemProgramShortcuts}, 
            {StartMenuShortcutsLocation.User, ActiveUserProgramShortcuts}
        };
        
        private void LoadActiveProgramShortcutsState()
        {
            Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> startMenuPrograms = SystemStateController.LoadSystemAndUserStartMenuProgramShortcutsFromDisk();
            startMenuPrograms[StartMenuShortcutsLocation.System].Copy(ProgramShortcuts[StartMenuShortcutsLocation.System].Self);
            startMenuPrograms[StartMenuShortcutsLocation.User].Copy(ProgramShortcuts[StartMenuShortcutsLocation.User].Self);
        }
    }
    
    public class SavedStartMenuDataController : StartMenuDataController
    {
        public SavedStartMenuDataController(SystemStateController systemStateController) 
            : base(systemStateController)
        {
            
        }

        public override Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> ProgramShortcuts { get; set; } = new Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> 
        {
            {StartMenuShortcutsLocation.System, SavedSystemProgramShortcuts}, 
            {StartMenuShortcutsLocation.User, SavedUserProgramShortcuts}
        };

    }

}