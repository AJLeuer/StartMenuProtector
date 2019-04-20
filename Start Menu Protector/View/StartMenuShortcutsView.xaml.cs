using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using StartMenuProtector.Control;
using StartMenuProtector.Util;

namespace StartMenuProtector.View
{
    /// <summary>
    /// Interaction logic for StartMenuView.xaml
    /// </summary>
    public abstract partial class StartMenuShortcutsView : UserControl 
    {
        public ObservableCollection<StartMenuShortcutsLocation> Locations { get; set; } = new ObservableCollection<StartMenuShortcutsLocation> { StartMenuShortcutsLocation.System, StartMenuShortcutsLocation.User };
        public StartMenuViewController Controller { get; set; }

        public ObservableCollection<FileSystemInfo> StartMenuContents
        {
            get { return Controller.StartMenuContents; }
        }

        public StartMenuItemDraggedAndDroppedEventHandler DragAndDropEventHandler
        {
            get { return this.HandleDragAndDropEvent; }
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

        private void HandleDragAndDropEvent(StartMenuItem droppedStartMenuItem, StartMenuItem recipient)
        {
           Controller.HandleRequestToMoveStartMenuItem(itemRequestingMove: droppedStartMenuItem, destinationItem: recipient);
        }
        
    }
}
