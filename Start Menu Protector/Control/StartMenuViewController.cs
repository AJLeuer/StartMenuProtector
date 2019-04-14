using System.Collections.ObjectModel;
using System.IO;
using Start_Menu_Protector.Data;
using StartMenuProtector.Data;

namespace StartMenuProtector.Control
{
    public class StartMenuViewController
    {
        public Collection<StartMenuShortcutsLocation> Locations { get; set; }
        public ObservableCollection<FileSystemInfo> StartMenuContents = new ObservableCollection<FileSystemInfo>();
        public EnhancedDirectoryInfo CurrentShortcutsDirectory = SystemState.ActiveStartMenuShortcuts[StartMenuShortcutsLocation.System];

        public StartMenuViewController()
        {
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
            CurrentShortcutsDirectory = SystemState.ActiveStartMenuShortcuts[startMenuStartMenuShortcutsLocation];

            PopulateStartMenuTreeView();
        }
    }
}