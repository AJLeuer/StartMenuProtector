using System.IO;
using StartMenuProtector.ViewModel;
using Directory = StartMenuProtector.Data.Directory;
using File = StartMenuProtector.Data.File;

namespace StartMenuProtectorTest.Utility
{
    public class MockableDirectory : Directory
    {
        public MockableDirectory() : base(directory: null)
        {
        }
    }
    
    public class MockableStartMenuDirectory : StartMenuDirectory
    {
        public MockableStartMenuDirectory() : base(directoryInfo: null)
        {
            
        }
    }
    
    public class MockableFile : File
    {
        public MockableFile() : base(file: null)
        {
        }
    }
    
    public class MockableStartMenuFile : StartMenuFile
    {
        public MockableStartMenuFile() : base(file: (FileInfo) null)
        {
            
        }
    }
}