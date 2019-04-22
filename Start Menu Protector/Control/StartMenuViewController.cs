using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public abstract void SaveCurrentShortcuts();

        public async void HandleRequestToMoveStartMenuItem(StartMenuItem itemRequestingMove, StartMenuItem destinationItem)
        {
            await Task.Run(() =>
            {
                ActiveDataService.HandleRequestToMoveFileSystemItems(itemRequestingMove: itemRequestingMove.File, destinationItem: destinationItem.File).Wait();
                UpdateCurrentShortcuts();
            });
        }
    }

    public class ActiveStartMenuViewController : StartMenuViewController
    {
        public ActiveStartMenuViewController(ActiveStartMenuDataService activeStartMenuDataService, SavedStartMenuDataService savedStartMenuDataService, SystemStateService systemStateService) 
            : base(activeStartMenuDataService, savedStartMenuDataService, systemStateService)
        {
            
        }
        
        public sealed override async Task UpdateCurrentShortcuts()
        {
            ICollection<FileSystemInfo> startMenuContentRetrieval = await ActiveDataService.GetStartMenuContents(StartMenuStartMenuShortcutsLocation);
            StartMenuContents.ReplaceAll(startMenuContentRetrieval);
        }
        
        public override void SaveCurrentShortcuts()
        {
            SavedDataService.SaveStartMenuItems(StartMenuStartMenuShortcutsLocation, StartMenuContents);
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

        public override void SaveCurrentShortcuts()
        {
            /* Do nothing */
        }
    }
}