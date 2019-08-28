using System;
using StartMenuProtector.Data;

namespace StartMenuProtector.Models
{
    public interface IStartMenuItem : IFileSystemItem
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