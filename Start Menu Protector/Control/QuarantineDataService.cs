using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StartMenuProtector.Data;

namespace StartMenuProtector.Control
{
    public class QuarantineDataService : StartMenuDataService
    {
        
        public override Dictionary<StartMenuShortcutsLocation, Directory> StartMenuItemsStorage { get; set; }
        protected override Object StartMenuItemsStorageAccessLock { get; } = new Object();
        
        public QuarantineDataService(SystemStateService systemStateService) : base(systemStateService)
        {
        }

        public override void SaveStartMenuItems(IEnumerable<IFileSystemItem> startMenuItems, StartMenuShortcutsLocation location)
        {
            
        }

        public override async Task HandleRequestToMoveFileSystemItems(IFileSystemItem itemRequestingMove, IFileSystemItem destinationItem)
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
}