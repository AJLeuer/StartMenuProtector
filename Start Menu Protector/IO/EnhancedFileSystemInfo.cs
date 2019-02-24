using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using StartMenuProtector.Util;

namespace StartMenuProtector.IO
{
    public abstract class EnhancedFileSystemInfo : FileSystemInfo
    {
        protected FileSystemInfo FileSystemItem { get; set; }

        public override string Name
        {
            get { return FileSystemItem.Name; }
        }

        public string PrettyName
        {
            get
            {
                ushort baseNameLength = (ushort)(FileSystemItem.Name.Length - FileSystemItem.Extension.Length);
                return FileSystemItem.Name.Substring(0, baseNameLength);
            }
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

        public override void Delete() { FileSystemItem.Delete(); }
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
    
    public class EnhancedFileInfo : EnhancedFileSystemInfo
    {
        private static ImageSourceConverter ImageConverter = new ImageSourceConverter();

        public BitmapImage Icon { get; }

        public EnhancedFileInfo(FileInfo file) : 
            base(file)
        {
            Bitmap icon = System.Drawing.Icon.ExtractAssociatedIcon(FullName).ToBitmap();
            Icon = icon.ConvertToImageSource();
        }

        public EnhancedFileInfo(string path) : 
            this(new FileInfo(path))
        {
            
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
        
        public static EnhancedFileInfo[] GetFilesEnhanced(this DirectoryInfo directoryInfo)
        {
            FileInfo[] files = directoryInfo.GetFiles();
            var enhancedFiles = new List<EnhancedFileInfo>();
            
            foreach (FileInfo file in files)
            {
                var enhancedFile = new EnhancedFileInfo(file);
                enhancedFiles.Add(enhancedFile);
            }

            return enhancedFiles.ToArray();
        }
        
        public static FileSystemInfo[] GetContents(this DirectoryInfo directoryInfo)
        {
            var contents = new List<FileSystemInfo>();

            foreach (FileSystemInfo directory in directoryInfo.GetDirectoriesEnhanced())
            {
                contents.Add(directory);
            }

            foreach (FileSystemInfo file in directoryInfo.GetFilesEnhanced())
            {
                contents.Add(file);
            }

            return contents.ToArray();
        }
    }
}