using System.Collections.ObjectModel;
using System.IO;
using StartMenuProtector.Data;


namespace StartMenuProtector.Control
{
    public class StartMenuViewController
    {
        public StartMenuDataController DataController { get; set; }
        public SystemStateController SystemStateController { get; set; }
        
        public readonly ObservableCollection<FileSystemInfo> StartMenuContents = new ObservableCollection<FileSystemInfo>();
        public StartMenuShortcutsLocation StartMenuStartMenuShortcutsLocation { get; set; } = StartMenuShortcutsLocation.System;

        public EnhancedDirectoryInfo CurrentShortcutsDirectory
        {
            get { return DataController.ProgramShortcuts[StartMenuStartMenuShortcutsLocation]; }
        }

        public StartMenuViewController(StartMenuDataController startMenuDataController, SystemStateController systemStateController)
        {
            this.DataController = startMenuDataController;
            this.SystemStateController = systemStateController;
            PopulateStartMenuTreeView();
        }
        
        private void PopulateStartMenuTreeView()
        {
            StartMenuContents.Clear();
            
            foreach (FileSystemInfo item in CurrentShortcutsDirectory.Contents)
            {
                StartMenuContents.Add(item);         
            }
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
    }
}