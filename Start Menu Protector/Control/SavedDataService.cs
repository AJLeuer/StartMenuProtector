using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StartMenuProtector.Configuration;
using StartMenuProtector.Data;

namespace StartMenuProtector.Control
{
    public class SavedDataService : StartMenuDataService
    {
        public override Dictionary<StartMenuShortcutsLocation, Directory> StartMenuItemsStorage { protected get; set; } = new Dictionary<StartMenuShortcutsLocation, Directory> 
        {
            {StartMenuShortcutsLocation.System, Globals.SavedSystemStartMenuItems}, 
            {StartMenuShortcutsLocation.User, Globals.SavedUserStartMenuItems}
        };

        protected override Object StartMenuItemsStorageAccessLock { get; } = new Object();

        public SavedDataService(SystemStateService systemStateService) 
            : base(systemStateService)
        {
        }

        public override void SaveStartMenuItems(IEnumerable<IFileSystemItem> startMenuItems, StartMenuShortcutsLocation location)
        {
            ClearStartMenuItems(location);
            
            lock (StartMenuItemsStorageAccessLock)
            {
                Directory startMenuItemsDirectory = StartMenuItemsStorage[location];
            
                foreach (var startMenuItem in startMenuItems)
                {
                    startMenuItem.Copy(startMenuItemsDirectory);
                }
            }
            
            RefreshStartMenuItems(location);
        }

        public override async Task HandleRequestToMoveFileSystemItems(IFileSystemItem itemRequestingMove, IFileSystemItem destinationItem)
        {
            /* Do nothing */
            await Task.Run(() => {});
        }
    }
}