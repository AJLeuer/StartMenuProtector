using System;
using System.Drawing;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Media.Imaging;
using Optional;
using Shell32;
using StartMenuProtector.Util;

namespace StartMenuProtector.Data 
{
    public interface IFile : IFileSystemItem 
    {
        FileInfo Self { get; }
        Option<FileSystemItem> GetShortcutTarget();
    }
    
    public class File : FileSystemItem, IFile 
    {
        private BitmapImage icon = null;
        public BitmapImage Icon 
        {
            get
            {
                if ((icon == null) && (OriginalFileSystemItem != null))
                {
                    Bitmap iconBitMap = System.Drawing.Icon.ExtractAssociatedIcon(Path)?.ToBitmap();
                    icon = iconBitMap.ConvertToImageSource();
                    icon.Freeze();
                }

                return icon;
            }
        }

        public FileInfo Self 
        {
            get { return OriginalFileSystemItem as FileInfo; }
        }

        public sealed override string Path 
        {
            get { return base.Path; }
        }
        
        public override bool Exists
        {
            get { return System.IO.File.Exists(Path); }
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
        }
        
        public File(string path) : 
            this(new FileInfo(path))
        {
            
        }
        
        public override Option<IFileSystemItem> Copy(IDirectory destination)
        {
            lock (destination.ContentsAccessLock)
            {
                return Copy(destination.Path);
            }
        }

        public override Option<IFileSystemItem> Copy(String path)
        {
            if (Valid)
            {
                String pathOfCopy = System.IO.Path.Combine(path, Name);
        
                FileSecurity originalSecurity = Self.GetAccessControl();
                originalSecurity.SetAccessRuleProtection(true, true);

                Self.CopyTo(pathOfCopy, true);

                var fileCopy = new FileInfo(pathOfCopy);

                // ReSharper disable once AssignNullToNotNullAttribute
                fileCopy.SetAccessControl(originalSecurity);

                return Option.Some<IFileSystemItem>(new File(pathOfCopy));
            }
            
            return Option.None<IFileSystemItem>();
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
}