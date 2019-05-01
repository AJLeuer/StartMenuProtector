using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Media.Imaging;
using Optional;
using Shell32;
using StartMenuProtector.Util;
using static StartMenuProtector.Configuration.Config;

namespace StartMenuProtector.Data 
{
    public interface IFileSystemItem : IEquatable<IFileSystemItem> 
    {
        FileSystemInfo OriginalFileSystemItem { get; set; }
        string Name { get; }

        /// <summary>
        /// The Name of the item without its file extension
        /// </summary>
        string PrettyName { get; }

        string Path { get; }
        string FullName { get; }
        bool Exists { get; }
        OwnerType OwnerType { get; }
        bool MarkedForExclusion { get; set; }
        bool Valid { get; }
        bool Filtered { get; }
        bool Equals(object @object);
        
        int GetHashCode();

        /// <summary>
        /// Copies this item inside of the directory given by destination 
        /// </summary>
        /// <param name="destination">The directory to copy into</param>
        void Copy(Directory destination);

        void Move(Directory destination);
        void Delete();
    }
    
    public interface IDirectory : IFileSystemItem 
    {
        DirectoryInfo Self { get; }
        List<IFile> Files { get; }
        List<IDirectory> Directories { get; }
        List<IFileSystemItem> Contents { get; }
        List<IFileSystemItem> RefreshContents();
        void DeleteContents();
        
        bool Contains(FileSystemItem item);
        ICollection<IFileSystemItem> FindMatchingItems(Func<IFileSystemItem, bool> matcher);

        /// <summary>
        /// Returns the first immediate subdirectory of this directory that matches name.
        /// If none exists, returns an empty optional. Does not search recursively.
        /// </summary>
        Option<Directory> GetSubdirectory(String name);
    }

    public interface IFile : IFileSystemItem
    {
        Option<FileSystemItem> GetShortcutTarget();
    }
    
    public abstract class FileSystemItem : FileSystemInfo, IFileSystemItem 
    {
        protected static ulong IDs = 0;
        public FileSystemInfo OriginalFileSystemItem { get; set; }

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

        public bool Valid
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
        
        public static bool operator == (FileSystemItem left, IFileSystemItem right)
        {
            return AreEqual(right, left);
        }

        public static bool operator != (FileSystemItem left, IFileSystemItem right)
        {
            return (!(left == right));
        }
        
        public static bool AreEqual(IFileSystemItem firstItem, IFileSystemItem secondItem)
        {
            if (ReferenceEquals(firstItem, null) || ReferenceEquals(secondItem, null))
            {
                return false;
            }

            return firstItem.Path == secondItem.Path;
        }
        
        public virtual bool Equals(IFileSystemItem item)
        {
            return this == item;
        }
        
        public override bool Equals(object @object)
        {
            if ((@object != null) && (@object.IsOfType<IFileSystemItem>()))
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
    
    public class Directory : FileSystemItem, IDirectory 
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

        private List<IFile> files = null;
        
        public virtual List<IFile> Files 
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

        private List<IDirectory> directories = null;
        public virtual List<IDirectory> Directories 
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

        public object ContentsAccessLock { get; } = new Object();
        
        private List<IFileSystemItem> contents = null;
        public virtual List<IFileSystemItem> Contents 
        {
            get
            {
                lock (ContentsAccessLock)
                {
                    if (contents == null)
                    {
                        InitializeContents();
                    }
                
                    return contents;   
                }
            }
        }
        
        private void InitializeContents()
        {
            lock (ContentsAccessLock)
            {
                var currentContents = new List<IFileSystemItem>();
                var subdirectories = new List<IDirectory>(Self.GetDirectoriesEnhanced());
                var currentFiles = new List<IFile>(Self.GetFilesEnhanced());

                foreach (IDirectory subdirectory in subdirectories)
                {
                    if (subdirectory is Directory childDirectory)
                    {
                        childDirectory.InitializeContents();
                    }
                }

                currentContents.AddAll(subdirectories);
                currentContents.AddAll(currentFiles);

                files = currentFiles;
                directories = subdirectories;
                contents = currentContents;
            }
        }
        
        public virtual List<IFileSystemItem> RefreshContents() 
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

                lock (ContentsAccessLock)
                {
                    files.ReplaceAll(currentFiles);
                    directories.ReplaceAll(subdirectories);
                    contents.ReplaceAll(currentContents);
                }
            }
            
