using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using StartMenuProtector.Control;
using StartMenuProtector.Data;
using StartMenuProtector.Util;

namespace StartMenuProtector.View
{
    /// <summary>
    /// Interaction logic for StartMenuView.xaml
    /// </summary>
    public abstract partial class StartMenuShortcutsView : UserControl 
    {
        public ObservableCollection<StartMenuShortcutsLocation> Locations { get; set; } = new AsyncObservableCollection<StartMenuShortcutsLocation> { StartMenuShortcutsLocation.System, StartMenuShortcutsLocation.User };
        public StartMenuViewController Controller { get; set; }

        public ObservableCollection<IFileSystemItem> StartMenuContents
        {
            get { return Controller.StartMenuContents; }
        }

        public StartMenuItemDraggedAndDroppedEventHandler DragAndDropEventHandler
        {
            get { return this.HandleDragAndDropEvent; }
        }

        public StartMenuItemMarkedExcludedEventHandler ItemMarkedExcludedHandler
        {
            get { return this.HandleItemMarkedExcludedEvent; }
        }

        public StartMenuShortcutsView()
        {
            InitializeComponent();
            DataContext = this;
            
            Loaded += (object sender, RoutedEventArgs routedEvent) =>
            {
                Controller?.UpdateCurrentShortcuts();
            };
        }
        
        private void CurrentShortcutsLocationChanged(object sender, SelectionChangedEventArgs _)
        {
            var selectedLocation = (sender as ListBox)?.SelectedItem;
            StartMenuShortcutsLocation startMenuStartMenuShortcutsLocation = selectedLocation is StartMenuShortcutsLocation ? (StartMenuShortcutsLocation) selectedLocation : StartMenuShortcutsLocation.System;

            Controller.StartMenuStartMenuShortcutsLocation = startMenuStartMenuShortcutsLocation;
            Controller.UpdateCurrentShortcuts();
        }

        private void SaveCurrentShortcuts(object sender, RoutedEventArgs _)
        {
            Controller.SaveCurrentStartMenuItems();
        }

        private void HandleDragAndDropEvent(StartMenuItem droppedStartMenuItem, StartMenuItem recipient)
        {
           Controller.HandleRequestToMoveStartMenuItem(itemRequestingMove: droppedStartMenuItem, destinationItem: recipient);
        }        
        
        private void HandleItemMarkedExcludedEvent(StartMenuItem itemMarkedRemoved)
        {
           Controller.HandleRequestToExcludeStartMenuItem();
        }
        
    }
}
