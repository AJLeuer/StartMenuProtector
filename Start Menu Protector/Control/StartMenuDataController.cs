using System.Collections.Generic;
using System.IO;
using System.Threading;
using StartMenuProtector.Data;
using static StartMenuProtector.Configuration.Globals;


namespace StartMenuProtector.Control
{
    public abstract class StartMenuDataController
    {
        public SystemStateController SystemStateController { get; set; }

        protected static Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> ActiveProgramShortcuts { get;} = new Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> 
        {
            {StartMenuShortcutsLocation.System, ActiveSystemProgramShortcuts}, 
            {StartMenuShortcutsLocation.User, ActiveUserProgramShortcuts}
        };
        
        public static Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> SavedProgramShortcuts { get; set; } = new Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> 
        {
            {StartMenuShortcutsLocation.System, SavedSystemProgramShortcuts}, 
            {StartMenuShortcutsLocation.User, SavedUserProgramShortcuts}
        };
        
        public abstract Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> ProgramShortcuts { get; set; }

        public StartMenuDataController(SystemStateController systemStateController)
        {
            this.SystemStateController = systemStateController;
        }

        public abstract void SaveProgramShortcuts(StartMenuShortcutsLocation location, IEnumerable<FileSystemInfo> startMenuContents);
    }

    public class ActiveStartMenuDataController : StartMenuDataController
    {
        public ActiveStartMenuDataController(SystemStateController systemStateController) 
            : base(systemStateController)
        {
            new Thread(this.LoadActiveProgramShortcutsState).Start();
        }

        public override Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> ProgramShortcuts { get; set; } = ActiveProgramShortcuts;
        
        private void LoadActiveProgramShortcutsState()
        {
            Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> startMenuPrograms = SystemStateController.LoadSystemAndUserStartMenuProgramShortcutsFromDisk();
            startMenuPrograms[StartMenuShortcutsLocation.System].Copy(ProgramShortcuts[StartMenuShortcutsLocation.System]);
            startMenuPrograms[StartMenuShortcutsLocation.User].Copy(ProgramShortcuts[StartMenuShortcutsLocation.User]);
        }
        
        public override void SaveProgramShortcuts(StartMenuShortcutsLocation location, IEnumerable<FileSystemInfo> startMenuContents)
        {
            EnhancedDirectoryInfo programShortcutsSaveDirectory = SavedProgramShortcuts[location];
            
            foreach (var fileSystemItem in startMenuContents)
            {
                EnhancedFileSystemInfo enhancedFileSystemItem = EnhancedFileSystemInfo.Create(fileSystemItem);
                enhancedFileSystemItem.Copy(programShortcutsSaveDirectory);
            }
        }
        
    }
    
    public class SavedStartMenuDataController : StartMenuDataController
    {
        public SavedStartMenuDataController(SystemStateController systemStateController) 
            : base(systemStateController)
        {
            
        }

        public override Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> ProgramShortcuts { get; set; } = SavedProgramShortcuts;

        public override void SaveProgramShortcuts(StartMenuShortcutsLocation location, IEnumerable<FileSystemInfo> startMenuContents)
        {
            /* Do nothing */
        }
    }

}