using System.Collections.Generic;
using System.IO;
using StartMenuProtector.Data;
using StartMenuProtector.Util;
using Directory = StartMenuProtector.Data.Directory;

namespace StartMenuProtector.Models
{
    public class StartMenuDirectory : Directory, IStartMenuItem
    {
        public override bool Valid 
        {
            get 
            {
                if (MarkedForExclusion)
                {
                    return false;
                }

                return base.Valid;
            }
        }
        
        public bool MarkedForExclusion { get; set; } = false;

        public StartMenuDirectory(DirectoryInfo directoryInfo) : 
            base(directoryInfo)
        {
            
        }

        public StartMenuDirectory(IDirectory directory) :
            this(directory.Self)
        {
            
        }

        public StartMenuDirectory(string path) : 
            this(new DirectoryInfo(path))
        {
            
        }
        
        protected override void InitializeContents()
        {
            lock (ContentsAccessLock)
            {
                base.InitializeContents();
                
                var startingContents = new List<IFileSystemItem>(contents);

                contents.Clear();
                directories.Clear();
                files.Clear();

                foreach (IFileSystemItem item in startingContents)
                {
                    if (item is IFile file)
                    {
                        var startMenuShortcut = new StartMenuFile(file);
                        files.Add(startMenuShortcut);
                    }
                    else if (item is IDirectory directory)
                    {
                        var startMenuDirectory = new StartMenuDirectory(directory);
                        startMenuDirectory.InitializeContents();
                        directories.Add(startMenuDirectory);
                    }
                }
                
                contents.Clear();
                contents.AddAll(files);
                contents.AddAll(directories);
            }
        }
    }
}