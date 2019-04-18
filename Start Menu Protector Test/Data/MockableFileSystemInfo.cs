using StartMenuProtector.Data;

namespace StartMenuProtectorTest.Data
{
    public class MockableEnhancedDirectoryInfo : EnhancedDirectoryInfo
    {
        public MockableEnhancedDirectoryInfo() : base(directory: null)
        {
        }
    }
    
    public class MockableEnhancedFileInfo : EnhancedFileInfo
    {
        public MockableEnhancedFileInfo() : base(file: null)
        {
        }
    }
}