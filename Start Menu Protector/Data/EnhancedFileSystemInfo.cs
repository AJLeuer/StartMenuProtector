using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.AccessControl;
using System.Windows.Media.Imaging;
using StartMenuProtector.Util;

namespace StartMenuProtector.Data
{
    public abstract class EnhancedFileSystemInfo : FileSystemInfo
    {
        protected FileSystemInfo OriginalFileSystemItem { get; set; }

        public override string Name
        {
            get { return OriginalFileSystemItem.Name; }
        }

        public string PrettyName
        {
            get
            {
                ushort baseNameLength = (ushort)(OriginalFileSystemItem.Name.Length - OriginalFileSystemItem.Extension.Length);
                return OriginalFileSystemItem.Name.Substring(0, baseNameLength);
            }
        }

        public virtual string Path
        {
            get { return FullName; }
        }
        public override string FullName
        {
            get { return OriginalFileSystemItem.FullName; }
        }

        public new string Extension 
        {
            get
            {
                return OriginalFileSystemItem.Extension;
            }
        }

        public new FileAttributes Attributes 
        {
            get
            {
                return OriginalFileSystemItem.Attributes;
            }
        }
        
        public override bool Exists 
        {
            get { return OriginalFileSystemItem.Exists; }
        }
        
        public new DateTime CreationTime 
        {
            get
            {
                return OriginalFileSystemItem.CreationTime;
            }
        }

        public DateTime CreationTimeUTC 
        {
            get { return OriginalFileSystemItem.CreationTimeUtc; }
        }

        public new DateTime LastAccessTime
        {
            get { return OriginalFileSystemItem.LastAccessTime; }
        }

        public DateTime LastAccessTimeUTC
        {
            get { return OriginalFileSystemItem.LastAccessTimeUtc; }
        }

        public new DateTime LastWriteTime
        {
            get { return OriginalFileSystemItem.LastWriteTime; }
        }

        public DateTime LastWriteTimeUTC
        {
            get { return OriginalFileSystemItem.LastWriteTimeUtc; }
        }

        protected EnhancedFileSystemInfo(FileSystemInfo originalFileSystemItem)
        {
            OriginalFileSystemItem = originalFileSystemItem;
        }

        public override void Delete() { OriginalFileSystemItem.Delete(); }
        
        /// <summary>
        /// Copies this item inside of the directory given by destination 
        /// </summary>
        /// <param name="destination">The directory to copy into</param>
        public abstract void Copy(DirectoryInfo destination);
    }

    public class EnhancedDirectoryInfo : EnhancedFileSystemInfo
    {

        public DirectoryInfo Self
        {
            get { return OriginalFileSystemItem as DirectoryInfo; }
        }
        
        public EnhancedDirectoryInfo(DirectoryInfo directory) : 
            base(directory)
        {
            
        }

        public EnhancedDirectoryInfo(string path) : 
            this(new DirectoryInfo(path))
        {
            
        }
        
        public EnhancedFileSystemInfo[] Contents
        {
            get { return Self.GetContents(); }
        }
        
        public EnhancedFileInfo[] Files
        {
            get { return Self.GetFilesEnhanced(); }
        }

        public EnhancedDirectoryInfo[] Directories
        {
            get { return Self.GetDirectoriesEnhanced(); }
        }
        
        /// <summary>
        /// Recursively copies this directory inside of the directory given by destination 
        /// </summary>
        /// <param name="destination">The directory to copy into</param>
        public override void Copy(DirectoryInfo destination)
        {
            String pathOfCopy = System.IO.Path.Combine(destination.FullName, Name);
            DirectoryInfo directoryCopy = Directory.Exists(pathOfCopy) ? new DirectoryInfo(pathOfCopy) : Directory.CreateDirectory(pathOfCopy);

            DirectorySecurity security = Self.GetAccessControl();
            security.SetAccessRuleProtection(true, true);
            directoryCopy.SetAccessControl(security);

            EnhancedFileSystemInfo[] contents = Self.GetContents();
            
            foreach (EnhancedFileSystemInfo itemToCopy in contents)
            {
                itemToCopy.Copy(directoryCopy);
            }
        }
    }
    
    public class EnhancedFileInfo : EnhancedFileSystemInfo
    {
        protected FileInfo Self
        {
            get { return OriginalFileSystemItem as FileInfo; }
        }
        public BitmapImage Icon { get; }
        
        public sealed override string Path
        {
            get { return base.Path; }
        }

        public EnhancedFileInfo(FileInfo file) : 
            base(file)
        {
            Bitmap icon = System.Drawing.Icon.ExtractAssociatedIcon(Path)?.ToBitmap();
            Icon = icon.ConvertToImageSource();
        }

        public EnhancedFileInfo(string path) : 
            this(new FileInfo(path))
        {
            
        }
        
        public override void Copy(DirectoryInfo destination)
        {
            String pathOfCopy = System.IO.Path.Combine(destination.FullName, Name);
            
            FileSecurity originalSecurity = Self.GetAccessControl();
            originalSecurity.SetAccessRuleProtection(true, true);

            Self.CopyTo(pathOfCopy, true);

            var fileCopy = new FileInfo(pathOfCopy);

            // ReSharper disable once AssignNullToNotNullAttribute
            fileCopy.SetAccessControl(originalSecurity);
        }
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
        
        public static EnhancedFileSystemInfo[] GetContents(this DirectoryInfo directoryInfo)
        {
            var contents = new List<EnhancedFileSystemInfo>();

            foreach (EnhancedDirectoryInfo directory in directoryInfo.GetDirectoriesEnhanced())
            {
                contents.Add(directory);
            }

            foreach (EnhancedFileInfo file in directoryInfo.GetFilesEnhanced())
            {
                contents.Add(file);
            }

            return contents.ToArray();
        }
    }
}