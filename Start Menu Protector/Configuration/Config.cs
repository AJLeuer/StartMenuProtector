using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using StartMenuProtector.Data;
using StartMenuProtector.Util;

namespace StartMenuProtector.Configuration 
{
	public static class Config
	{
		public const TargetEnvironment TargetBuildEnvironment =
		#if DEV
			TargetEnvironment.Development;
		#elif PROD
            TargetEnvironment.Production;
		#endif

		public static readonly RunningState StartupState = RunningState.Enabled;

		/// <summary>
		/// How often Start Menu Protector should check (and possibly fix) the state of the start menu
		/// </summary>
		// ReSharper disable once UnreachableCode
		public const uint ProtectorRunIntervalSeconds = (TargetBuildEnvironment == TargetEnvironment.Production) ? 60 : 4;
		
		#region IO
		public static Key ExcludeItemMainKey 	  = Key.Delete;
		public static Key ExcludeItemSecondaryKey = Key.Back;
		public static Key ReincludeItemKey 		  = Key.Insert;
		#endregion
		
		#region UI

		public const double MainWindowWidth = 1280;
		public const double MainWindowHeight = 720;
		public const double FontSize = 20;
		public static readonly FontFamily DefaultFontFamily = new FontFamily("Roboto");

		public static readonly Dictionary<TargetEnvironment, WindowState> StartupWindowState = new Dictionary<TargetEnvironment, WindowState>
		{
			{ TargetEnvironment.Development, WindowState.Maximized },
			{ TargetEnvironment.Production,  WindowState.Minimized }
		};

		public const string ApplicationIconFilePath = "/Assets/ApplicationIcon.ico";
		public const string TrayIconFilePath = "/Assets/TrayIcon.ico";

		public static readonly Color TextColor = Colors.Black;
		public static readonly Color BackgroundColor = Colors.White;
		public static readonly Color OutlineColor = Colors.LightGray;
		public static readonly Color SelectionTextColor = Colors.White;
		public static readonly Color SelectionBackgroundColor = Color.FromArgb(0xFF, 0x38, 0x92, 0xF1); //"#FF3892F1";
		public static readonly Color NegativeChangeSymbolicColor = Color.FromArgb(0xFF, 0xFF, 0x00, 0x60);
		public static readonly Color PositiveChangeSymbolicColor = Color.FromArgb(0xFF, 0x82, 0xFF, 0x00);

		public static Brush OutlineStrokeColor { get; set; } = new SolidColorBrush(OutlineColor);
		public static Brush TextStrokeColor { get; set; } = new SolidColorBrush(TextColor);
		public static Brush BackgroundFillColor { get; set; } = new SolidColorBrush(BackgroundColor);
		public static Brush SelectionTextStrokeColor { get; set; } = new SolidColorBrush(SelectionTextColor);

		public static Brush SelectionBackgroundFillColor { get; } = new LinearGradientBrush
		{
			EndPoint = new Point(0.5, 1),
			MappingMode = BrushMappingMode.RelativeToBoundingBox,
			StartPoint = new Point(0.5, 0),
			GradientStops =
			{
				new GradientStop { Color = SelectionBackgroundColor },
				new GradientStop { Color = Color.FromArgb(0xFF, 0x48, 0x77, 0xAA) },
				new GradientStop { Color = Color.FromArgb(0xFF, 0x4C, 0x8D, 0xD3) }
			}
		};

		public static Brush NegativeChangeSymbolicFillColor { get; } = new LinearGradientBrush
		{
			EndPoint = new Point(0.5, 1),
			MappingMode = BrushMappingMode.RelativeToBoundingBox,
			StartPoint = new Point(0.5, 0),
			GradientStops =
			{
				new GradientStop { Color = NegativeChangeSymbolicColor },
				new GradientStop { Color = Color.FromArgb(NegativeChangeSymbolicColor.A, NegativeChangeSymbolicColor.R, NegativeChangeSymbolicColor.G, (byte) (NegativeChangeSymbolicColor.B + 0x08)) },
				new GradientStop { Color = Color.FromArgb(NegativeChangeSymbolicColor.A, NegativeChangeSymbolicColor.R, NegativeChangeSymbolicColor.G, (byte) (NegativeChangeSymbolicColor.B + 0x0F)) }
			}
		};

		public static Brush PositiveChangeSymbolicFillColor { get; } = new LinearGradientBrush
		{
			EndPoint = new Point(0.5, 1),
			MappingMode = BrushMappingMode.RelativeToBoundingBox,
			StartPoint = new Point(0.5, 0),
			GradientStops =
			{
				new GradientStop { Color = PositiveChangeSymbolicColor },
				new GradientStop { Color = Color.FromArgb(PositiveChangeSymbolicColor.A, (byte) (PositiveChangeSymbolicColor.R + 0x08), PositiveChangeSymbolicColor.G, PositiveChangeSymbolicColor.B) },
				new GradientStop { Color = Color.FromArgb(PositiveChangeSymbolicColor.A, (byte) (PositiveChangeSymbolicColor.R + 0x0F), PositiveChangeSymbolicColor.G, PositiveChangeSymbolicColor.B) }
			}
		};

		public static double HiddenItemOpacity { get; } = 0.5;

		#endregion

		public static readonly ISet<Func<FileSystemItem, Boolean>> FileSystemItemFilters = new HashSet<Func<FileSystemItem, Boolean>>
		{
			(FileSystemItem item) => { return ((item.OwnerType == OwnerType.OS) && (item.PrettyName != "Programs") && (item.PrettyName != "Start Menu")); },
			(FileSystemItem item) => { return (String.Equals(item.Name, "desktop.ini", StringComparison.OrdinalIgnoreCase)); }
		};

		public enum TargetEnvironment
		{
			Development,
			Production
		}
	}
}