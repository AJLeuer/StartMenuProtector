using System.Collections.Generic;
using System.IO;

namespace StartMenuProtector.IO
{
    public abstract class EnhancedFileSystemInfo : FileSystemInfo
    {
        protected FileSystemInfo FileSystemItem { get; set; }

        public override string Name
        {
            get { return FileSystemItem.Name; }
        }
        
        public override string FullName
        {
            get { return FileSystemItem.FullName; }
        }
        
        public override bool Exists
        {
            get { return FileSystemItem.Exists; }
        }
        
        public EnhancedFileSystemInfo(FileSystemInfo fileSystemItem)
        {
            this.FileSystemItem = fileSystemItem;
        }

        public override void Delete() {}
    }

    public class EnhancedDirectoryInfo : EnhancedFileSystemInfo
    {
        public EnhancedDirectoryInfo(DirectoryInfo directory) : 
            base(directory)
        {
            
        }

        public EnhancedDirectoryInfo(string path) : 
            this(new DirectoryInfo(path))
        {
            
        }
        
        public FileSystemInfo[] Contents
        {
            get { return (FileSystemItem as DirectoryInfo).GetContents(); }
        }
        
        public FileInfo[] Files
        {
            get { return (FileSystemItem as DirectoryInfo)?.GetFiles(); }
        }

        public EnhancedFileSystemInfo[] Directories
        {
            get { return (FileSystemItem as DirectoryInfo).GetDirectoriesEnhanced(); }
        }
    }

    public static class DirectoryInfoExtensions
    {
        public static EnhancedFileSystemInfo[] GetDirectoriesEnhanced(this DirectoryInfo directoryInfo)
        {
            DirectoryInfo[] directories = directoryInfo.GetDirectories();
            var enhancedDirectories = new List<EnhancedFileSystemInfo>();
            
            foreach (DirectoryInfo directory in directories)
            {
                var enhancedDirectory = new EnhancedDirectoryInfo(directory);
                enhancedDirectories.Add(enhancedDirectory);
            }

            return enhancedDirectories.ToArray();
        }
        
        public static FileSystemInfo[] GetContents(this DirectoryInfo directoryInfo)
        {
            var contents = new List<FileSystemInfo>();

            foreach (FileSystemInfo directory in directoryInfo.GetDirectoriesEnhanced())
            {
                contents.Add(directory);
            }

            foreach (FileSystemInfo file in directoryInfo.GetFiles())
            {
                contents.Add(file);
            }

            return contents.ToArray();
        }
    }
}