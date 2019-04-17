using System;
using System.Collections.Generic;
using System.Windows.Media;
using StartMenuProtector.Data;

namespace StartMenuProtector.Configuration
{
    public static class Config
    {
        public const ushort FontSize = 18;
        public static readonly Color TextColor = Colors.Black;
        public static readonly Color BackgroundColor = Colors.White;
        public static readonly Color OutlineColor = Colors.LightGray;
        public static readonly Color SelectionTextColor = Colors.White;
        public static readonly Color SelectionBackgroundColor = Color.FromArgb(0xFF, 0x38,0x92,0xF1); //"#FF3892F1";
        public static readonly Color MarkedDeletedBackgroundColor = Color.FromArgb(0xFF, 0xFF,0x00,0x60); //"#FF3892F1";
        public static readonly ISet<Func<EnhancedFileSystemInfo, Boolean>> FileSystemItemFilters = new HashSet<Func<EnhancedFileSystemInfo, Boolean>>
        {
            (EnhancedFileSystemInfo item) => { return ((item.OwnerType == OwnerType.OS) && (item.PrettyName != "Programs")); },
            (EnhancedFileSystemInfo item) => { return (String.Equals(item.Name,"desktop.ini", StringComparison.OrdinalIgnoreCase)); }
        };
    }
}