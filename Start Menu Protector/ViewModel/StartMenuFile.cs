using System.IO;
using System.Windows;
using StartMenuProtector.Data;
using File = StartMenuProtector.Data.File;

namespace StartMenuProtector.ViewModel
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

        public bool IsSelected { get; set; }
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

        public void HandleFocusChange(object sender, RoutedEventArgs eventInfo)
        {
        }

    }
}