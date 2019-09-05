using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using StartMenuProtector.Data;
using StartMenuProtector.Util;
using StartMenuProtector.View;
using StartMenuProtector.ViewModel;

namespace StartMenuProtector.Control 
{
    public abstract class StartMenuViewController 
    {
        public ActiveDataService ActiveDataService { get; set; }
        public SavedDataService SavedDataService { get; set; }
        public SystemStateService SystemStateService { get; set; }
        
        public ObservableCollection<IStartMenuItem> StartMenuContents { get;} = new AsyncObservableCollection<IStartMenuItem>();

        public StartMenuShortcutsLocation StartMenuStartMenuShortcutsLocation { get; set; } = StartMenuShortcutsLocation.System;

        protected StartMenuViewController(ActiveDataService activeDataService, SavedDataService savedDataService, SystemStateService systemStateService)
        {
            this.ActiveDataService  = activeDataService;
            this.SavedDataService   = savedDataService;
            this.SystemStateService = systemStateService;
        }

        public abstract Task UpdateCurrentShortcuts();

        public abstract void ExecutePrimaryInteractionAction();

        public virtual void HandleDraggedItemEnteredArea(StartMenuItemView target)
        {
            if (target.File.IsOfType<IDirectory>())
            {
                target.CandidateForDrop = true;
            }
        }

        public virtual void HandleDraggedItemExitedArea(StartMenuItemView target)
        {
            if (target.File.IsOfType<IDirectory>())
            {
                target.CandidateForDrop = false;
            }
        }

        public abstract Task HandleRequestToMoveStartMenuItem(IStartMenuItemView itemViewRequestingMove, IStartMenuItemView destinationItemView);

        public abstract void HandleRequestToExcludeStartMenuItem();
        
        
        public void HandleItemGainedFocusEvent(object sender, RoutedEventArgs eventInfo)
        {
        }

        public void HandleItemLostFocusEvent(object sender, RoutedEventArgs eventInfo)
        {
        }
        
        protected static ICollection<IStartMenuItem> CreateStartMenuItemsFromData(ICollection<IFileSystemItem> startMenuDataContents)
        {
            ICollection<IStartMenuItem> startMenuItems = new List<IStartMenuItem>();

            foreach (IFileSystemItem fileSystemItem in startMenuDataContents)
            {
                IStartMenuItem startMenuItem = StartMenuItemFactory.CreateFromBaseFileSystemType(fileSystemItem);
                startMenuItems.Add(startMenuItem);
            }

            return startMenuItems;
        }
    }
}