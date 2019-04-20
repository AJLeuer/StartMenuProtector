using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using StartMenuProtector.Data;
using StartMenuProtector.Util;
using StartMenuProtector.View;


namespace StartMenuProtector.Control
{
    public class StartMenuViewController
    {
        public StartMenuDataController DataController { get; set; }
        public SystemStateController SystemStateController { get; set; }
        
        public readonly ObservableCollection<FileSystemInfo> StartMenuContents = new AsyncObservableCollection<FileSystemInfo>();
        public StartMenuShortcutsLocation StartMenuStartMenuShortcutsLocation { get; set; } = StartMenuShortcutsLocation.System;

        public EnhancedDirectoryInfo CurrentShortcutsDirectory
        {
            get { return DataController.StartMenuShortcuts[StartMenuStartMenuShortcutsLocation]; }
        }

        public StartMenuViewController(StartMenuDataController startMenuDataController, SystemStateController systemStateController)
        {
            this.DataController = startMenuDataController;
            this.SystemStateController = systemStateController;
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

        public void SaveCurrentShortcuts()
        {
            DataController.SaveProgramShortcuts(StartMenuStartMenuShortcutsLocation, StartMenuContents);
        }

        public async void HandleRequestToMoveStartMenuItem(StartMenuItem itemRequestingMove, StartMenuItem destinationItem)
        {
            await Task.Run(() =>
            {
                DataController.HandleRequestToMoveFileSystemItems(itemRequestingMove: itemRequestingMove.File, destinationItem: destinationItem.File).Wait();
                PopulateStartMenuTreeView();
            });
        }
    }
}