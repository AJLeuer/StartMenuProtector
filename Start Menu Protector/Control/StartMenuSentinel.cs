using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Optional;
using Optional.Unsafe;
using StartMenuProtector.Data;
using StartMenuProtector.Util;
using StartMenuProtector.View;
using static StartMenuProtector.Util.Util;
using static StartMenuProtector.Configuration.Globals;
using Directory = StartMenuProtector.Data.Directory;

namespace StartMenuProtector.Control 
{
    public class StartMenuSentinel 
    {
        private RunningState applicationState = RunningState.Disabled;

        public RunningState ApplicationState
        {
            get
            {
                return applicationState;
            }
            private set
            {
                if (value == RunningState.Disabled)
                {
                    Disable();
                }
                
                applicationState = value;
                
                if (value == RunningState.Disabled)
                {
                    ContinueRunFlag.Set();
                }
            }
        }

        public RunningState UserSelectedState { get; private set; } = RunningState.Disabled;
        private readonly AutoResetEvent ContinueRunFlag = new AutoResetEvent (false);

        private Thread Thread { get; set; }
        
        public SystemStateService SystemStateService { private get; set; }
        public SavedDataService SavedDataService { private get; set; }
        public QuarantineDataService QuarantineDataService { private get; set; }


        public readonly Dictionary<StartMenuShortcutsLocation, ICollection<IFileSystemItem>> ItemsToRestore = new Dictionary<StartMenuShortcutsLocation, ICollection<IFileSystemItem>>
        {
            { StartMenuShortcutsLocation.User,   new HashSet<IFileSystemItem>() },
            { StartMenuShortcutsLocation.System, new HashSet<IFileSystemItem>() }
        };
        
        public readonly Dictionary<StartMenuShortcutsLocation, ICollection<IFileSystemItem>> ItemsToQuarantine = new Dictionary<StartMenuShortcutsLocation, ICollection<IFileSystemItem>>
        {
            { StartMenuShortcutsLocation.User,   new HashSet<IFileSystemItem>() },
            { StartMenuShortcutsLocation.System, new HashSet<IFileSystemItem>() }
        };
        
        public StartMenuSentinel(SystemStateService systemStateService, SavedDataService savedDataService, QuarantineDataService quarantineDataService)
        {
            this.SystemStateService = systemStateService;
            this.SavedDataService = savedDataService;
            this.QuarantineDataService = quarantineDataService;
        }
        
        public StartMenuSentinel(SystemStateService systemStateService, SavedDataService savedDataService, QuarantineDataService quarantineDataService, Toggleable toggle):
            this(systemStateService, savedDataService, quarantineDataService)
        {
            toggle.ToggleOnEvent  += Enable;
            toggle.ToggleOffEvent += Disable;
        }
        
        public void Start()
        {
            ApplicationState = RunningState.Enabled;
            Thread = new Thread(Run);
            Thread.Start();
        }
        
        public void Stop()
        {
            ApplicationState = RunningState.Disabled;
            Thread.Join();
        }

        public void Enable()
        {
            lock (UserSelectedState)
            {
                this.UserSelectedState = RunningState.Enabled;
            }
            
            ContinueRunFlag.Set();
        }
        
        public void Disable()
        {
            lock (UserSelectedState)
            {
                this.UserSelectedState = RunningState.Disabled;
            }
        }
        
        private void Run()
        {
            while (ApplicationState == RunningState.Enabled)
            {
                while (UserSelectedState == RunningState.Enabled)
                {
                    lock (UserSelectedState)
                    {
                        try
                        {
                            MonitorStartMenuState();
                        }
                        catch (Exception exception)
                        {
                            //todo: need true logging
                            Console.WriteLine(exception);
                        }
                    }
                
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                }

                if ((ApplicationState == RunningState.Enabled) && (UserSelectedState == RunningState.Disabled))
                {
                    ContinueRunFlag.WaitOne();
                }
            }
        }

        private void MonitorStartMenuState()
        {
            foreach (StartMenuShortcutsLocation location in GetEnumValues<StartMenuShortcutsLocation>())
            {
                var (unexpected, missing) = CheckForDivergencesFromUsersSavedStartMenuState(location);
                
                UpdateSavedDataWithNewerItemCounterParts(location: location, unexpectedItems: unexpected);
                FilterOutItemsWithTheSameName(location, unexpectedItems: unexpected, missingItems: missing);
                FilterOutShortcutsMovedOutOfPosition(unexpectedItems: unexpected, missingItems: missing);

                ItemsToQuarantine[location].AddAll(unexpected);
                ItemsToRestore[location]   .AddAll(missing);
                
                RestoreExpectedStartMenuItems(location);
                QuarantineUnrecognizedStartMenuItems(location);
            }
        }

