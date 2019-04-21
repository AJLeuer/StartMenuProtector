using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using StartMenuProtector.Data;
using static StartMenuProtector.Configuration.Globals;


namespace StartMenuProtector.Control
{
    public abstract class StartMenuDataService
    {
        public SystemStateService SystemStateService { get; set; }

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
        
        public StartMenuDataService(SystemStateService systemStateService)
        {
            this.SystemStateService = systemStateService;
        }
        
        protected void ClearOldStartMenuShortcutsFromDisk()
        {
            StartMenuShortcuts[StartMenuShortcutsLocation.System].DeleteContents();
            StartMenuShortcuts[StartMenuShortcutsLocation.User].DeleteContents();
        }

        public abstract void SaveProgramShortcuts(StartMenuShortcutsLocation location, IEnumerable<FileSystemInfo> startMenuContents);

        public abstract Task HandleRequestToMoveFileSystemItems(EnhancedFileSystemInfo itemRequestingMove, EnhancedFileSystemInfo destinationItem);
    }

    public class ActiveStartMenuDataService : StartMenuDataService
    {
        public override Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> StartMenuShortcuts { get; set; } = ActiveStartMenuShortcuts;

        public ActiveStartMenuDataService(SystemStateService systemStateService) 
            : base(systemStateService)
        {
            #pragma warning disable 4014
            LoadCurrentStartMenuData();
            #pragma warning restore 4014
        }

        public async Task LoadCurrentStartMenuData()
        {
            await Task.Run(() =>
            {
                ClearOldStartMenuShortcutsFromDisk();
                LoadCurrentActiveStartMenuShortcutsFromDisk();
            });
        }
        
        private void LoadCurrentActiveStartMenuShortcutsFromDisk()
        {
            Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> startMenuPrograms = SystemStateService.LoadSystemAndUserStartMenuProgramShortcutsFromDisk();
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

        public override async Task HandleRequestToMoveFileSystemItems(EnhancedFileSystemInfo itemRequestingMove, EnhancedFileSystemInfo destinationItem)
        {
            if (destinationItem is EnhancedDirectoryInfo destinationFolder)
            {
                await Task.Run(() =>
                {
                    itemRequestingMove.Move(destinationFolder);
                });
            }
        }
    }
    
    public class SavedStartMenuDataService : StartMenuDataService
    {
        public SavedStartMenuDataService(SystemStateService systemStateService) 
            : base(systemStateService)
        {
            
        }

        public override Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> StartMenuShortcuts { get; set; } = SavedStartMenuShortcuts;

        public override void SaveProgramShortcuts(StartMenuShortcutsLocation location, IEnumerable<FileSystemInfo> startMenuContents)
        {
            /* Do nothing */
        }
        
        public override async Task HandleRequestToMoveFileSystemItems(EnhancedFileSystemInfo itemRequestingMove, EnhancedFileSystemInfo destinationItem)
        {
            /* Do nothing */
            await Task.Run(() => {});
        }
    }

}