            return Contents;
        }

        public virtual void DeleteContents()
        {
            lock (ContentsAccessLock)
            {
                foreach (IFileSystemItem fileSystemItem in Contents)
                {
                    fileSystemItem.Delete();
                }
            
                contents.Clear();
            }
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
                lock (destination.ContentsAccessLock)
                {
                    String pathOfCopy = System.IO.Path.Combine(destination.FullName, Name);
                    Directory directoryCopy = System.IO.Directory.Exists(pathOfCopy) ? new Directory(pathOfCopy) : new Directory(System.IO.Directory.CreateDirectory(pathOfCopy));

                    DirectorySecurity security = Self.GetAccessControl();
                    security.SetAccessRuleProtection(true, true);
                    directoryCopy.Self.SetAccessControl(security);

                    foreach (IFileSystemItem itemToCopy in Contents)
                    {
                        itemToCopy.Copy(directoryCopy);
                    }
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
                foreach (IDirectory subdirectory in Directories)
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

        public ICollection<IFileSystemItem> FindMatchingItems(Func<IFileSystemItem, bool> matcher)
        {
            var matchingItems = new HashSet<IFileSystemItem>();

            foreach (IDirectory subdirectory in Directories)
            {
                IEnumerable<IFileSystemItem> matches = subdirectory.FindMatchingItems(matcher);
                matchingItems.AddAll(matches);
            }

            foreach (IFileSystemItem item in Contents)
            {
                if (matcher(item))
                {
                    matchingItems.Add(item);
                }
            }

            return matchingItems;
        }

        /// <summary>
        /// Returns the first immediate subdirectory of this directory that matches name.
        /// If none exists, returns an empty optional. Does not search recursively.
        /// </summary>
        public Option<Directory> GetSubdirectory(String name)
        {
            foreach (IDirectory directory in Directories)
            {
                if ((directory.Name == name) && (directory is Directory subdirectory))
                {
                    return Option.Some(subdirectory);
                }
            }

            return Option.None<Directory>();
        }

        /// <summary>
        /// Recursively searches for any items in test that differ from those in sourceOfTruth. Items present in
        /// sourceOfTruth but absent in test are returned as part of removed, items discovered in test not present
        /// in sourceOfTruth are returned as part of added. If the item is a directory, it is returned with all its
        /// contents present.
        /// </summary>
        /// <param name="sourceOfTruth"></param>
        /// <param name="test"></param>
        /// <returns></returns>
        public static (ICollection<RelocatableItem> added, ICollection<RelocatableItem> removed) FindDivergences(IDirectory sourceOfTruth, IDirectory test)
        {
            ICollection<RelocatableItem> added = FindAddedItems(sourceOfTruth, test), removed = FindRemovedItems(sourceOfTruth, test);

            return (added, removed);
        }

        private static ICollection<RelocatableItem> FindRemovedItems(IDirectory sourceOfTruth, IDirectory test)
        {
            ICollection<RelocatableItem> removed = FindUnexpectedItems(expected: test, test: sourceOfTruth);
            return removed;
        }

        private static ICollection<RelocatableItem> FindAddedItems(IDirectory sourceOfTruth, IDirectory test)
        {
            ICollection<RelocatableItem> added = FindUnexpectedItems(expected: sourceOfTruth, test: test);
            return added;
        }

        private static ICollection<RelocatableItem> FindUnexpectedFiles<FileItemType>(IEnumerable<FileItemType> expected, IEnumerable<FileItemType> test) where FileItemType : IFile
        {
            ICollection<RelocatableItem> unexpectedFiles = new HashSet<RelocatableItem>();
            IEnumerable<FileItemType> expectedFiles = expected.ToList();
            
            foreach (var file in test)
            {
                bool matchFound = false;
                
                foreach (var expectedFile in expectedFiles)
                {
                    if (expectedFile.Name == file.Name)
                    {
                        matchFound = true;
                        break;
                    }
                }

                if (matchFound == false)
                {
                    unexpectedFiles.Add(new RelocatableItem(file));
                }
            }

            return unexpectedFiles;
        }
        
        private static ICollection<RelocatableItem> FindUnexpectedItems(IDirectory expected, IDirectory test)
        {
            ICollection<RelocatableItem> unexpectedItems = new HashSet<RelocatableItem>();

            if (expected.Name != test.Name)
            {
                unexpectedItems.Add(new RelocatableItem(test));
                return unexpectedItems;
            }
            
            unexpectedItems.AddAll(FindUnexpectedFiles(expected: expected.Files, test: test.Files));

            foreach (var directory in test.Directories)
            {
                bool matchFound = false;

                foreach (var expectedDirectory in expected.Directories)
                {
                    if (expectedDirectory.Name == directory.Name)
                    {
                        matchFound = true;
                        unexpectedItems.AddAll(FindUnexpectedItems(expected: expectedDirectory, test: directory));
                        break;
                    }        
                }

                if (matchFound == false)
                {
                    unexpectedItems.Add(new RelocatableItem(directory));
                }
            }

            return unexpectedItems;
        }
    }
    
    public class File : FileSystemItem, IFile 
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
                lock (destination.ContentsAccessLock)
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

            try
            {
                if (folderItem != null)
                {
                    var link = (ShellLinkObject)folderItem.GetLink;
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
            }
            catch (Exception)
            {
                return Option.None<FileSystemItem>();
            }

            return Option.None<FileSystemItem>();
        }
    }

