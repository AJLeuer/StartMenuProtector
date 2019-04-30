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

        public virtual async Task<Directory> GetStartMenuContents(StartMenuShortcutsLocation location)
        {
            Directory startMenuContents = await Task.Run(() =>
            {
                /* In the the saved data service, unlike the active one, we don't clear the old contents in AppData(since that
                 happens only when saving new contents, and we also don't load from the OS environments start menu state, since 
                 our state is determined entirely by the user. So the saved data service uses this default implementation, the 
                 active overrides it */
                Dictionary<StartMenuShortcutsLocation, Directory> startMenuContentsFromAppData = LoadStartMenuContentsFromAppDataDiskStorageToMemory().Result;
                return startMenuContentsFromAppData[location];
            });

            return startMenuContents;
        }

        public abstract void SaveStartMenuItems(IEnumerable<FileSystemInfo> startMenuItems, StartMenuShortcutsLocation location);
        protected async Task<Dictionary<StartMenuShortcutsLocation, Directory>> LoadStartMenuContentsFromAppDataDiskStorageToMemory()
        {
            await Task.Run(() =>
            {
                StartMenuItemsStorage.Values.ForEach((Directory directory) => { directory.RefreshContents(); });
            });

            return StartMenuItemsStorage;
        }

        public abstract Task HandleRequestToMoveFileSystemItems(FileSystemItem itemRequestingMove, FileSystemItem destinationItem);

        public void RefreshStartMenuItems(StartMenuShortcutsLocation location)
        {
            Directory startMenuItemsDirectory = StartMenuItemsStorage[location];
            startMenuItemsDirectory.RefreshContents();
        }

        protected void ClearStartMenuItems(StartMenuShortcutsLocation location)
        {
            Directory startMenuItemsDirectory = StartMenuItemsStorage[location];
            startMenuItemsDirectory.DeleteContents();
        }
        
        protected async Task ClearAllStartMenuItems()
        {
            await Task.Run(() =>
            {
                ClearStartMenuItems(StartMenuShortcutsLocation.System);
                ClearStartMenuItems(StartMenuShortcutsLocation.User);
            });
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
        public override async Task<Directory> GetStartMenuContents(StartMenuShortcutsLocation location)
        {
            Directory startMenuContents = await Task.Run(() =>
            {
                ClearAllStartMenuItems().Wait();
                CopyCurrentActiveStartMenuItemsFromOSEnvironmentToAppDataDiskStorage();
                Dictionary<StartMenuShortcutsLocation, Directory> startMenuContentsFromAppData = LoadStartMenuContentsFromAppDataDiskStorageToMemory().Result;
                return startMenuContentsFromAppData[location];
            });

            return startMenuContents;
        }
        
        public virtual async Task<Directory> GetStartMenuContentsFromAppDataCache(StartMenuShortcutsLocation location)
        {
            return await base.GetStartMenuContents(location);
        }
        
        private void CopyCurrentActiveStartMenuItemsFromOSEnvironmentToAppDataDiskStorage()
        {
            Dictionary<StartMenuShortcutsLocation, Directory> startMenuContents = SystemStateService.OSEnvironmentStartMenuItems;
            startMenuContents[StartMenuShortcutsLocation.System].Copy(StartMenuItemsStorage[StartMenuShortcutsLocation.System]);
            startMenuContents[StartMenuShortcutsLocation.User].Copy(StartMenuItemsStorage[StartMenuShortcutsLocation.User]);
        }

        public override void SaveStartMenuItems(IEnumerable<FileSystemInfo> startMenuItems, StartMenuShortcutsLocation location)
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

        public override void SaveStartMenuItems(IEnumerable<FileSystemInfo> startMenuItems, StartMenuShortcutsLocation location)
        {
            ClearStartMenuItems(location);
            
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