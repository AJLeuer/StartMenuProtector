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
        public EnhancedDirectoryInfo CurrentShortcutsDirectory { get; set; }

        public StartMenuViewController(StartMenuDataController startMenuDataController, SystemStateController systemStateController)
        {
            this.DataController = startMenuDataController;
            this.SystemStateController = systemStateController;
            CurrentShortcutsDirectory = DataController.ActiveProgramShortcuts[StartMenuShortcutsLocation.System];
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
            CurrentShortcutsDirectory = DataController.ActiveProgramShortcuts[startMenuStartMenuShortcutsLocation];
            PopulateStartMenuTreeView();
        }
        

    }
}