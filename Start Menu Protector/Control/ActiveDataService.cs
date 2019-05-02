using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StartMenuProtector.Configuration;
using StartMenuProtector.Data;

namespace StartMenuProtector.Control
{
    public class ActiveDataService : StartMenuDataService
    {
        public override Dictionary<StartMenuShortcutsLocation, IDirectory> StartMenuItemsStorage { get; set; } = new Dictionary<StartMenuShortcutsLocation, IDirectory> 
        {
            { StartMenuShortcutsLocation.System, Globals.ActiveSystemStartMenuItems }, 
            { StartMenuShortcutsLocation.User,   Globals.ActiveUserStartMenuItems   }
        };
        
        protected override Object StartMenuItemsStorageAccessLock { get; } = new Object();

        public ActiveDataService(SystemStateService systemStateService) 
            : base(systemStateService)
        {
        }

        /// <summary>
        /// Clears the local cache of the Start Menu's contents held in AppData, replaces it with the contents of the
        /// actual live Start Menu, loads said contents into memory, and returns the results
        /// </summary>
        /// <param name="location"></param>
        /// <returns>The up-to-date contents of the environment's Start Menu</returns>
        public override async Task<IDirectory> GetStartMenuContentDirectory(StartMenuShortcutsLocation location)
        {
            IDirectory startMenuContents = await Task.Run(() =>
            {
                ClearAllStartMenuItems().Wait();
                CopyCurrentActiveStartMenuItemsFromOSEnvironmentToAppDataDiskStorage();
                Dictionary<StartMenuShortcutsLocation, IDirectory> startMenuContentsFromAppData = LoadStartMenuContentsFromAppDataDiskStorageToMemory().Result;
                return startMenuContentsFromAppData[location];
            });

            return startMenuContents;
        }
        
        public virtual async Task<IDirectory> GetStartMenuContentsFromAppDataCache(StartMenuShortcutsLocation location)
        {
            return await base.GetStartMenuContentDirectory(location);
        }
        
        private void CopyCurrentActiveStartMenuItemsFromOSEnvironmentToAppDataDiskStorage()
        {
            Dictionary<StartMenuShortcutsLocation, Directory> startMenuContents = SystemStateService.OSEnvironmentStartMenuItems;
            
            lock (StartMenuItemsStorageAccessLock)
            {
                startMenuContents[StartMenuShortcutsLocation.System].Copy(StartMenuItemsStorage[StartMenuShortcutsLocation.System]);
                startMenuContents[StartMenuShortcutsLocation.User].Copy(StartMenuItemsStorage[StartMenuShortcutsLocation.User]);
            }
        }

        public override void SaveStartMenuItems(IEnumerable<IFileSystemItem> fileSystemItems, StartMenuShortcutsLocation location)
        {
            /* Do nothing */
        }

        public override async Task MoveFileSystemItems(IFileSystemItem destinationItem, params IFileSystemItem[] itemsRequestingMove)
        {
            if (destinationItem is Directory destinationFolder)
            {
                await Task.Run(() =>
                {
                    foreach (IFileSystemItem itemRequestingMove in itemsRequestingMove)
                    {
                        itemRequestingMove.Move(destinationFolder);
                        IDirectory startMenuItemsStorage = FindRootStartMenuItemsStorageDirectoryForItem(destinationFolder);
                        startMenuItemsStorage.RefreshContents();
                    }
                });
            }
        }
        
        
    }
}