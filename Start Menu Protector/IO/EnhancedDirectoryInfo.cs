using System;
using System.Collections.Generic;
using System.IO;

namespace StartMenuProtector.IO
{
    public class EnhancedDirectoryInfo : FileSystemInfo
    {
        private DirectoryInfo Directory;

        public override string Name
        {
            get { return Directory.Name; }
        }
        
        public override string FullName
        {
            get { return Directory.FullName; }
        }
        
        public override bool Exists
        {
            get { return Directory.Exists; }
        }
        
        public FileSystemInfo[] Contents
        {
            get { return Directory.GetContents(); }
        }
        
        public FileInfo[] Files
        {
            get { return Directory.GetFiles(); }
        }

        public EnhancedDirectoryInfo[] Directories
        {
            get { return Directory.GetDirectoriesEnhanced(); }
        }
        
        public EnhancedDirectoryInfo(DirectoryInfo directory)
        {
            this.Directory = directory;
        }

        public EnhancedDirectoryInfo(string path) :
            this(new DirectoryInfo(path))
        {
            
        }

        public override void Delete() {}

    }

    public static class DirectoryInfoExtensions
    {
        public static EnhancedDirectoryInfo[] GetDirectoriesEnhanced(this DirectoryInfo directoryInfo)
        {
            DirectoryInfo[] directories = directoryInfo.GetDirectories();
            var enhancedDirectories = new List<EnhancedDirectoryInfo>();
            
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