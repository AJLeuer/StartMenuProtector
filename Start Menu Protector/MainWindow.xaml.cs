using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StartMenuProtector;
using StartMenuProtector.Data;
using StartMenuProtector.IO;

namespace StartMenuProtector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<ShortcutLocation> Locations { get; set; } = new ObservableCollection<ShortcutLocation> { ShortcutLocation.System, ShortcutLocation.User };

        public ObservableCollection<FileSystemInfo> StartMenuContents { get; set; } = new ObservableCollection<FileSystemInfo>();

        public EnhancedDirectoryInfo CurrentShortcutsDirectory = StartMenuShortcuts.SystemStartMenuShortcuts;
        
        public MainWindow()
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
