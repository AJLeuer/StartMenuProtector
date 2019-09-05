using StartMenuProtector.Data;
using StartMenuProtector.ViewModel;

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
}