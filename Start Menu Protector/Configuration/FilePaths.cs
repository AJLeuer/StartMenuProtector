using System;
using System.Collections.Generic;
using System.IO;
using StartMenuProtector.Control;
using Syroot.Windows.IO;
using Directory = StartMenuProtector.Data.Directory;

namespace StartMenuProtector.Configuration
{
	public static class FilePaths
	{
		public const string ApplicationName                = "Start Menu Protector";
		public const string SystemShortcutsDirectoryName   = "System Shortcuts";
		public const string UserShortcutsDirectoryName     = "User Shortcuts";
		public const string StartMenuDirectoryName         = "Start Menu";
		public const string StartMenuProgramsDirectoryName = "Programs";


		private static readonly Dictionary<Config.TargetEnvironment, String> AppDataPaths = new Dictionary<Config.TargetEnvironment, String>
		{
			{ Config.TargetEnvironment.Development, @"Development App Data\" },
			{ Config.TargetEnvironment.Production,  new KnownFolder(KnownFolderType.RoamingAppData).Path }
		};

		private static Dictionary<Config.TargetEnvironment, String> SystemStartMenuItemsPaths { get; } = new Dictionary<Config.TargetEnvironment, String>
		{
			{ Config.TargetEnvironment.Development, $@"Development Start Menu Items\System Start Menu\{StartMenuDirectoryName}\{StartMenuProgramsDirectoryName}" },
			{ Config.TargetEnvironment.Production,  $@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)}\{StartMenuProgramsDirectoryName}" }
		};

		private static Dictionary<Config.TargetEnvironment, String> UserStartMenuItemsPaths { get; } = new Dictionary<Config.TargetEnvironment, String>
		{
			{ Config.TargetEnvironment.Development, $@"Development Start Menu Items\User Start Menu\{StartMenuDirectoryName}\{StartMenuProgramsDirectoryName}" },
			{ Config.TargetEnvironment.Production,  $@"{Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)}\{StartMenuProgramsDirectoryName}" }
		};

		public static Dictionary<StartMenuShortcutsLocation, String> StartMenuItemsPath { get; } = new Dictionary<StartMenuShortcutsLocation, String>
		{
			{ StartMenuShortcutsLocation.User,   UserStartMenuItemsPaths[Config.TargetBuildEnvironment] },
			{ StartMenuShortcutsLocation.System, SystemStartMenuItemsPaths[Config.TargetBuildEnvironment] }
		};

		public static readonly Directory AppData                         = new Directory(AppDataPaths[Config.TargetBuildEnvironment]);
		public static readonly Directory StartMenuProtectorAppData       = new Directory(System.IO.Directory.CreateDirectory(Path.Combine(AppData.Path, ApplicationName)));

		public static readonly Directory LogsDirectory                   = new Directory(System.IO.Directory.CreateDirectory(Path.Combine(StartMenuProtectorAppData.Path, "Logs")));

		public static readonly Directory ActiveStartMenuItems            = new Directory(System.IO.Directory.CreateDirectory(Path.Combine(StartMenuProtectorAppData.Path, "Active")));
		public static readonly Directory ActiveSystemStartMenuItems      = new Directory(System.IO.Directory.CreateDirectory(Path.Combine(ActiveStartMenuItems.Path, SystemShortcutsDirectoryName)));
		public static readonly Directory ActiveUserStartMenuItems        = new Directory(System.IO.Directory.CreateDirectory(Path.Combine(ActiveStartMenuItems.Path, UserShortcutsDirectoryName)));

		public static readonly Directory SavedStartMenuItems             = new Directory(System.IO.Directory.CreateDirectory(Path.Combine(StartMenuProtectorAppData.Path, "Saved")));
		public static readonly Directory SavedSystemStartMenuItems       = new Directory(System.IO.Directory.CreateDirectory(Path.Combine(SavedStartMenuItems.Path, SystemShortcutsDirectoryName)));
		public static readonly Directory SavedUserStartMenuItems         = new Directory(System.IO.Directory.CreateDirectory(Path.Combine(SavedStartMenuItems.Path, UserShortcutsDirectoryName)));

		public static readonly Directory QuarantinedStartMenuItems       = new Directory(System.IO.Directory.CreateDirectory(Path.Combine(StartMenuProtectorAppData.Path, "Quarantined")));
		public static readonly Directory QuarantinedSystemStartMenuItems = new Directory(System.IO.Directory.CreateDirectory(Path.Combine(QuarantinedStartMenuItems.Path, SystemShortcutsDirectoryName)));
		public static readonly Directory QuarantinedUserStartMenuItems   = new Directory(System.IO.Directory.CreateDirectory(Path.Combine(QuarantinedStartMenuItems.Path, UserShortcutsDirectoryName)));
	}
}