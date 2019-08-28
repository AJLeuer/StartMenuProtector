using System.IO;
using StartMenuProtector.Data;
using File = StartMenuProtector.Data.File;

namespace StartMenuProtector.Models
{
    public class StartMenuFile : File, IStartMenuItem
    {
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