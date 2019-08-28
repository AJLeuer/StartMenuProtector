using System.Collections.ObjectModel;
using System.Threading.Tasks;
using StartMenuProtector.Data;
using StartMenuProtector.Util;
using StartMenuProtector.View;

namespace StartMenuProtector.Control 
{
    public abstract class StartMenuViewController 
    {
        public ActiveDataService ActiveDataService { get; set; }
        public SavedDataService SavedDataService { get; set; }
        public SystemStateService SystemStateService { get; set; }
        
        public ObservableCollection<IFileSystemItem> StartMenuContents { get;} = new AsyncObservableCollection<IFileSystemItem>();

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
    }
}