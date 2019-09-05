using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using StartMenuProtector.Control;
using StartMenuProtector.Util;
using StartMenuProtector.ViewModel;

namespace StartMenuProtector.View
{
    /// <summary>
    /// Interaction logic for StartMenuView.xaml
    /// </summary>
    public abstract partial class StartMenuShortcutsView : UserControl 
    {
        public ObservableCollection<StartMenuShortcutsLocation> Locations { get; set; } = new AsyncObservableCollection<StartMenuShortcutsLocation> { StartMenuShortcutsLocation.System, StartMenuShortcutsLocation.User };
        public StartMenuViewController Controller { get; set; }

        public ObservableCollection<IStartMenuItem> StartMenuContents
        {
            get { return Controller?.StartMenuContents; }
        }

        public Action<StartMenuItemView> DraggedOverItemEnteredAreaEventHandler
        {
            get { return this.HandleDraggedItemEnteredArea; }
        }

        public Action<StartMenuItemView> DraggedOverItemExitedAreaEventHandler
        {
            get { return this.HandleDraggedItemExitedArea; }
        }
        
        public StartMenuItemDraggedAndDroppedEventHandler DragAndDropEventHandler
        {
            get { return this.HandleDragAndDropEvent; }
        }

        public Action<StartMenuItemView> ItemMarkExcludedCompletedHandler
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
        
        private void HandleCurrentShortcutsLocationChanged(object sender, SelectionChangedEventArgs _)
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

        private void HandleDraggedItemEnteredArea(StartMenuItemView target)
        {
            Controller?.HandleDraggedItemEnteredArea(target);
        }

        private void HandleDraggedItemExitedArea(StartMenuItemView target)
        {
            Controller?.HandleDraggedItemExitedArea(target);
        }
        
        private void HandleDragAndDropEvent(StartMenuItemView droppedStartMenuItemView, StartMenuItemView recipient)
        {
           Controller?.HandleRequestToMoveStartMenuItem(itemViewRequestingMove: droppedStartMenuItemView, destinationItemView: recipient);
        }        
        
        private void HandleItemMarkedExcludedEvent(StartMenuItemView itemViewMarkedRemoved)
        {
           Controller?.HandleRequestToExcludeStartMenuItem();
        }

        private void HandleItemGainedFocusEvent(object sender, RoutedEventArgs eventInfo)
        {
            Controller?.HandleItemGainedFocusEvent(sender, eventInfo);
        }

        private void HandleItemLostFocusEvent(object sender, RoutedEventArgs eventInfo)
        {
            Controller?.HandleItemLostFocusEvent(sender, eventInfo);
        }
    }
}
