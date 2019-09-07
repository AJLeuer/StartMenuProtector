using System.IO;
using System.Windows;
using StartMenuProtector.Data;
using File = StartMenuProtector.Data.File;

namespace StartMenuProtector.ViewModel
{
    public class StartMenuFile : File, IStartMenuFile
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

        private bool focused = default;
        private bool selected = default;


        public bool IsFocused
        {
            get
            {
                return focused;
            }
            set
            {
                focused = value;
                if (focused)
                {
                    Focused?.Invoke(this, null);
                }

                IsSelected = IsFocused;
            }
        }
        
        public bool IsSelected
        {
            get { return IsFocused || selected; }
            set
            {
                selected = value;

                if (IsSelected)
                {
                    Selected?.Invoke(this, null);
                }
                else
                {
                    Deselected?.Invoke(this, null);
                }
            }
        }
        public bool MarkedForExclusion { get; set; } = false;
        public event RoutedEventHandler Focused;
        public event RoutedEventHandler Selected;
        public event RoutedEventHandler Deselected;

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