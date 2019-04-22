using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using StartMenuProtector.Data;
using StartMenuProtector.Util;
using static StartMenuProtector.Configuration.Globals;


namespace StartMenuProtector.Control 
{
    public abstract class StartMenuDataService
    {
        public SystemStateService SystemStateService { get; set; }

        public abstract Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> StartMenuItemsStorage { protected get; set; }

        public StartMenuDataService(SystemStateService systemStateService)
        {
            this.SystemStateService = systemStateService;
        }

        public abstract Task<ICollection<FileSystemInfo>> GetStartMenuContents(StartMenuShortcutsLocation location);

        protected async Task ClearOldStartMenuShortcutsFromDisk()
        {
            await Task.Run(() =>
            {
                StartMenuItemsStorage[StartMenuShortcutsLocation.System].DeleteContents();
                StartMenuItemsStorage[StartMenuShortcutsLocation.User].DeleteContents();
            });
        }

        public abstract void SaveStartMenuItems(StartMenuShortcutsLocation location, IEnumerable<FileSystemInfo> startMenuItems);
        
        protected abstract Task<Dictionary<StartMenuShortcutsLocation, ICollection<FileSystemInfo>>> LoadStartMenuContentsFromAppDataDiskStorageToMemory();

        public abstract Task HandleRequestToMoveFileSystemItems(EnhancedFileSystemInfo itemRequestingMove, EnhancedFileSystemInfo destinationItem);
    }

    public class ActiveStartMenuDataService : StartMenuDataService
    {
        public override Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> StartMenuItemsStorage { protected get; set; } = new Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> 
        {
            { StartMenuShortcutsLocation.System, ActiveSystemStartMenuShortcuts }, 
            { StartMenuShortcutsLocation.User, ActiveUserStartMenuShortcuts }
        };

        public ActiveStartMenuDataService(SystemStateService systemStateService) 
            : base(systemStateService)
        {
        }
        
        public override async Task<ICollection<FileSystemInfo>> GetStartMenuContents(StartMenuShortcutsLocation location)
        {
            ICollection<FileSystemInfo> startMenuContents = await Task.Run(() =>
            {
                ClearOldStartMenuShortcutsFromDisk().Wait();
                CopyCurrentActiveStartMenuItemsFromOSEnvironmentToAppDataDiskStorage();
                Dictionary<StartMenuShortcutsLocation, ICollection<FileSystemInfo>> startMenuContentsFromAppData = LoadStartMenuContentsFromAppDataDiskStorageToMemory().Result;
                return startMenuContentsFromAppData[location];
            });

            return startMenuContents;
        }
        
        private void CopyCurrentActiveStartMenuItemsFromOSEnvironmentToAppDataDiskStorage()
        {
            Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> startMenuPrograms = SystemStateService.LoadSystemAndUserStartMenuProgramShortcutsFromDisk();
            startMenuPrograms[StartMenuShortcutsLocation.System].Copy(StartMenuItemsStorage[StartMenuShortcutsLocation.System]);
            startMenuPrograms[StartMenuShortcutsLocation.User].Copy(StartMenuItemsStorage[StartMenuShortcutsLocation.User]);
        }
        
        protected sealed override async Task<Dictionary<StartMenuShortcutsLocation, ICollection<FileSystemInfo>>> LoadStartMenuContentsFromAppDataDiskStorageToMemory()
        {
            var startMenuContents = new Dictionary<StartMenuShortcutsLocation, ICollection<FileSystemInfo>>
            {
                { StartMenuShortcutsLocation.System, new List<FileSystemInfo>() },
                { StartMenuShortcutsLocation.User,   new List<FileSystemInfo>() }
            };
            
            await Task.Run(() =>
            {
                startMenuContents[StartMenuShortcutsLocation.System].AddAll(ActiveSystemStartMenuShortcuts.Contents);
                startMenuContents[StartMenuShortcutsLocation.User].AddAll(ActiveUserStartMenuShortcuts.Contents);
            });

            return startMenuContents;
        }
        
        public override void SaveStartMenuItems(StartMenuShortcutsLocation location, IEnumerable<FileSystemInfo> startMenuItems)
        {
            /* Do nothing */
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
        public override Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> StartMenuItemsStorage { protected get; set; } = new Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> 
        {
            {StartMenuShortcutsLocation.System, SavedSystemStartMenuShortcuts}, 
            {StartMenuShortcutsLocation.User, SavedUserStartMenuShortcuts}
        };
        
        public SavedStartMenuDataService(SystemStateService systemStateService) 
            : base(systemStateService)
        {
        }
        
        public override async Task<ICollection<FileSystemInfo>> GetStartMenuContents(StartMenuShortcutsLocation location)
        {
            ICollection<FileSystemInfo> startMenuContents = await Task.Run(() =>
            {
                /* In the the saved data service, unlike the active one, we don't clear the old contents in AppData(since that
                 happens only when saving new contents, and we also don't load from the OS environments start menu state, since 
                 our state is determined entirely by the user */
                Dictionary<StartMenuShortcutsLocation, ICollection<FileSystemInfo>> startMenuContentsFromAppData = LoadStartMenuContentsFromAppDataDiskStorageToMemory().Result;
                return startMenuContentsFromAppData[location];
            });

            return startMenuContents;
        }
        
        protected sealed override async Task<Dictionary<StartMenuShortcutsLocation, ICollection<FileSystemInfo>>> LoadStartMenuContentsFromAppDataDiskStorageToMemory()
        {
            var startMenuContents = new Dictionary<StartMenuShortcutsLocation, ICollection<FileSystemInfo>>
            {
                { StartMenuShortcutsLocation.System, new List<FileSystemInfo>() },
                { StartMenuShortcutsLocation.User,   new List<FileSystemInfo>() }
            };
            
            await Task.Run(() =>
            {
                startMenuContents[StartMenuShortcutsLocation.System].AddAll(SavedSystemStartMenuShortcuts.Contents);
                startMenuContents[StartMenuShortcutsLocation.User].AddAll(SavedUserStartMenuShortcuts.Contents);
            });

            return startMenuContents;
        }

        public override void SaveStartMenuItems(StartMenuShortcutsLocation location, IEnumerable<FileSystemInfo> startMenuItems)
        {
            EnhancedDirectoryInfo programShortcutsSaveDirectory = StartMenuItemsStorage[location];
            
            foreach (var fileSystemItem in startMenuItems)
            {
                EnhancedFileSystemInfo enhancedFileSystemItem = EnhancedFileSystemInfo.Create(fileSystemItem);
                enhancedFileSystemItem.Copy(programShortcutsSaveDirectory);
            }
        }

        public override async Task HandleRequestToMoveFileSystemItems(EnhancedFileSystemInfo itemRequestingMove, EnhancedFileSystemInfo destinationItem)
        {
            /* Do nothing */
            await Task.Run(() => {});
        }
    }

}