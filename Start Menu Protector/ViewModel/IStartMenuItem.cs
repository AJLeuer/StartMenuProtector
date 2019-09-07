using System;
using System.Windows;
using StartMenuProtector.Data;

namespace StartMenuProtector.ViewModel
{
    public interface IStartMenuItem : IFileSystemItem
    {
        bool IsFocused { get; set; }
        bool IsSelected { get; set; }
        bool MarkedForExclusion { get; set; }

        event RoutedEventHandler Focused;
        event RoutedEventHandler Selected;
        event RoutedEventHandler Deselected;
    }

    public interface IStartMenuDirectory : IStartMenuItem, IDirectory
    {
        
    }
    
    public interface IStartMenuFile : IStartMenuItem, IFile
    {
        
    }

    public static class StartMenuItemFactory
    {
        public static IStartMenuItem CreateFromBaseFileSystemType(IFileSystemItem item)
        {
            switch (item)
            {
                case IFile file:
                    return new StartMenuFile(file);
                case IDirectory directory:
                    return new StartMenuDirectory(directory);
                default:
                    throw new ArgumentException("Unrecognized subtype of IFileSystemItem");
            }
        }
    }
}