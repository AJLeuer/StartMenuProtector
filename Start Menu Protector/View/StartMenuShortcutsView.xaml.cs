using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StartMenuProtector.Configuration;
using StartMenuProtector.Control;

namespace StartMenuProtector.View
{
    /// <summary>
    /// Interaction logic for StartMenuView.xaml
    /// </summary>
    public abstract partial class StartMenuShortcutsView : UserControl 
    {
        public static Brush OutlineColor { get; set; } = new SolidColorBrush(Config.OutlineColor);

        public static Brush SelectionBackgroundGradient { get; } = new LinearGradientBrush
        {
            EndPoint = new Point(0.5, 1),
            MappingMode = BrushMappingMode.RelativeToBoundingBox,
            StartPoint = new Point(0.5, 0),
            GradientStops =
            {
                new GradientStop { Color = Config.SelectionBackgroundColor },
                new GradientStop { Color = Color.FromArgb(0xFF, 0x48, 0x77, 0xAA)},
                new GradientStop { Color = Color.FromArgb(0xFF, 0x4C, 0x8D, 0xD3)}
            }
        };
        
        public ObservableCollection<StartMenuShortcutsLocation> Locations { get; set; } = new ObservableCollection<StartMenuShortcutsLocation> { StartMenuShortcutsLocation.System, StartMenuShortcutsLocation.User };
        public StartMenuViewController Controller { get; set; }

        public ObservableCollection<FileSystemInfo> StartMenuContents
        {
            get { return Controller.StartMenuContents; }
        }

        public StartMenuShortcutsView()
        {
            InitializeComponent();
            DataContext = this;
        }
        
        private void CurrentShortcutsLocationChanged(object sender, SelectionChangedEventArgs _)
        {
            var selectedLocation = (sender as ListBox)?.SelectedItem;
            StartMenuShortcutsLocation startMenuStartMenuShortcutsLocation = selectedLocation is StartMenuShortcutsLocation ? (StartMenuShortcutsLocation) selectedLocation : StartMenuShortcutsLocation.System;

            Controller.UpdateCurrentShortcuts(startMenuStartMenuShortcutsLocation);
        }

        private void SaveCurrentShortcuts(object sender, RoutedEventArgs _)
        {
            Controller.SaveCurrentShortcuts();
        }
    }
}
