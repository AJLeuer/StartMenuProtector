using StartMenuProtector.Data;

namespace StartMenuProtectorTest.Utility
{
    public class MockableDirectory : Directory
    {
        public MockableDirectory() : base(directory: null)
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