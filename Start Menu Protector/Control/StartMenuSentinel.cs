﻿using System;
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
        public RunningState State { get; private set; } = RunningState.Disabled;
        private readonly AutoResetEvent ReenabledFlag = new AutoResetEvent (false);

        private Thread Thread { get; set; }
        
        public SystemStateService SystemStateService { private get; set; }
        public SavedDataService SavedDataService { private get; set; }
        
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

        public StartMenuSentinel(SystemStateService systemStateService, SavedDataService savedDataService)
        {
            this.SystemStateService = systemStateService;
            this.SavedDataService = savedDataService;
        }
        
        public StartMenuSentinel(SystemStateService systemStateService, SavedDataService savedDataService, Toggleable toggle):
            this(systemStateService, savedDataService)
        {
            toggle.ToggleOnEvent += Enable;
            toggle.ToggleOffEvent += Disable;
        }
        
        public void Start()
        {
            Thread = new Thread(Run);
            Enable();
            Thread.Start();
        }

        public void Enable()
        {
            lock (State)
            {
                this.State = RunningState.Enabled;
            }
            
            ReenabledFlag.Set();
        }
        
        public void Disable()
        {
            lock (State)
            {
                this.State = RunningState.Disabled;
            }
        }
        
        private void Run()
        {
            while (true)
            {
                while (State == RunningState.Enabled)
                {
                    lock (State)
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
                
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }

                if (State == RunningState.Disabled)
                {
                    ReenabledFlag.WaitOne();
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
                
                RestoreStartMenuItems(location);
            }
        }

        private (ICollection<RelocatableItem> unexpected, ICollection<RelocatableItem> missing) CheckForDivergencesFromUsersSavedStartMenuState(StartMenuShortcutsLocation location)
        {
            ICollection<RelocatableItem> unexpected = new HashSet<RelocatableItem>();
                
            ICollection<RelocatableItem> absent     = new HashSet<RelocatableItem>();

            Option<Directory> appDataSavedStartMenuContents = SavedDataService.GetStartMenuContentDirectoryMainSubdirectory(location).Result;

            if (appDataSavedStartMenuContents.HasValue)
            {
                SystemStateService.OSEnvironmentStartMenuItems[location].RefreshContents();
                Directory currentStartMenuItemsDirectoryState = SystemStateService.OSEnvironmentStartMenuItems[location];
                Directory expectedStartMenuStateDirectoryState = appDataSavedStartMenuContents.ValueOrFailure();

                (unexpected, absent) = Directory.FindDivergences(sourceOfTruth: expectedStartMenuStateDirectoryState, test: currentStartMenuItemsDirectoryState);
            }
            
            return (unexpected, absent);
        }
        
        
        private void UpdateSavedDataWithNewerItemCounterParts(StartMenuShortcutsLocation location, ICollection<RelocatableItem> unexpectedItems)
        {
            Option<Directory> appDataSavedStartMenuContents = SavedDataService.GetStartMenuContentDirectoryMainSubdirectory(location).Result;

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
        
        
        private void RestoreStartMenuItems(StartMenuShortcutsLocation location)
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
                
                String relativePath = itemToRestore.Path.Substring((SavedStartMenuItemsSubdirectory[location].Path + @"\Start Menu").Length + 1);
                String restoredPath = Path.Combine(StartMenuItemsPath[location], relativePath);

                if (itemToRestore.IsOfType<IDirectory>())
                {
                    restoredPath =  Path.GetDirectoryName(restoredPath); //gets parent's directory
                    itemToRestore.Copy(restoredPath);
                }
                else /* if file */
                {
                    restoredPath = Path.GetDirectoryName(restoredPath);
                    itemToRestore.Copy(restoredPath);
                }

            }
            
            ItemsToRestore[location].Clear();
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
