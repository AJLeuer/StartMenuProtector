using System.Collections.Generic;
using System.Threading.Tasks;
using StartMenuProtector.Data;
using StartMenuProtector.Util;
using StartMenuProtector.View;

namespace StartMenuProtector.Control
{
    public class SavedViewController : StartMenuViewController
    {
        public SavedViewController(ActiveDataService activeDataService, SavedDataService savedDataService, SystemStateService systemStateService) 
            : base(activeDataService, savedDataService, systemStateService)
        {
            
        }

        public sealed override async Task UpdateCurrentShortcuts()
        {
            ICollection<IFileSystemItem> startMenuContent = (await SavedDataService.GetStartMenuContentDirectory(StartMenuStartMenuShortcutsLocation)).Contents;
            StartMenuContents.ReplaceAll(startMenuContent);
        }

        public override void ExecutePrimaryInteractionAction()
        {
            /* Do nothing */
        }
        
        public override void HandleDraggedItemEnteredArea(StartMenuItem target)
        {
            /* Do nothing */
        }

        public override void HandleDraggedItemExitedArea(StartMenuItem target)
        {
            /* Do nothing */
        }
        
        public override async Task HandleRequestToMoveStartMenuItem(IStartMenuItem itemRequestingMove, IStartMenuItem destinationItem)
        {
            /* Do nothing */
            await Task.Run(() => {});
        }
        
        public override void HandleRequestToExcludeStartMenuItem()
        {
            /* Do nothing */
        }
    }
}