using System;
using System.Collections.Generic;
using System.IO;
using StartMenuProtector.Control;
using Syroot.Windows.IO;
using Directory = StartMenuProtector.Data.Directory;

namespace StartMenuProtector.Configuration
{
    public static class Globals
    {
        public static readonly String SystemStartMenuItemsPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)}";
        public static readonly String UserStartMenuItemsPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)}";

        public static readonly Dictionary<StartMenuShortcutsLocation, String> StartMenuItemsPath = new Dictionary<StartMenuShortcutsLocation, String>
        {
            {StartMenuShortcutsLocation.User, UserStartMenuItemsPath},
            {StartMenuShortcutsLocation.System, SystemStartMenuItemsPath}
        }; 
        
        public const string ApplicationName              = "Start Menu Protector";
        public const string SystemShortcutsDirectoryName = "System Shortcuts";
        public const string UserShortcutsDirectoryName   = "User Shortcuts";

        public static readonly Directory UserAppData                = new Directory(new KnownFolder(KnownFolderType.RoamingAppData).Path);
        public static readonly Directory StartMenuProtectorAppData  = new Directory(System.IO.Directory.CreateDirectory(Path.Combine(UserAppData.Path, ApplicationName)));
        
        public static readonly Directory ActiveStartMenuItems       = new Directory(System.IO.Directory.CreateDirectory(Path.Combine(StartMenuProtectorAppData.Path, "Active")));
        public static readonly Directory ActiveSystemStartMenuItems = new Directory(System.IO.Directory.CreateDirectory(Path.Combine(ActiveStartMenuItems.Path, SystemShortcutsDirectoryName)));
        public static readonly Directory ActiveUserStartMenuItems   = new Directory(System.IO.Directory.CreateDirectory(Path.Combine(ActiveStartMenuItems.Path, UserShortcutsDirectoryName)));
        
        public static readonly Directory SavedStartMenuItems        = new Directory(System.IO.Directory.CreateDirectory(Path.Combine(StartMenuProtectorAppData.Path, "Saved")));
        public static readonly Directory SavedSystemStartMenuItems  = new Directory(System.IO.Directory.CreateDirectory(Path.Combine(SavedStartMenuItems.Path, SystemShortcutsDirectoryName)));
        public static readonly Directory SavedUserStartMenuItems    = new Directory(System.IO.Directory.CreateDirectory(Path.Combine(SavedStartMenuItems.Path, UserShortcutsDirectoryName)));

        public static readonly Dictionary<StartMenuShortcutsLocation, Directory> SavedStartMenuItemsSubdirectory = new Dictionary<StartMenuShortcutsLocation, Directory>
        {
            { StartMenuShortcutsLocation.User,   SavedUserStartMenuItems   },
            { StartMenuShortcutsLocation.System, SavedSystemStartMenuItems }
        };
    }
}