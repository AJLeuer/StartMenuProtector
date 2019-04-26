using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using StartMenuProtector.Data;
using StartMenuProtector.Util;
using static StartMenuProtector.Configuration.Globals;
using Directory = StartMenuProtector.Data.Directory;


namespace StartMenuProtector.Control 
{
    public abstract class StartMenuDataService
    {
        public SystemStateService SystemStateService { get; set; }

        public abstract Dictionary<StartMenuShortcutsLocation, Directory> StartMenuItemsStorage { protected get; set; }

        public StartMenuDataService(SystemStateService systemStateService)
        {
            this.SystemStateService = systemStateService;
        }

        public virtual async Task<ICollection<FileSystemInfo>> GetStartMenuContents(StartMenuShortcutsLocation location)
        {
            ICollection<FileSystemInfo> startMenuContents = await Task.Run(() =>
            {
                /* In the the saved data service, unlike the active one, we don't clear the old contents in AppData(since that
                 happens only when saving new contents, and we also don't load from the OS environments start menu state, since 
                 our state is determined entirely by the user. So the saved data service uses this default implementation, the 
                 active overrides it */
                Dictionary<StartMenuShortcutsLocation, ICollection<FileSystemInfo>> startMenuContentsFromAppData = LoadStartMenuContentsFromAppDataDiskStorageToMemory().Result;
                return startMenuContentsFromAppData[location];
            });

            return startMenuContents;
        }

        protected async Task ClearAppDataStartMenuItemsFromDisk()
        {
            await Task.Run(() =>
            {
                StartMenuItemsStorage[StartMenuShortcutsLocation.System].DeleteContents();
                StartMenuItemsStorage[StartMenuShortcutsLocation.User].DeleteContents();
            });
        }

        public abstract void SaveStartMenuItems(StartMenuShortcutsLocation location, IEnumerable<FileSystemInfo> startMenuItems);
        
        protected abstract Task<Dictionary<StartMenuShortcutsLocation, ICollection<FileSystemInfo>>> LoadStartMenuContentsFromAppDataDiskStorageToMemory();

        public abstract Task HandleRequestToMoveFileSystemItems(FileSystemItem itemRequestingMove, FileSystemItem destinationItem);

        public void RefreshStartMenuItems(StartMenuShortcutsLocation location)
        {
            Directory startMenuItemsDirectory = StartMenuItemsStorage[location];
            startMenuItemsDirectory.RefreshContents();
        }
        
        protected Directory FindRootStartMenuItemsStorageDirectoryForItem(FileSystemItem item)
        {
            if (StartMenuItemsStorage[StartMenuShortcutsLocation.System].Contains(item))
            {
                return StartMenuItemsStorage[StartMenuShortcutsLocation.System];
            }
            else if (StartMenuItemsStorage[StartMenuShortcutsLocation.User].Contains(item))
            {
                return StartMenuItemsStorage[StartMenuShortcutsLocation.User];
            }
            else
            {
                throw new ArgumentException("File system item not found in Saved Start Menu items");
            }
        }
    }

    public class ActiveStartMenuDataService : StartMenuDataService
    {
        public override Dictionary<StartMenuShortcutsLocation, Directory> StartMenuItemsStorage { protected get; set; } = new Dictionary<StartMenuShortcutsLocation, Directory> 
        {
            { StartMenuShortcutsLocation.System, ActiveSystemStartMenuItems }, 
            { StartMenuShortcutsLocation.User, ActiveUserStartMenuItems }
        };

        public ActiveStartMenuDataService(SystemStateService systemStateService) 
            : base(systemStateService)
        {
        }
        
        /// <summary>
        /// Clears the local cache of the Start Menu's contents held in AppData, replaces it with the contents of the
        /// actual live Start Menu, loads said contents into memory, and returns the results
        /// </summary>
        /// <param name="location"></param>
        /// <returns>The up-to-date contents of the environment's Start Menu</returns>
        public override async Task<ICollection<FileSystemInfo>> GetStartMenuContents(StartMenuShortcutsLocation location)
        {
            ICollection<FileSystemInfo> startMenuContents = await Task.Run(() =>
            {
                ClearAppDataStartMenuItemsFromDisk().Wait();
                CopyCurrentActiveStartMenuItemsFromOSEnvironmentToAppDataDiskStorage();
                Dictionary<StartMenuShortcutsLocation, ICollection<FileSystemInfo>> startMenuContentsFromAppData = LoadStartMenuContentsFromAppDataDiskStorageToMemory().Result;
                return startMenuContentsFromAppData[location];
            });

            return startMenuContents;
        }
        
        public virtual async Task<ICollection<FileSystemInfo>> GetStartMenuContentsFromAppDataCache(StartMenuShortcutsLocation location)
        {
            return await base.GetStartMenuContents(location);
        }
        
        private void CopyCurrentActiveStartMenuItemsFromOSEnvironmentToAppDataDiskStorage()
        {
            Dictionary<StartMenuShortcutsLocation, Directory> startMenuPrograms = SystemStateService.LoadSystemAndUserStartMenuProgramShortcutsFromDisk();
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
                startMenuContents[StartMenuShortcutsLocation.System].AddAll(ActiveSystemStartMenuItems.Contents);
                startMenuContents[StartMenuShortcutsLocation.User].AddAll(ActiveUserStartMenuItems.Contents);
            });

            return startMenuContents;
        }
        
        public override void SaveStartMenuItems(StartMenuShortcutsLocation location, IEnumerable<FileSystemInfo> startMenuItems)
        {
            /* Do nothing */
        }

        public override async Task HandleRequestToMoveFileSystemItems(FileSystemItem itemRequestingMove, FileSystemItem destinationItem)
        {
            if (destinationItem is Directory destinationFolder)
            {
                await Task.Run(() =>
                {
                    itemRequestingMove.Move(destinationFolder);
                    Directory startMenuItemsStorage = FindRootStartMenuItemsStorageDirectoryForItem(destinationFolder);
                    startMenuItemsStorage.RefreshContents();
                });
            }
        }
    }
    
    public class SavedStartMenuDataService : StartMenuDataService
    {
        public override Dictionary<StartMenuShortcutsLocation, Directory> StartMenuItemsStorage { protected get; set; } = new Dictionary<StartMenuShortcutsLocation, Directory> 
        {
            {StartMenuShortcutsLocation.System, SavedSystemStartMenuItems}, 
            {StartMenuShortcutsLocation.User, SavedUserStartMenuItems}
        };
        
        public SavedStartMenuDataService(SystemStateService systemStateService) 
            : base(systemStateService)
        {
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
                startMenuContents[StartMenuShortcutsLocation.System].AddAll(SavedSystemStartMenuItems.Contents);
                startMenuContents[StartMenuShortcutsLocation.User].AddAll(SavedUserStartMenuItems.Contents);
            });

            return startMenuContents;
        }

        public override void SaveStartMenuItems(StartMenuShortcutsLocation location, IEnumerable<FileSystemInfo> startMenuItems)
        {
            Directory startMenuItemsDirectory = StartMenuItemsStorage[location];
            
            foreach (var startMenuItem in startMenuItems)
            {
                FileSystemItem fileSystemItem = FileSystemItem.Create(startMenuItem);
                fileSystemItem.Copy(startMenuItemsDirectory);
            }

            RefreshStartMenuItems(location);
        }

        public override async Task HandleRequestToMoveFileSystemItems(FileSystemItem itemRequestingMove, FileSystemItem destinationItem)
        {
            /* Do nothing */
            await Task.Run(() => {});
        }
    }

}