using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StartMenuProtector.Configuration;
using StartMenuProtector.Control;
using StartMenuProtector.Data;

namespace StartMenuProtector.View
{
    /// <summary>
    /// Interaction logic for StartMenuView.xaml
    /// </summary>
    public partial class StartMenuView : UserControl 
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
                
        private (StartMenuItem, Border) selectedStartMenuItem;
        private (StartMenuItem, Border) SelectedStartMenuItem 
        {
            get { return selectedStartMenuItem; }
            set
            {
                selectedStartMenuItem.Item1?.Deselected();
                
                if (selectedStartMenuItem.Item2 != null)
                {
                    selectedStartMenuItem.Item2.BorderBrush = OutlineColor;
                }
                
                selectedStartMenuItem = value;
                selectedStartMenuItem.Item1?.Selected();
                
                if (selectedStartMenuItem.Item2 != null)
                {
                    selectedStartMenuItem.Item2.BorderBrush = SelectionBackgroundGradient;
                }
            }
        }
        public StartMenuViewController Controller { get; set; }

        public ObservableCollection<FileSystemInfo> StartMenuContents
        {
            get { return Controller.StartMenuContents; }
        }

        public StartMenuView()
        {
            InitializeComponent();
            DataContext = this;
        }
        
        private void CurrentShortcutsLocationChanged(object sender, SelectionChangedEventArgs @event)
        {
            var selectedLocation = (sender as ListBox)?.SelectedItem;
            StartMenuShortcutsLocation startMenuStartMenuShortcutsLocation = selectedLocation is StartMenuShortcutsLocation ? (StartMenuShortcutsLocation) selectedLocation : StartMenuShortcutsLocation.System;

            Controller.UpdateCurrentShortcuts(startMenuStartMenuShortcutsLocation);
        }

        private void UpdateSelectedItem(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var newSelectedStartMenuItem = this.SelectedStartMenuItem;
            switch (sender)
            {
                case StartMenuItem startMenuItem:
                    newSelectedStartMenuItem.Item1 = startMenuItem;
                    break;
                case Border border:
                    newSelectedStartMenuItem.Item2 = border;
                    break;
            }
            SelectedStartMenuItem = newSelectedStartMenuItem;
        }
    }
}
