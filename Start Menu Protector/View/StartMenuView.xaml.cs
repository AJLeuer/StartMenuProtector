using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using StartMenuProtector.Data;
using StartMenuProtector.IO;

namespace StartMenuProtector.View
{
    /// <summary>
    /// Interaction logic for StartMenuView.xaml
    /// </summary>
    public partial class StartMenuView : UserControl 
    {
        public ObservableCollection<ShortcutLocation> Locations { get; set; } = new ObservableCollection<ShortcutLocation> { ShortcutLocation.System, ShortcutLocation.User };

        public ObservableCollection<FileSystemInfo> StartMenuContents { get; set; } = new ObservableCollection<FileSystemInfo>();

        public EnhancedDirectoryInfo CurrentShortcutsDirectory = StartMenuShortcuts.SystemStartMenuShortcuts;
        
        public StartMenuView()
        {
            InitializeComponent();
            this.DataContext = this;
            PopulateStartMenuTreeView();
        }
        
        private void CurrentShortcutsLocationChanged(object sender, SelectionChangedEventArgs @event)
        {
            var selectedLocation = (sender as ListBox)?.SelectedItem;
            
            switch (selectedLocation is ShortcutLocation ? (ShortcutLocation) selectedLocation : ShortcutLocation.System)
            {
                case ShortcutLocation.System:
                    CurrentShortcutsDirectory = StartMenuShortcuts.SystemStartMenuShortcuts;
                    break;
                case ShortcutLocation.User:
                    CurrentShortcutsDirectory = StartMenuShortcuts.UserStartMenuShortcuts;
                    break;
            }
            
            PopulateStartMenuTreeView();
        }

        private void PopulateStartMenuTreeView()
        {
            StartMenuContents.Clear();
            
            foreach (FileSystemInfo file in CurrentShortcutsDirectory.Files)
            {
                StartMenuContents.Add(file);         
            }
            
            foreach (FileSystemInfo directory in CurrentShortcutsDirectory.Directories)
            {
                StartMenuContents.Add(directory);         
            }
        }
    }
    
    public enum ShortcutLocation
    {
        System, 
        User
    }
}
