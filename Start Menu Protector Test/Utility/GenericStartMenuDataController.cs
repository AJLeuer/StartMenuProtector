using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using StartMenuProtector.Control;
using StartMenuProtector.Data;

namespace StartMenuProtectorTest.Utility
{
    public class GenericStartMenuDataController : StartMenuDataController
    {
        public GenericStartMenuDataController(SystemStateController systemStateController) 
            : base(systemStateController)
        {
        }

        public override Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> StartMenuShortcuts { get; set; }
        
        public override void SaveProgramShortcuts(StartMenuShortcutsLocation location, IEnumerable<FileSystemInfo> startMenuContents)
        {
            throw new System.NotImplementedException();
        }

        public override Task HandleRequestToMoveFileSystemItems(EnhancedFileSystemInfo itemRequestingMove, EnhancedFileSystemInfo destinationItem)
        {
            throw new System.NotImplementedException();
        }
    }
}