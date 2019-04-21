using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using StartMenuProtector.Control;
using StartMenuProtector.Data;

namespace StartMenuProtectorTest.Utility
{
    public class GenericStartMenuDataService : StartMenuDataService
    {
        public GenericStartMenuDataService(SystemStateService systemStateService) 
            : base(systemStateService)
        {
        }

        public override Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> StartMenuShortcuts { get; set; }
        
        public override void SaveStartMenuItems(StartMenuShortcutsLocation location, IEnumerable<FileSystemInfo> startMenuItems)
        {
            throw new NotImplementedException();
        }

        public override Task HandleRequestToMoveFileSystemItems(EnhancedFileSystemInfo itemRequestingMove, EnhancedFileSystemInfo destinationItem)
        {
            throw new NotImplementedException();
        }
    }
}