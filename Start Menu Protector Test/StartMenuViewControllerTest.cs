using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using StartMenuProtector.Control;
using StartMenuProtector.Data;

namespace StartMenuProtectorTest
{
    public class MockableEnhancedDirectoryInfo : EnhancedDirectoryInfo
    {
        public MockableEnhancedDirectoryInfo() : base(directory: null)
        {
        }
    }
    
    public static class StartMenuViewControllerTest
    {
        public static SystemStateController MockSystemStateController = new Mock<SystemStateController>().Object;
        public static Mock<StartMenuDataController> DataControllerMock = new Mock<StartMenuDataController>(MockSystemStateController);
        public static Mock<MockableEnhancedDirectoryInfo> SystemProgramsMock = new Mock<MockableEnhancedDirectoryInfo>(); 
        public static Mock<MockableEnhancedDirectoryInfo> UserProgramsMock = new Mock<MockableEnhancedDirectoryInfo>();
        
        public static Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo> ActiveProgramShortcuts =
            new Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo>
            {
                {StartMenuShortcutsLocation.System, SystemProgramsMock.Object},
                {StartMenuShortcutsLocation.User, UserProgramsMock.Object}
            };

        [SetUp]
        public static void Setup()
        {
            SystemProgramsMock.Setup(
                    (self) => self.Contents)
                .Returns(new EnhancedFileSystemInfo[]{});
            
            UserProgramsMock.Setup(
                    (self) => self.Contents)
                .Returns(new EnhancedFileSystemInfo[]{});
            
            DataControllerMock.Setup(
                (self) => self.ProgramShortcuts)
                .Returns(ActiveProgramShortcuts);
        }
        
        [Test]
        public static void ShouldSetCurrentShortcutsToUsersWhenUserIsSelected()
        {
            var mockDataController = DataControllerMock.Object;
            var viewController = new StartMenuViewController(mockDataController, MockSystemStateController) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };

            viewController.UpdateCurrentShortcuts(StartMenuShortcutsLocation.System);

            Assert.AreEqual(ActiveProgramShortcuts[StartMenuShortcutsLocation.System], viewController.CurrentShortcutsDirectory);
        }
    }
}