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
        public StartMenuDataService DataService { get; set; }
        public SystemStateService SystemStateService { get; set; }
        
        public readonly ObservableCollection<FileSystemInfo> StartMenuContents = new AsyncObservableCollection<FileSystemInfo>();
        public StartMenuShortcutsLocation StartMenuStartMenuShortcutsLocation { get; set; } = StartMenuShortcutsLocation.System;

        public EnhancedDirectoryInfo CurrentShortcutsDirectory
        {
            get { return DataService.StartMenuShortcuts[StartMenuStartMenuShortcutsLocation]; }
        }

        public StartMenuViewController(StartMenuDataService startMenuDataService, SystemStateService systemStateService)
        {
            this.DataService = startMenuDataService;
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
                DataService.HandleRequestToMoveFileSystemItems(itemRequestingMove: itemRequestingMove.File, destinationItem: destinationItem.File).Wait();
                PopulateStartMenuTreeView();
            });
        }
    }

    public class ActiveStartMenuViewController : StartMenuViewController
    {
        public ActiveStartMenuViewController(StartMenuDataService startMenuDataService, SystemStateService systemStateService) 
            : base(startMenuDataService, systemStateService)
        {
        }
        
        public override void SaveCurrentShortcuts()
        {
            DataService.SaveProgramShortcuts(StartMenuStartMenuShortcutsLocation, StartMenuContents);
        }
    }
    
    public class SavedStartMenuViewController : StartMenuViewController
    {
        public SavedStartMenuViewController(StartMenuDataService startMenuDataService, SystemStateService systemStateService) 
            : base(startMenuDataService, systemStateService)
        {
        }
        
        public override void SaveCurrentShortcuts()
        {
            /* Do nothing */
        }
    }
}