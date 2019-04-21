using System.Collections.ObjectModel;
using System.IO;
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
        
        public readonly ObservableCollection<FileSystemInfo> StartMenuContents = new AsyncObservableCollection<FileSystemInfo>();
        public StartMenuShortcutsLocation StartMenuStartMenuShortcutsLocation { get; set; } = StartMenuShortcutsLocation.System;

        public abstract EnhancedDirectoryInfo CurrentShortcutsDirectory { get; }

        protected StartMenuViewController(ActiveStartMenuDataService activeStartMenuDataService, SavedStartMenuDataService savedStartMenuDataService, SystemStateService systemStateService)
        {
            this.ActiveDataService  = activeStartMenuDataService;
            this.SavedDataService   = savedStartMenuDataService;
            this.SystemStateService = systemStateService;
            PopulateStartMenuTreeView();
        }
        
        private void PopulateStartMenuTreeView()
        {
            Task.Run(() =>
            {
                StartMenuContents.Clear();
            
                foreach (EnhancedFileSystemInfo item in CurrentShortcutsDirectory.Contents)
                {
                    StartMenuContents.Add(item);         
                }
            });
        }

        public void UpdateCurrentShortcuts(StartMenuShortcutsLocation startMenuStartMenuShortcutsLocation)
        {
            StartMenuStartMenuShortcutsLocation = startMenuStartMenuShortcutsLocation;
            PopulateStartMenuTreeView();
        }

        public abstract void SaveCurrentShortcuts();

        public async void HandleRequestToMoveStartMenuItem(StartMenuItem itemRequestingMove, StartMenuItem destinationItem)
        {
            await Task.Run(() =>
            {
                ActiveDataService.HandleRequestToMoveFileSystemItems(itemRequestingMove: itemRequestingMove.File, destinationItem: destinationItem.File).Wait();
                PopulateStartMenuTreeView();
            });
        }
    }

    public class ActiveStartMenuViewController : StartMenuViewController
    {
        public override EnhancedDirectoryInfo CurrentShortcutsDirectory
        {
            get { return ActiveDataService.StartMenuShortcuts[StartMenuStartMenuShortcutsLocation]; }
        }
        
        public ActiveStartMenuViewController(ActiveStartMenuDataService activeStartMenuDataService, SavedStartMenuDataService savedStartMenuDataService, SystemStateService systemStateService) 
            : base(activeStartMenuDataService, savedStartMenuDataService, systemStateService)
        {
        }
        
        public override void SaveCurrentShortcuts()
        {
            SavedDataService.SaveStartMenuItems(StartMenuStartMenuShortcutsLocation, StartMenuContents);
        }
    }
    
    public class SavedStartMenuViewController : StartMenuViewController
    {
        public override EnhancedDirectoryInfo CurrentShortcutsDirectory
        {
            get { return SavedDataService.StartMenuShortcuts[StartMenuStartMenuShortcutsLocation]; }
        }
        
        public SavedStartMenuViewController(ActiveStartMenuDataService activeStartMenuDataService, SavedStartMenuDataService savedStartMenuDataService, SystemStateService systemStateService) 
            : base(activeStartMenuDataService, savedStartMenuDataService, systemStateService)
        {
        }
        
        public override void SaveCurrentShortcuts()
        {
            /* Do nothing */
        }
    }
}