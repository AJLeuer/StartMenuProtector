using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Shell32;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Media.Imaging;
using Optional;
using StartMenuProtector.Util;
using static StartMenuProtector.Configuration.Config;

namespace StartMenuProtector.Data 
{
    public abstract class EnhancedFileSystemInfo : FileSystemInfo
    {
        protected FileSystemInfo OriginalFileSystemItem { get; set; }

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
                
                foreach (Func<EnhancedFileSystemInfo,bool> filter in FileSystemItemFilters)
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

        protected EnhancedFileSystemInfo(FileSystemInfo originalFileSystemItem)
        {
            OriginalFileSystemItem = originalFileSystemItem;
        }

        public static EnhancedFileSystemInfo Create(FileSystemInfo fileSystemItem)
        {
            switch (fileSystemItem)
            {
                case EnhancedFileInfo file:
                    return file;
                case EnhancedDirectoryInfo directory:
                    return directory;
                case FileInfo file:
                    return new EnhancedFileInfo(file);
                case DirectoryInfo directory:
                    return new EnhancedDirectoryInfo(directory);
                default:
                    throw new ArgumentException("Unknown subtype of FileSystemInfo");
            }
        }
        
        /// <summary>
        /// Copies this item inside of the directory given by destination 
        /// </summary>
        /// <param name="destination">The directory to copy into</param>
        public abstract void Copy(EnhancedDirectoryInfo destination);

        public virtual void Move(EnhancedDirectoryInfo destination)
        {
            Copy(destination);
            Delete();
        }
        
        public override void Delete() { OriginalFileSystemItem.Delete(); }
    }

    public class EnhancedDirectoryInfo : EnhancedFileSystemInfo
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
        
        public EnhancedDirectoryInfo(DirectoryInfo directory) : 
            base(directory)
        {
            
        }

        public EnhancedDirectoryInfo(string path) : 
            this(new DirectoryInfo(path))
        {
            
        }
        
        public virtual EnhancedFileSystemInfo[] Contents
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
        public override void Copy(EnhancedDirectoryInfo destination)
        {
            if (Valid)
            {
                String pathOfCopy = System.IO.Path.Combine(destination.FullName, Name);
                EnhancedDirectoryInfo directoryCopy = Directory.Exists(pathOfCopy) ? new EnhancedDirectoryInfo(pathOfCopy) : new EnhancedDirectoryInfo(Directory.CreateDirectory(pathOfCopy));

                DirectorySecurity security = Self.GetAccessControl();
                security.SetAccessRuleProtection(true, true);
                directoryCopy.Self.SetAccessControl(security);

                EnhancedFileSystemInfo[] contents = Self.GetContents();
            
                foreach (EnhancedFileSystemInfo itemToCopy in contents)
                {
                    itemToCopy.Copy(directoryCopy);
                }
            }
        }

        public override void Delete()
        {
            Self.Delete(true);
        }

        public virtual void DeleteContents()
        {
            foreach (EnhancedFileSystemInfo fileSystemItem in Contents)
            {
                fileSystemItem.Delete();
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

        public EnhancedFileInfo(FileInfo file) : 
            base(file)
        {
            if (file != null)
            {
                Bitmap icon = System.Drawing.Icon.ExtractAssociatedIcon(Path)?.ToBitmap();
                Icon = icon.ConvertToImageSource();
            }
        }
        
        public EnhancedFileInfo(string path) : 
            this(new FileInfo(path))
        {
            
        }

        public override void Copy(EnhancedDirectoryInfo destination)
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
        public Option<EnhancedFileSystemInfo> GetShortcutTarget()
        {
            string pathOnly = System.IO.Path.GetDirectoryName(Path);
            string filenameOnly = System.IO.Path.GetFileName(Path);

            var shell = new Shell();
            Folder folder = shell.NameSpace(pathOnly);
            FolderItem folderItem = folder.ParseName(filenameOnly);
            
            if (folderItem != null)
            {
                ShellLinkObject link = (ShellLinkObject)folderItem.GetLink;
                FileAttributes targetItemAttributes = File.GetAttributes(link.Path);

                if ((targetItemAttributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    return Option.Some<EnhancedFileSystemInfo>(new EnhancedDirectoryInfo(link.Path));
                }
                else /* if is file */
                {
                    return Option.Some<EnhancedFileSystemInfo>(new EnhancedFileInfo(link.Path));
                }
                    
            }

            return Option.None<EnhancedFileSystemInfo>();
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