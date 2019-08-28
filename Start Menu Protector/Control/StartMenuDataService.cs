using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Optional;
using StartMenuProtector.Configuration;
using StartMenuProtector.Data;
using StartMenuProtector.Util;
using static StartMenuProtector.Util.Util;


namespace StartMenuProtector.Control 
{
    public abstract class StartMenuDataService
    {
        public SystemStateService SystemStateService { get; set; }

        public abstract Dictionary<StartMenuShortcutsLocation, IDirectory> StartMenuItemsStorage { get; set; }
        protected abstract Object StartMenuItemsStorageAccessLock { get; }

        public StartMenuDataService(SystemStateService systemStateService)
        {
            this.SystemStateService = systemStateService;
        }

        public virtual async Task<IDirectory> GetStartMenuContentDirectory(StartMenuShortcutsLocation location)
        {
            IDirectory startMenuContents = await Task.Run(() =>
            {
                /* In the the saved data service, unlike the active one, we don't clear the old contents in AppData(since that
                 happens only when saving new contents, and we also don't load from the OS environments start menu state, since 
                 our state is determined entirely by the user. So the saved data service uses this default implementation, the 
                 active overrides it */
                Dictionary<StartMenuShortcutsLocation, IDirectory> startMenuContentsFromAppData = LoadStartMenuContentsFromAppDataDiskStorageToMemory().Result;
                return startMenuContentsFromAppData[location];
            });

            return startMenuContents;
        }

        public async Task<Option<IDirectory>> GetStartMenuContentDirectoryMainSubdirectory(StartMenuShortcutsLocation location)
        {
            IDirectory directory = await GetStartMenuContentDirectory(location);
            return directory.GetSubdirectory(FilePaths.StartMenuDirectoryName);
        }

        public abstract void SaveStartMenuItems(IEnumerable<IFileSystemItem> startMenuItems, StartMenuShortcutsLocation location);
        protected async Task<Dictionary<StartMenuShortcutsLocation, IDirectory>> LoadStartMenuContentsFromAppDataDiskStorageToMemory()
        {
            
            await Task.Run(RefreshAllStartMenuItems);

            return StartMenuItemsStorage;
        }

        public abstract Task MoveFileSystemItems(IFileSystemItem destinationItem, params IFileSystemItem[] itemsRequestingMove);

        public void RefreshStartMenuItems(StartMenuShortcutsLocation location)
        {
            lock (StartMenuItemsStorageAccessLock)
            {
                IDirectory startMenuItemsDirectory = StartMenuItemsStorage[location];
                startMenuItemsDirectory.RefreshContents();
            }
        }
        
        public void RefreshAllStartMenuItems()
        {
            GetEnumValues<StartMenuShortcutsLocation>().ForEach(RefreshStartMenuItems); 
        }

        protected void ClearStartMenuItems(StartMenuShortcutsLocation location)
        {
            lock (StartMenuItemsStorageAccessLock)
            {
                IDirectory startMenuItemsDirectory = StartMenuItemsStorage[location];
                startMenuItemsDirectory.DeleteContents();
            }
        }
        
        protected async Task ClearAllStartMenuItems()
        {
            await Task.Run(() =>
            {
                ClearStartMenuItems(StartMenuShortcutsLocation.System);
                ClearStartMenuItems(StartMenuShortcutsLocation.User);
            });
        }
        
        protected IDirectory FindRootStartMenuItemsStorageDirectoryForItem(IFileSystemItem item)
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
}