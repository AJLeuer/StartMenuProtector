using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using StartMenuProtector.Data;
using StartMenuProtector.Util;
using Directory = StartMenuProtector.Data.Directory;

namespace StartMenuProtector.ViewModel
{
    public class StartMenuDirectory : Directory, IStartMenuDirectory
    {
        public override bool Valid 
        {
            get 
            {
                if (IsExcluded)
                {
                    return false;
                }

                return base.Valid;
            }
        }

        private bool focused = default;
        private bool selected = default;
        private bool isExcluded = false;

        public bool IsFocused 
        {
            get
            {
                return focused;
            }
            set
            {
                focused = value;
                
                if (IsFocused)
                {
                    Focused?.Invoke(this, null);
                }

                IsSelected = IsFocused;
            }
        }

        public bool IsSelected
        {
            get { return IsFocused || selected; }
            set
            {
                selected = value;

                if (IsSelected)
                {
                    Selected?.Invoke(this, null);
                }
                else
                {
                    Deselected?.Invoke(this, null);
                }
                
                Action<IStartMenuItem> changeSelectionState = (IStartMenuItem startMenuItem) => { startMenuItem.IsSelected = this.IsSelected; };
                
                PropogateStateChange(changeSelectionState);
            }
        }

        public bool IsExcluded
        {
            get { return isExcluded; }
            set
            {
                isExcluded = value;

                if (IsExcluded)
                {
                    Excluded?.Invoke(this, null);
                }
                else
                {
                    Reincluded?.Invoke(this, null);
                }

                Action<IStartMenuItem> changeExclusionState = (IStartMenuItem startMenuItem) => { startMenuItem.IsExcluded = this.IsExcluded; };
                
                this.PropogateStateChange(changeExclusionState);
            }
        }

        public event RoutedEventHandler Focused;
        public event RoutedEventHandler Selected;
        public event RoutedEventHandler Deselected;
        public event RoutedEventHandler Excluded;
        public event RoutedEventHandler Reincluded;

        public StartMenuDirectory(DirectoryInfo directoryInfo) : 
            base(directoryInfo)
        {
            
        }

        public StartMenuDirectory(IDirectory directory) :
            this(directory.Self)
        {
            
        }

        public StartMenuDirectory(string path) : 
            this(new DirectoryInfo(path))
        {
            
        }
        
        protected override void InitializeContents()
        {
            lock (ContentsAccessLock)
            {
                base.InitializeContents();
                
                var startingContents = new List<IFileSystemItem>(contents);

                contents.Clear();
                directories.Clear();
                files.Clear();

                foreach (IFileSystemItem item in startingContents)
                {
                    if (item is IFile file)
                    {
                        var startMenuShortcut = new StartMenuFile(file);
                        files.Add(startMenuShortcut);
                    }
                    else if (item is IDirectory directory)
                    {
                        var startMenuDirectory = new StartMenuDirectory(directory);
                        startMenuDirectory.InitializeContents();
                        directories.Add(startMenuDirectory);
                    }
                }
                
                contents.Clear();
                contents.AddAll(files);
                contents.AddAll(directories);
            }
        }
        
        
        private void PropogateStateChange(Action<IStartMenuItem> stateChanger)
        {
            foreach (IFileSystemItem item in Contents)
            {
                if (item is IStartMenuItem startMenuItem)
                {
                    stateChanger(startMenuItem);
                }
            }
        }
    }
}