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
        public ActiveStartMenuDataService ActiveDataService { get; set; }
        public SavedStartMenuDataService SavedDataService { get; set; }
        public SystemStateService SystemStateService { get; set; }
        
        public ObservableCollection<IFileSystemItem> StartMenuContents { get;} = new AsyncObservableCollection<IFileSystemItem>();

        public StartMenuShortcutsLocation StartMenuStartMenuShortcutsLocation { get; set; } = StartMenuShortcutsLocation.System;

        protected StartMenuViewController(ActiveStartMenuDataService activeStartMenuDataService, SavedStartMenuDataService savedStartMenuDataService, SystemStateService systemStateService)
        {
            this.ActiveDataService  = activeStartMenuDataService;
            this.SavedDataService   = savedStartMenuDataService;
            this.SystemStateService = systemStateService;
        }

        public abstract Task UpdateCurrentShortcuts();

        public abstract void SaveCurrentStartMenuItems();

        public abstract Task HandleRequestToMoveStartMenuItem(IStartMenuItem itemRequestingMove, IStartMenuItem destinationItem);

        public abstract void HandleRequestToExcludeStartMenuItem();
    }

    public class ActiveStartMenuViewController : StartMenuViewController
    {
        public enum ContentState
        {
            MirroringOSEnvironment,
            UserChangesPresent
        }

        public ContentState CurrentContentState { get; set; } = ContentState.MirroringOSEnvironment;
        
        public ActiveStartMenuViewController(ActiveStartMenuDataService activeStartMenuDataService, SavedStartMenuDataService savedStartMenuDataService, SystemStateService systemStateService) 
            : base(activeStartMenuDataService, savedStartMenuDataService, systemStateService)
        {
            
        }
        
        public sealed override async Task UpdateCurrentShortcuts()
        {
            ICollection<IFileSystemItem> startMenuContents;
            
            switch (CurrentContentState)
            {
                case ContentState.MirroringOSEnvironment:
                    startMenuContents = (await ActiveDataService.GetStartMenuContents(StartMenuStartMenuShortcutsLocation)).Contents;
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
    
    public class SavedStartMenuViewController : StartMenuViewController
    {
        public SavedStartMenuViewController(ActiveStartMenuDataService activeStartMenuDataService, SavedStartMenuDataService savedStartMenuDataService, SystemStateService systemStateService) 
            : base(activeStartMenuDataService, savedStartMenuDataService, systemStateService)
        {
            
        }

        public sealed override async Task UpdateCurrentShortcuts()
        {
            ICollection<IFileSystemItem> startMenuContent = (await SavedDataService.GetStartMenuContents(StartMenuStartMenuShortcutsLocation)).Contents;
            StartMenuContents.ReplaceAll(startMenuContent);
        }

        public override void SaveCurrentStartMenuItems()
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