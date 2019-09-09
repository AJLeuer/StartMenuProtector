using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using Optional;
using StartMenuProtector.Util;

namespace StartMenuProtector.Data 
{
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
    
    public interface IDirectory : IFileSystemItem 
    {
        DirectoryInfo Self { get; }
        List<IFile> Files { get; }
        List<IDirectory> Directories { get; }
        
        Object ContentsAccessLock { get; }
        List<IFileSystemItem> Contents { get; }

        ICollection<IFileSystemItem> GetFlatContents();
        List<IFileSystemItem> RefreshContents();
        
        /// <summary>
        /// Copies the contents of this directory into of the directory given by destination 
        /// </summary>
        /// <param name="destination">The directory to copy into</param>
        void CopyContents(IDirectory destination);
        
        void DeleteContents();
        
        bool Contains(IFileSystemItem item);
        ICollection<IFileSystemItem> FindMatchingItems(Func<IFileSystemItem, bool> matcher);

        /// <summary>
        /// Returns the first immediate subdirectory of this directory that matches name.
        /// If none exists, returns an empty optional. Does not search recursively.
        /// </summary>
        Option<IDirectory> GetSubdirectory(string name);
    }
    
    public class Directory : FileSystemItem, IDirectory 
    {
        public virtual DirectoryInfo Self 
        {
            get { return OriginalFileSystemItem as DirectoryInfo; }
        }

        public override bool Exists
        {
            get { return System.IO.Directory.Exists(Path); }
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
        
        
        protected List<IFile> files = null;
        
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

        protected List<IDirectory> directories = null;
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
        
        protected List<IFileSystemItem> contents = null;
        
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

        public Directory(DirectoryInfo directory) : 
            base(directory)
        {
            
        }

        public Directory(string path) : 
            this(new DirectoryInfo(path))
        {
            
        }

        protected virtual void InitializeContents()
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

        public ICollection<IFileSystemItem> GetFlatContents()
        {
            var flatContents = new HashSet<IFileSystemItem>();

            foreach (IDirectory subDirectory in Directories)
            {
                ICollection<IFileSystemItem> subdirectoryContents = subDirectory.GetFlatContents();
                flatContents.AddAll(subdirectoryContents);
            }

            foreach (IFileSystemItem item in Contents)
            {
                flatContents.Add(item);
            }

            return flatContents;
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
            SetAttributesRecursively(FileAttributes.Normal);
            Self.Delete(true);
        }

        /// <summary>
        /// Recursively copies this directory inside of the directory given by destination 
        /// </summary>
        /// <param name="destination">The directory to copy into</param>
        public override Option<IFileSystemItem> Copy(IDirectory destination)
        {
            lock (destination.ContentsAccessLock)
            {
                return Copy(destination.FullName);
            }
        }
        
        public override Option<IFileSystemItem> Copy(string path)
        {
            if (Valid)
            {
                String pathOfCopy = System.IO.Path.Combine(path, Name);
                Directory directoryCopy = System.IO.Directory.Exists(pathOfCopy) ? new Directory(pathOfCopy) : new Directory(System.IO.Directory.CreateDirectory(pathOfCopy));

                DirectorySecurity security = Self.GetAccessControl();
                security.SetAccessRuleProtection(true, true);
                directoryCopy.Self.SetAccessControl(security);

                CopyContents(directoryCopy);
                
                return Option.Some<IFileSystemItem>(new Directory(pathOfCopy));
            }

            return Option.None<IFileSystemItem>();
        }
        
        
        public void CopyContents(IDirectory destination)
        {
            foreach (IFileSystemItem itemToCopy in Contents)
            {
                itemToCopy.Copy(destination);
            }
        }

        public virtual bool Contains(IFileSystemItem item)
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
        public Option<IDirectory> GetSubdirectory(string name)
        {
            foreach (IDirectory directory in Directories)
            {
                if ((directory.Name == name) && (directory is Directory subdirectory))
                {
                    return Option.Some<IDirectory>(subdirectory);
                }
            }

            return Option.None<IDirectory>();
        }

        private void SetAttributesRecursively(FileAttributes attributes)
        {
            if (Self.Exists)
            {
                SetAttributes(Self);
            }    

            void SetAttributes(DirectoryInfo dir)
            {
                foreach (var subDir in dir.GetDirectories())
                {
                    SetAttributes(subDir);
                    subDir.Attributes = attributes;
                }
                foreach (var file in dir.GetFiles())
                {
                    file.Attributes = attributes;
                }
            }
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
}