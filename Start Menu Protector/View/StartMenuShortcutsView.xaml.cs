using System;
using System.Collections.ObjectModel;
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
            get { return Controller?.StartMenuContents; }
        }

        public Action<StartMenuItem> DraggedOverItemEnteredAreaEventHandler
        {
            get { return this.HandleDraggedItemEnteredArea; }
        }

        public Action<StartMenuItem> DraggedOverItemExitedAreaEventHandler
        {
            get { return this.HandleDraggedItemExitedArea; }
        }
        
        public StartMenuItemDraggedAndDroppedEventHandler DragAndDropEventHandler
        {
            get { return this.HandleDragAndDropEvent; }
        }

        public Action<StartMenuItem> ItemMarkedExcludedHandler
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

            if (Controller != null)
            {
                Controller.StartMenuStartMenuShortcutsLocation = startMenuStartMenuShortcutsLocation;
            }
            Controller?.UpdateCurrentShortcuts();
        }

        protected void HandlePrimaryActionButtonPressed(object sender, RoutedEventArgs _)
        {
            Controller?.ExecutePrimaryInteractionAction();
        }

        private void HandleDraggedItemEnteredArea(StartMenuItem target)
        {
            Controller?.HandleDraggedItemEnteredArea(target);
        }

        private void HandleDraggedItemExitedArea(StartMenuItem target)
        {
            Controller?.HandleDraggedItemExitedArea(target);
        }
        
        private void HandleDragAndDropEvent(StartMenuItem droppedStartMenuItem, StartMenuItem recipient)
        {
           Controller?.HandleRequestToMoveStartMenuItem(itemRequestingMove: droppedStartMenuItem, destinationItem: recipient);
        }        
        
        private void HandleItemMarkedExcludedEvent(StartMenuItem itemMarkedRemoved)
        {
           Controller?.HandleRequestToExcludeStartMenuItem();
        }
        
    }
}
