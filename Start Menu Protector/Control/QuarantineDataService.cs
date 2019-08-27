using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StartMenuProtector.Configuration;
using StartMenuProtector.Data;
using static StartMenuProtector.Util.LogManager;

namespace StartMenuProtector.Control
{
    public class QuarantineDataService : StartMenuDataService
    {
        
        public override Dictionary<StartMenuShortcutsLocation, IDirectory> StartMenuItemsStorage { get; set; } = new Dictionary<StartMenuShortcutsLocation, IDirectory> 
        {
            { StartMenuShortcutsLocation.System, FilePaths.QuarantinedSystemStartMenuItems }, 
            { StartMenuShortcutsLocation.User,   FilePaths.QuarantinedUserStartMenuItems   }
        };
        
        protected override Object StartMenuItemsStorageAccessLock { get; } = new Object();
        
        public QuarantineDataService(SystemStateService systemStateService) 
            : base(systemStateService)
        {
        }

        public override void SaveStartMenuItems(IEnumerable<IFileSystemItem> startMenuItems, StartMenuShortcutsLocation location)
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
                        if (itemRequestingMove.Exists)
                        {
                            itemRequestingMove.Move(destinationFolder);
                            Log($"Quarantined the following item: {itemRequestingMove.Path}.");
                        }
                    }
                });
            }
        }
        
        
    }
}