using System;
using System.IO;
using StartMenuProtector.Data;
using File = StartMenuProtector.Data.File;

namespace StartMenuProtector.Models
{
    public class StartMenuFile : File, IStartMenuItem
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

        public StartMenuFile(FileInfo file) : 
            base(file)
        {
            
        }
        
        public StartMenuFile(IFile file) :
            this(file.Self)
        {
            
        }
        
        public StartMenuFile(string path) : 
            this(new FileInfo(path))
        {
            
        }
    }
}