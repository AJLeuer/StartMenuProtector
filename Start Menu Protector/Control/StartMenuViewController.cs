using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using StartMenuProtector.Data;
using StartMenuProtector.Util;
using StartMenuProtector.View;

namespace StartMenuProtector.Control 
{
    public abstract class StartMenuViewController 
    {
        public ActiveDataService ActiveDataService { get; set; }
        public SavedDataService SavedDataService { get; set; }
        public SystemStateService SystemStateService { get; set; }
        
        public ObservableCollection<IFileSystemItem> StartMenuContents { get;} = new AsyncObservableCollection<IFileSystemItem>();

        public StartMenuShortcutsLocation StartMenuStartMenuShortcutsLocation { get; set; } = StartMenuShortcutsLocation.System;

        protected StartMenuViewController(ActiveDataService activeDataService, SavedDataService savedDataService, SystemStateService systemStateService)
        {
            this.ActiveDataService  = activeDataService;
            this.SavedDataService   = savedDataService;
            this.SystemStateService = systemStateService;
        }

        public abstract Task UpdateCurrentShortcuts();

        public abstract void SaveCurrentStartMenuItems();

        public virtual void HandleDraggedItemEnteredArea(StartMenuItem target)
        {
            if (target.File.IsOfType<IDirectory>())
            {
                target.CandidateForDrop = true;
            }
        }

        public virtual void HandleDraggedItemExitedArea(StartMenuItem target)
        {
            if (target.File.IsOfType<IDirectory>())
            {
                target.CandidateForDrop = false;
            }
        }

        public abstract Task HandleRequestToMoveStartMenuItem(IStartMenuItem itemRequestingMove, IStartMenuItem destinationItem);

        public abstract void HandleRequestToExcludeStartMenuItem();
    }

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
        
        public override void SaveCurrentStartMenuItems()
        {
            SavedDataService.SaveStartMenuItems(StartMenuContents, StartMenuStartMenuShortcutsLocation);
            CurrentContentState = ContentState.MirroringOSEnvironment;
        }
        
        public override async Task HandleRequestToMoveStartMenuItem(IStartMenuItem itemRequestingMove, IStartMenuItem destinationItem)
        {
            CurrentContentState = ContentState.UserChangesPresent;
            
            await Task.Run(() =>
            {
                ActiveDataService.HandleRequestToMoveFileSystemItems(itemRequestingMove: itemRequestingMove.File, destinationItem: destinationItem.File).Wait();
                UpdateCurrentShortcuts().Wait();
            });
        }
        
        public override void HandleRequestToExcludeStartMenuItem()
        {
            CurrentContentState = ContentState.UserChangesPresent;
        }
    }
    
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

        public override void SaveCurrentStartMenuItems()
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