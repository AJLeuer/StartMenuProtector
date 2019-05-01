using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Optional.Unsafe;
using StartMenuProtector.Data;
using static StartMenuProtector.Util.Util;

namespace StartMenuProtector.Control
{
    public class StartMenuSentinel
    {
        private Thread Thread;
        
        public SystemStateService SystemStateService { private get; set; }
        public SavedStartMenuDataService SavedStartMenuDataService { private get; set; }

        public StartMenuSentinel(SystemStateService systemStateService, SavedStartMenuDataService savedStartMenuDataService)
        {
            this.SystemStateService = systemStateService;
            this.SavedStartMenuDataService = savedStartMenuDataService;
        }
        
        public void Start()
        {
            Thread = new Thread(Run);
            Thread.Start();
        }

        private void Run()
        {
            var (unexpected, missing) = CheckForDivergencesFromUsersSavedStartMenuState();

            foreach (StartMenuShortcutsLocation location in GetEnumValues<StartMenuShortcutsLocation>())
            {
                FilterOutShortcutItemsMovedOutOfPosition(unexpectedItems: unexpected[location], missingItems: missing[location]);
            }
        }

        private  (Dictionary<StartMenuShortcutsLocation, ICollection<RelocatableItem>> unexpected, Dictionary<StartMenuShortcutsLocation, ICollection<RelocatableItem>> missing) CheckForDivergencesFromUsersSavedStartMenuState()
        {
            var unexpected = new Dictionary<StartMenuShortcutsLocation, ICollection<RelocatableItem>>
            {
                { StartMenuShortcutsLocation.User, new HashSet<RelocatableItem>() },
                { StartMenuShortcutsLocation.System, new HashSet<RelocatableItem>() }
            };
                
            var absent = new Dictionary<StartMenuShortcutsLocation, ICollection<RelocatableItem>>
            {
                { StartMenuShortcutsLocation.User, new HashSet<RelocatableItem>() },
                { StartMenuShortcutsLocation.System, new HashSet<RelocatableItem>() }
            };
            
            foreach (StartMenuShortcutsLocation location in GetEnumValues<StartMenuShortcutsLocation>())
            {
                var appDataSavedStartMenuContents = SavedStartMenuDataService.GetStartMenuContents(location).Result.GetSubdirectory("Start Menu");

                if (appDataSavedStartMenuContents.HasValue)
                {
                    SystemStateService.OSEnvironmentStartMenuItems[location].RefreshContents();
                    Directory currentStartMenuItemsDirectoryState = SystemStateService.OSEnvironmentStartMenuItems[location];
                    Directory expectedStartMenuStateDirectoryState = appDataSavedStartMenuContents.ValueOrFailure();

                    (unexpected[location], absent[location]) = Directory.FindDivergences(sourceOfTruth: expectedStartMenuStateDirectoryState, test: currentStartMenuItemsDirectoryState);
                }
            }

            return (unexpected, absent);
        }

        /// <summary>
        /// Reconciles unexpectedItems with missing items by looking for items that are shortcuts and getting their target file. Any unexpectedItem
        /// with the same target file as a missingItem is assumed to be that item. When two such matching items are found, they are removed from their
        /// respective lists.
        /// </summary>
        /// <param name="unexpectedItems"></param>
        /// <param name="missingItems"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void FilterOutShortcutItemsMovedOutOfPosition(ICollection<RelocatableItem> unexpectedItems, ICollection<RelocatableItem> missingItems)
        {
            Action filterOutShortcutItemsMovedOutOfPosition = () =>
            {
                var allUnexpectedItems = unexpectedItems.ToArray();
                var allMissingItems = missingItems.ToArray();
            
                foreach (RelocatableItem unexpectedItem in allUnexpectedItems)
                {
                    if ((unexpectedItem.UnderlyingItem is IFile unexpectedFile) && (unexpectedFile.GetShortcutTarget().HasValue))
                    {
                        FileSystemItem unexpectedFileShortcutTarget = unexpectedFile.GetShortcutTarget().ValueOrFailure();
                    
                        foreach (RelocatableItem missingItem in allMissingItems)
                        {
                            if ((missingItem.UnderlyingItem is IFile missingFile) && (missingFile.GetShortcutTarget().HasValue))
                            {
                                FileSystemItem missingFileShortcutTarget = missingFile.GetShortcutTarget().ValueOrFailure();

                                if (unexpectedFileShortcutTarget.Path == missingFileShortcutTarget.Path)
                                {
                                    unexpectedItems.Remove(unexpectedItem);
                                    missingItems.Remove(missingItem);
                                    break;
                                }
                            }
                        }
                    }
                }
            } ;

            StartSTATask(filterOutShortcutItemsMovedOutOfPosition).Wait();
        }
        
        
    }
}
