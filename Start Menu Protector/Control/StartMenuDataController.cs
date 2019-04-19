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

        public static Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> ActiveStartMenuShortcuts { get; set; } = new Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> 
        {
            {StartMenuShortcutsLocation.System, ActiveSystemProgramShortcuts}, 
            {StartMenuShortcutsLocation.User, ActiveUserProgramShortcuts}
        };
        
        public static Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> SavedStartMenuShortcuts { get; set; } = new Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> 
        {
            {StartMenuShortcutsLocation.System, SavedSystemStartMenuShortcuts}, 
            {StartMenuShortcutsLocation.User, SavedUserStartMenuShortcuts}
        };
        
        public abstract Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> StartMenuShortcuts { get; set; }

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
            new Thread(this.LoadCurrentStartMenuData).Start();
        }

        public override Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> StartMenuShortcuts { get; set; } = ActiveStartMenuShortcuts;

        public void LoadCurrentStartMenuData()
        {
            ClearOldActiveStartMenuShortcutsFromDisk();
            LoadCurrentActiveStartMenuShortcutsFromDisk();
        }
        
        private void ClearOldActiveStartMenuShortcutsFromDisk()
        {
            StartMenuShortcuts[StartMenuShortcutsLocation.System].DeleteContents();
            StartMenuShortcuts[StartMenuShortcutsLocation.User].DeleteContents();
        }
        
        private void LoadCurrentActiveStartMenuShortcutsFromDisk()
        {
            Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> startMenuPrograms = SystemStateController.LoadSystemAndUserStartMenuProgramShortcutsFromDisk();
            startMenuPrograms[StartMenuShortcutsLocation.System].Copy(StartMenuShortcuts[StartMenuShortcutsLocation.System]);
            startMenuPrograms[StartMenuShortcutsLocation.User].Copy(StartMenuShortcuts[StartMenuShortcutsLocation.User]);
        }
        
        public override void SaveProgramShortcuts(StartMenuShortcutsLocation location, IEnumerable<FileSystemInfo> startMenuContents)
        {
            EnhancedDirectoryInfo programShortcutsSaveDirectory = SavedStartMenuShortcuts[location];
            
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

        public override Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> StartMenuShortcuts { get; set; } = SavedStartMenuShortcuts;

        public override void SaveProgramShortcuts(StartMenuShortcutsLocation location, IEnumerable<FileSystemInfo> startMenuContents)
        {
            /* Do nothing */
        }
    }

}