    /// <summary>
    /// A FileSystemItem that records its original location in the file system, allowing it to be returned there
    /// </summary>
    public class RelocatableItem : IFileSystemItem
    {
        public String OriginalPath { get; }
        public IFileSystemItem UnderlyingItem { get; }
        public FileSystemInfo OriginalFileSystemItem
        {
            get { return UnderlyingItem.OriginalFileSystemItem;}
            set { UnderlyingItem.OriginalFileSystemItem = value; }
        }
        public string Name
        {
            get { return UnderlyingItem.Name; }
        }
        public string PrettyName
        {
            get { return UnderlyingItem.PrettyName; }
        }
        
        public string Path
        {
            get { return UnderlyingItem.Path; }
        }
        
        public string FullName
        {
            get { return UnderlyingItem.FullName; }
        }
        public bool Exists
        {
            get { return UnderlyingItem.Exists; }
        }
        public OwnerType OwnerType
        {
            get { return UnderlyingItem.OwnerType; }
        }
        public bool MarkedForExclusion
        {
            get { return UnderlyingItem.MarkedForExclusion;}
            set { UnderlyingItem.MarkedForExclusion = value; } 
        }
        public bool Valid
        {
            get { return UnderlyingItem.Valid; }
        }
        public bool Filtered
        {
            get
            {
                return UnderlyingItem.Filtered;
            }
        }

        public RelocatableItem(IFileSystemItem underlyingItem)
        {
            this.UnderlyingItem = underlyingItem;
            this.OriginalPath = UnderlyingItem.Path;
        }
        
        public bool Equals(IFileSystemItem other)
        {
            return UnderlyingItem.Equals(other);
        }

        public void Copy(Directory destination)
        {
            UnderlyingItem.Copy(destination);
        }

        public void Move(Directory destination)
        {
            UnderlyingItem.Move(destination);
        }

        public void MoveToOriginalPath()
        {
            Move(new Directory(OriginalPath));
        }

        public void Delete()
        {
            UnderlyingItem.Delete();
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

                if (enhancedDirectory.Filtered == false)
                {
                    enhancedDirectories.Add(enhancedDirectory);
                }
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
                
                if (enhancedFile.Filtered == false)
                {
                    enhancedFiles.Add(enhancedFile);
                }
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