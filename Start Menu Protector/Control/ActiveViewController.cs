using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using StartMenuProtector.Data;
using StartMenuProtector.Util;
using StartMenuProtector.View;

namespace StartMenuProtector.Control
{
    public class ActiveViewController : StartMenuViewController
    {
        public enum ContentState
        {
            MirroringOSEnvironment,
            UserChangesPresent
        }

        public ContentState CurrentContentState { get; set; } = ContentState.MirroringOSEnvironment;
        
        public ActiveViewController(ActiveDataService activeDataService, SavedDataService savedDataService, SystemStateService systemStateService) 
            : base(activeDataService, savedDataService, systemStateService)
        {
            
        }
        
        public sealed override async Task UpdateCurrentShortcuts()
        {
            ICollection<IFileSystemItem> startMenuContents;
            
            switch (CurrentContentState)
            {
                case ContentState.MirroringOSEnvironment:
                    startMenuContents = (await ActiveDataService.GetStartMenuContentDirectory(StartMenuStartMenuShortcutsLocation)).Contents;
                    break;
                case ContentState.UserChangesPresent:
                    startMenuContents = (await ActiveDataService.GetStartMenuContentsFromAppDataCache(StartMenuStartMenuShortcutsLocation)).Contents;
                    break;
                default:
                    throw new InvalidEnumArgumentException("Unhandled type of ContentState");
            }

            StartMenuContents.ReplaceAll(startMenuContents);
        }
        
        public override void ExecutePrimaryInteractionAction()
        {
            SaveCurrentStartMenuItems();
        }

        private void SaveCurrentStartMenuItems()
        {
            SavedDataService.SaveStartMenuItems(StartMenuContents, StartMenuStartMenuShortcutsLocation);
            CurrentContentState = ContentState.MirroringOSEnvironment;
        }

        public override async Task HandleRequestToMoveStartMenuItem(IStartMenuItemView itemViewRequestingMove, IStartMenuItemView destinationItemView)
        {
            CurrentContentState = ContentState.UserChangesPresent;
            
            await Task.Run(() =>
            {
                ActiveDataService.MoveFileSystemItems(destinationItem: destinationItemView.File, itemsRequestingMove: itemViewRequestingMove.File).Wait();
                UpdateCurrentShortcuts().Wait();
            });
        }
        
        public override void HandleRequestToExcludeStartMenuItem()
        {
            CurrentContentState = ContentState.UserChangesPresent;
        }
    }
}