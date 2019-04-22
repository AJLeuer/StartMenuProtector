using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using StartMenuProtector.Util;
using StartMenuProtector.View;

namespace StartMenuProtector.Control 
{
    public abstract class StartMenuViewController 
    {
        public ActiveStartMenuDataService ActiveDataService { get; set; }
        public SavedStartMenuDataService SavedDataService { get; set; }
        public SystemStateService SystemStateService { get; set; }
        
        public readonly ObservableCollection<FileSystemInfo> StartMenuContents = new AsyncObservableCollection<FileSystemInfo>();
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
            ICollection<FileSystemInfo> startMenuContentRetrieval;
            
            switch (CurrentContentState)
            {
                case ContentState.MirroringOSEnvironment:
                    startMenuContentRetrieval = await ActiveDataService.GetStartMenuContents(StartMenuStartMenuShortcutsLocation);
                    break;
                case ContentState.UserChangesPresent:
                    startMenuContentRetrieval = await ActiveDataService.GetStartMenuContentsFromAppDataCache(StartMenuStartMenuShortcutsLocation);
                    break;
                default:
                    throw new InvalidEnumArgumentException("Unhandled type of ContentState");
            }

            StartMenuContents.ReplaceAll(startMenuContentRetrieval);
        }
        
        public override void SaveCurrentStartMenuItems()
        {
            SavedDataService.SaveStartMenuItems(StartMenuStartMenuShortcutsLocation, StartMenuContents);
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
    }
    
    public class SavedStartMenuViewController : StartMenuViewController
    {
        public SavedStartMenuViewController(ActiveStartMenuDataService activeStartMenuDataService, SavedStartMenuDataService savedStartMenuDataService, SystemStateService systemStateService) 
            : base(activeStartMenuDataService, savedStartMenuDataService, systemStateService)
        {
            
        }

        public sealed override async Task UpdateCurrentShortcuts()
        {
            ICollection<FileSystemInfo> startMenuContent = await SavedDataService.GetStartMenuContents(StartMenuStartMenuShortcutsLocation);
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
    }
}