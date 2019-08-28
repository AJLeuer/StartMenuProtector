using System;
using System.IO;
using Optional;
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
        String ParentDirectoryPath { get; }
        bool Equals(object @object);
        
        int GetHashCode();

        /// <summary>
        /// Copies this item inside of the directory given by destination 
        /// </summary>
        /// <param name="destination">The directory to copy into</param>
        /// <returns>The new file created from the copy operation</returns>
        Option<IFileSystemItem> Copy(IDirectory destination);

        /// <summary>
        /// Copies this item inside of the directory given by destination 
        /// </summary>
        /// <param name="path">The path of the directory to copy into</param>
        /// <returns>The new file created from the copy operation</returns>
        Option<IFileSystemItem> Copy(string path);

        /// <summary>
        /// Copies this item to the path specified, deletes this,
        /// and returns the new file created from the copy operation
        /// </summary>
        /// <param name="destination"></param>
        /// <returns>The new file created from the copy operation</returns>
        Option<IFileSystemItem> Move(IDirectory destination);

        /// <summary>
        /// Copies this item to the path specified, deletes this,
        /// and returns the new file created from the copy operation
        /// </summary>
        /// <param name="path"></param>
        /// <returns>The new file created from the copy operation</returns>
        Option<IFileSystemItem> Move(string path);
        
        void Delete();
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

        public virtual string ParentDirectoryPath
        {
            get
            {
                return System.IO.Path.GetDirectoryName(Path);
            }
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

        public abstract override bool Exists { get; }

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
        public virtual Option<IFileSystemItem> Copy(IDirectory destination)
        {
            return Copy(destination.FullName);
        }

        public abstract Option<IFileSystemItem> Copy(string path);

        public virtual Option<IFileSystemItem> Move(IDirectory destination)
        {
            return Move(destination.FullName);
        }

        public Option<IFileSystemItem> Move(string path)
        {
            Option<IFileSystemItem> copy = Copy(path);
            Delete();
            return copy;
        }

        public override void Delete() { OriginalFileSystemItem.Delete(); }
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


        public string ParentDirectoryPath
        {
            get { return UnderlyingItem.ParentDirectoryPath; }
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
            if (underlyingItem.IsOfType<RelocatableItem>())
            {
                throw new ArgumentException("RelocatableItem cannot be created with an underlying RelocatableItem");
            }
            
            this.UnderlyingItem = underlyingItem;
            this.OriginalPath = UnderlyingItem.Path;
        }
        
        public bool Equals(IFileSystemItem other)
        {
            return UnderlyingItem.Equals(other);
        }

        public Option<IFileSystemItem> Copy(IDirectory destination)
        {
            return UnderlyingItem.Copy(destination);
        }
        
        public Option<IFileSystemItem> Copy(String path)
        {
            return UnderlyingItem.Copy(path);
        }

        public Option<IFileSystemItem> Move(IDirectory destination)
        {
            return UnderlyingItem.Move(destination);
        }
        
        public Option<IFileSystemItem> Move(string path)
        {
            return UnderlyingItem.Move(path);
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