        private (ICollection<RelocatableItem> unexpected, ICollection<RelocatableItem> missing) CheckForDivergencesFromUsersSavedStartMenuState(StartMenuShortcutsLocation location)
        {
            ICollection<RelocatableItem> unexpected = new HashSet<RelocatableItem>();
                
            ICollection<RelocatableItem> absent     = new HashSet<RelocatableItem>();

            Option<IDirectory> appDataSavedStartMenuContents = SavedDataService.GetStartMenuContentDirectoryMainSubdirectory(location).Result;

            if (appDataSavedStartMenuContents.HasValue)
            {
                SystemStateService.OSEnvironmentStartMenuItems[location].RefreshContents();
                Directory currentStartMenuItemsDirectoryState = SystemStateService.OSEnvironmentStartMenuItems[location];
                IDirectory expectedStartMenuStateDirectoryState = appDataSavedStartMenuContents.ValueOrFailure();

                (unexpected, absent) = Directory.FindDivergences(sourceOfTruth: expectedStartMenuStateDirectoryState, test: currentStartMenuItemsDirectoryState);
            }
            
            return (unexpected, absent);
        }
        
        
        private void UpdateSavedDataWithNewerItemCounterParts(StartMenuShortcutsLocation location, ICollection<RelocatableItem> unexpectedItems)
        {
            Option<IDirectory> appDataSavedStartMenuContents = SavedDataService.GetStartMenuContentDirectoryMainSubdirectory(location).Result;

            if (appDataSavedStartMenuContents.HasValue)
            {
                ICollection<IFileSystemItem> allUnexpectedItems = ExtractFlatListOfItems(unexpectedItems);
                ICollection<IFileSystemItem> savedStartMenuItems = appDataSavedStartMenuContents.ValueOrFailure().GetFlatContents();

                foreach (IFileSystemItem unexpectedItem in allUnexpectedItems)
                {
                    foreach (IFileSystemItem savedStartMenuItem in savedStartMenuItems)
                    {
                        if (savedStartMenuItem.Name == unexpectedItem.Name)
                        {
                            string savedDataLocation = savedStartMenuItem.ParentDirectoryPath;
                            savedStartMenuItem.Delete();

                            IEnumerable<RelocatableItem> toRemoveFromUnexpected = unexpectedItems.Where((RelocatableItem item) => item.Name == unexpectedItem.Name );
                            
                            foreach (var itemInUnexpected in toRemoveFromUnexpected.ToArray())
                            {
                                unexpectedItems.Remove(itemInUnexpected);
                            }
                            
                            Option<IFileSystemItem> itemToRestore = unexpectedItem.Move(savedDataLocation);

                            if (itemToRestore.HasValue)
                            {
                                ItemsToRestore[location].Add(itemToRestore.ValueOrFailure());
                            }
                        }
                    }
                }
            }
        }
        
        public void FilterOutItemsWithTheSameName(StartMenuShortcutsLocation location, ICollection<RelocatableItem> unexpectedItems, ICollection<RelocatableItem> missingItems)
        {
            var allUnexpectedItems = unexpectedItems.ToArray();
            var allMissingItems = missingItems.ToArray(); 
            
            foreach (RelocatableItem unexpectedItem in allUnexpectedItems)
            {
                foreach (RelocatableItem missingItem in allMissingItems)
                {
                    if (missingItem.Name == unexpectedItem.Name)
                    {
                        //we'll move the unexpected item to take the place of the missing item, since it's target might
                        //be a newer version of the original executable
                        string missingItemPath = missingItem.ParentDirectoryPath;
                        
                        Option<IFileSystemItem> itemToRestore = unexpectedItem.Move(missingItemPath);
                        missingItems.Remove(missingItem);
                        
                        missingItems.Remove(unexpectedItem);
                        unexpectedItems.Remove(unexpectedItem);
                        
                        if (itemToRestore.HasValue)
                        {
                            ItemsToRestore[location].Add(itemToRestore.ValueOrFailure());
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Reconciles unexpectedItems with missing items by looking for items that are shortcuts and getting their target file. Any unexpectedItem
        /// with the same target file as a missingItem is assumed to be that item. When two such matching items are found, they are removed from their
        /// respective lists.
        /// </summary>
        /// <param name="unexpectedItems"></param>
        /// <param name="missingItems"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void FilterOutShortcutsMovedOutOfPosition(ICollection<RelocatableItem> unexpectedItems, ICollection<RelocatableItem> missingItems)
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
        
        
        private void RestoreExpectedStartMenuItems(StartMenuShortcutsLocation location)
        {
            foreach (IFileSystemItem item in ItemsToRestore[location])
            {
                IFileSystemItem itemToRestore;
                
                if (item is RelocatableItem relocatableItem)
                {
                    itemToRestore = relocatableItem.UnderlyingItem;
                }
                else
                {
                    itemToRestore = item;
                }
                
                String relativePath = itemToRestore.Path.Substring(GetSavedStartMenuItemsPath(location).Length + 1);
                String restoredPath = Path.Combine(EnvironmentStartMenuItemsPath[location], relativePath);
                restoredPath        = Path.GetDirectoryName(restoredPath); //gets parent's directory
                
                itemToRestore.Copy(restoredPath);
            }
            
            ItemsToRestore[location].Clear();

            string GetSavedStartMenuItemsPath(StartMenuShortcutsLocation startMenuShortcutsLocation)
            {
                return (SavedDataService.StartMenuItemsStorage[startMenuShortcutsLocation].Path + @"\Start Menu");
            }
        }
        
        private void QuarantineUnrecognizedStartMenuItems(StartMenuShortcutsLocation location)
        {
            QuarantineDataService.MoveFileSystemItems(QuarantineDataService.StartMenuItemsStorage[location], ItemsToQuarantine[location].ToArray()).Wait();
            
            ItemsToQuarantine[location].Clear();
        }


        private static ICollection<IFileSystemItem> ExtractFlatListOfItems(ICollection<RelocatableItem> items)
        {
            ICollection<IFileSystemItem> flatContents = new HashSet<IFileSystemItem>();

            foreach (var item in items)
            {
                if (item.UnderlyingItem is IFile file)
                {
                    flatContents.Add(file);
                }
                else if (item.UnderlyingItem is IDirectory directory)
                {
                    flatContents.AddAll(directory.GetFlatContents());
                }
            }

            return flatContents;
        }
    }

    public class RunningState 
    {
        public enum Value
        {
            On,
            Off
        }
        
        public Value State { get; }
        
        public static RunningState Enabled  { get; } = new RunningState(state: Value.On);
        public static RunningState Disabled { get; } = new RunningState(state: Value.Off);

        private RunningState(Value state)
        {
            this.State = state;
        }
    }
}
