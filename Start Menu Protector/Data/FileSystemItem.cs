using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Media.Imaging;
using Optional;
using Shell32;
using StartMenuProtector.Util;
using static StartMenuProtector.Configuration.Config;

namespace StartMenuProtector.Data 
{
    public abstract class FileSystemItem : FileSystemInfo, IEquatable<FileSystemItem> 
    {
        protected static ulong IDs = 0;
        protected FileSystemInfo OriginalFileSystemItem { get; set; }

        public readonly ulong ID = IDs++;

        public override string Name 
        {
            get { return OriginalFileSystemItem.Name; }
        }

        /// <summary>
        /// The Name of the item without its file extension
        /// </summary>
        public virtual string PrettyName
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

        public abstract OwnerType OwnerType { get; }

        public bool MarkedForExclusion { get; set; } = false;
        protected bool Valid
        {
            get
            {
                if (MarkedForExclusion)
                {
                    return false;
                }
                else if (Filtered)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public bool Filtered 
        {
            get
            {
                bool filtered = false;
                
                foreach (Func<FileSystemItem,bool> filter in FileSystemItemFilters)
                {
                    if (filter.Invoke(this))
                    {
                        filtered = true;
                        break;
                    }
                }

                return filtered;
            }
        }

        protected FileSystemItem(FileSystemInfo originalFileSystemItem)
        {
            OriginalFileSystemItem = originalFileSystemItem;
        }
        
        /// <summary>
        /// If fileSystemItem is already an instance of one of the FileSystemItem subclasses
        /// (that is, if it's an instance of either File or Directory), Create() merely returns
        /// the argument fileSystemItem unchanged. However, if fileSystemItem's an instance of one of the
        /// built-in FileSystemInfo types (FileInfo or DirectoryInfo), Create() wraps it in
        /// either a new Directory or a new File, and returns that.
        /// </summary>
        public static FileSystemItem Create(FileSystemInfo fileSystemItem)
        {
            switch (fileSystemItem)
            {
                case File file:
                    return file;
                case Directory directory:
                    return directory;
                case FileInfo file:
                    return new File(file);
                case DirectoryInfo directory:
                    return new Directory(directory);
                default:
                    throw new ArgumentException("Unknown subtype of FileSystemInfo");
            }
        }
        
        public static bool operator == (FileSystemItem left, FileSystemItem right)
        {
            return left?.Path == right?.Path;
        }

        public static bool operator != (FileSystemItem left, FileSystemItem right)
        {
            return (!(left == right));
        }

        public bool Equals(FileSystemItem item)
        {
            return this == item;
        }
        
        public override bool Equals(object @object)
        {
            if ((@object != null) && (@object.IsOfType<FileSystemItem>()))
            {
                return this.Equals((FileSystemItem) @object);
            }
            else
            {
                return false;
            }
        }
        
        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        /// <summary>
        /// Copies this item inside of the directory given by destination 
        /// </summary>
        /// <param name="destination">The directory to copy into</param>
        public abstract void Copy(Directory destination);

        public virtual void Move(Directory destination)
        {
            Copy(destination);
            Delete();
        }
        
        public override void Delete() { OriginalFileSystemItem.Delete(); }
    }

    public class Directory : FileSystemItem 
    {

        public virtual DirectoryInfo Self
        {
            get { return OriginalFileSystemItem as DirectoryInfo; }
        }
        
        public override OwnerType OwnerType
        {
            get
            {
                DirectorySecurity security = Self.GetAccessControl();
                IdentityReference owner = security.GetOwner(typeof(SecurityIdentifier)).Translate(typeof(NTAccount));
                return owner.Value;
            }
        }
        
        public Directory(DirectoryInfo directory) : 
            base(directory)
        {
            
        }

        public Directory(string path) : 
            this(new DirectoryInfo(path))
        {
            
        }

        private List<File> files = null;
        
        public List<File> Files 
        {
            get
            {
                if (files == null)
                {
                    InitializeContents();
                }
                
                return files;
            }
        }

        private List<Directory> directories = null;
        public List<Directory> Directories 
        {
            get
            {
                if (directories == null)
                {
                    InitializeContents();
                }
                
                return directories;
            }
        }

        private readonly object contentsAccessLock = new Object();
        private List<FileSystemItem> contents = null;
        public virtual List<FileSystemItem> Contents 
        {
            get
            {
                if (contents == null)
                {
                    InitializeContents();
                }
                
                return contents;
            }
        }
        
        private void InitializeContents()
        {
            lock (contentsAccessLock)
            {
                var currentContents = new List<FileSystemItem>();
                var subdirectories = new List<Directory>(Self.GetDirectoriesEnhanced());
                var currentFiles = new List<File>(Self.GetFilesEnhanced());

                foreach (Directory subdirectory in subdirectories)
                {
                    subdirectory.InitializeContents();
                }

                currentContents.AddAll(subdirectories);
                currentContents.AddAll(currentFiles);

                files = currentFiles;
                directories = subdirectories;
                contents = currentContents;
            }
        }
        
        public virtual List<FileSystemItem> RefreshContents() 
        {
            if ((files == null) || (directories == null) || (contents == null))
            {
                InitializeContents();
            }
            else
            {
                var currentContents = new List<FileSystemItem>();
                var subdirectories = new List<Directory>(Self.GetDirectoriesEnhanced());
                var currentFiles = new List<File>(Self.GetFilesEnhanced());

                foreach (Directory subdirectory in subdirectories)
                {
                    subdirectory.RefreshContents();
                }

                currentContents.AddAll(subdirectories);
                currentContents.AddAll(currentFiles);

                files.ReplaceAll(currentFiles);
                directories.ReplaceAll(subdirectories);
                contents.ReplaceAll(currentContents);
            }
            
            return Contents;
        }

        public virtual void DeleteContents()
        {
            foreach (FileSystemItem fileSystemItem in Contents)
            {
                fileSystemItem.Delete();
            }
            
            contents.Clear();
        }
        
        public override void Delete()
        {
            Self.Delete(true);
        }
        
        /// <summary>
        /// Recursively copies this directory inside of the directory given by destination 
        /// </summary>
        /// <param name="destination">The directory to copy into</param>
        public override void Copy(Directory destination)
        {
            if (Valid)
            {
                String pathOfCopy = System.IO.Path.Combine(destination.FullName, Name);
                Directory directoryCopy = System.IO.Directory.Exists(pathOfCopy) ? new Directory(pathOfCopy) : new Directory(System.IO.Directory.CreateDirectory(pathOfCopy));

                DirectorySecurity security = Self.GetAccessControl();
                security.SetAccessRuleProtection(true, true);
                directoryCopy.Self.SetAccessControl(security);

                foreach (FileSystemItem itemToCopy in Contents)
                {
                    itemToCopy.Copy(directoryCopy);
                }
            }
        }
        
        public virtual bool Contains(FileSystemItem item)
        {
            bool contained = false;
            
            if (Contents.Contains(item))
            {
                contained = true;
            }
            else
            {
                foreach (Directory subdirectory in Directories)
                {
                    if (subdirectory.Contains(item))
                    {
                        contained = true;
                        break;
                    }
                }
            }

            return contained;
        }
    }
    
    public class File : FileSystemItem 
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
        
        public override OwnerType OwnerType 
        {
            get
            {
                FileSecurity security = Self.GetAccessControl();
                IdentityReference ownerID = security.GetOwner(typeof(SecurityIdentifier));
                
                try
                {
                    IdentityReference owner = ownerID.Translate(typeof(NTAccount));
                    return owner.Value;
                }
                catch (IdentityNotMappedException)
                {
                    return ownerID.Value;
                }
            }
        }

        public File(FileInfo file) : 
            base(file)
        {
            if (file != null)
            {
                Bitmap icon = System.Drawing.Icon.ExtractAssociatedIcon(Path)?.ToBitmap();
                Icon = icon.ConvertToImageSource();
                Icon.Freeze();
            }
        }
        
        public File(string path) : 
            this(new FileInfo(path))
        {
            
        }

        public override void Copy(Directory destination)
        {
            if (Valid)
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
        
        /// <returns>If this is a shortcut, returns the FileSystemItem this points to.
        /// Otherwise, returns an empty optional.</returns>
        public Option<FileSystemItem> GetShortcutTarget()
        {
            string pathOnly = System.IO.Path.GetDirectoryName(Path);
            string filenameOnly = System.IO.Path.GetFileName(Path);

            var shell = new Shell();
            Folder folder = shell.NameSpace(pathOnly);
            FolderItem folderItem = folder.ParseName(filenameOnly);
            
            if (folderItem != null)
            {
                ShellLinkObject link = (ShellLinkObject)folderItem.GetLink;
                FileAttributes targetItemAttributes = System.IO.File.GetAttributes(link.Path);

                if ((targetItemAttributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    return Option.Some<FileSystemItem>(new Directory(link.Path));
                }
                else /* if is file */
                {
                    return Option.Some<FileSystemItem>(new File(link.Path));
                }
            }

            return Option.None<FileSystemItem>();
        }
    }

    internal static class DirectoryInfoExtensions 
    {
        public static Directory[] GetDirectoriesEnhanced(this DirectoryInfo directoryInfo)
        {
            DirectoryInfo[] directories = directoryInfo.GetDirectories();
            var enhancedDirectories = new List<Directory>();
            
            foreach (DirectoryInfo directory in directories)
            {
                var enhancedDirectory = new Directory(directory);
                enhancedDirectories.Add(enhancedDirectory);
            }

            return enhancedDirectories.ToArray();
        }
        
        public static File[] GetFilesEnhanced(this DirectoryInfo directoryInfo)
        {
            FileInfo[] files = directoryInfo.GetFiles();
            var enhancedFiles = new List<File>();
            
            foreach (FileInfo file in files)
            {
                var enhancedFile = new File(file);
                enhancedFiles.Add(enhancedFile);
            }

            return enhancedFiles.ToArray();
        }
        
        public static FileSystemItem[] GetContents(this DirectoryInfo directoryInfo)
        {
            var contents = new List<FileSystemItem>();

            foreach (Directory directory in directoryInfo.GetDirectoriesEnhanced())
            {
                contents.Add(directory);
            }

            foreach (File file in directoryInfo.GetFilesEnhanced())
            {
                contents.Add(file);
            }

            return contents.ToArray();
        }
    }

    public class OwnerType 
    {
        public static System OS { get; } = new System();
        public static Administrator Admin { get; } = new Administrator();

        public class System : OwnerType 
        {
            public const String Name = @"NT AUTHORITY\SYSTEM";
            public override String Value { get { return Name; } }
        }
        
        public class Administrator : OwnerType
        {
            public const String Name = @"BUILTIN\Administrators";
            public override String Value { get { return Name; } }
        }

        public virtual String Value { get; }
        
        protected OwnerType()
        {
        }
        
        private OwnerType(string value)
        {
            this.Value = value;
        }
        
        public static implicit operator OwnerType(string value)
        {
            switch (value)
            {
                case System.Name:
                    return OS;
                case Administrator.Name:
                    return Admin;
                default:
                    return new OwnerType(value);
            }
        }
        
        public static implicit operator String(OwnerType type)
        {
            return type.Value;
        }
        
    }
}