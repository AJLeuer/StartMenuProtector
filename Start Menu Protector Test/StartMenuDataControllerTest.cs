using System.Collections.Generic;
using System.IO;
using Moq;
using NUnit.Framework;
using StartMenuProtector.Control;
using StartMenuProtector.Data;
using StartMenuProtectorTest.Data;

namespace StartMenuProtectorTest
{
    public static class StartMenuDataControllerTest
    {
        public static SystemStateController MockSystemStateController = new Mock<SystemStateController>().Object;
        public static Mock<MockableEnhancedDirectoryInfo> SystemProgramsMock = new Mock<MockableEnhancedDirectoryInfo>(); 
        public static Mock<MockableEnhancedDirectoryInfo> UserProgramsMock = new Mock<MockableEnhancedDirectoryInfo>();
        public static Mock<MockableEnhancedFileInfo> MockFileToBeSaved;
        public static ICollection<FileSystemInfo> FilesToSave;

        [SetUp]
        public static void Setup()
        {
            MockFileToBeSaved = new Mock<MockableEnhancedFileInfo>();
            
            FilesToSave = new List<FileSystemInfo>
            {
                MockFileToBeSaved.Object,
                MockFileToBeSaved.Object,
                MockFileToBeSaved.Object
            };
            
            StartMenuDataController.SavedProgramShortcuts = new Dictionary<StartMenuShortcutsLocation, EnhancedDirectoryInfo>
            {
                {StartMenuShortcutsLocation.System, SystemProgramsMock.Object},
                {StartMenuShortcutsLocation.User, UserProgramsMock.Object}
            };
            
            SystemProgramsMock.Setup(
                    (self) => self.Self)
                .Returns((DirectoryInfo) null);
            
            UserProgramsMock.Setup(
                    (self) => self.Self)
                .Returns((DirectoryInfo) null);

            MockFileToBeSaved.Setup(
                (self) => self.Copy(It.IsAny<EnhancedDirectoryInfo>()));
        }

        [Test]
        public static void ActiveStartMenuDataControllerShouldSaveAllRequestedStartMenuShortcuts()
        {
            StartMenuDataController controller = new ActiveStartMenuDataController(MockSystemStateController);

            controller.SaveProgramShortcuts(StartMenuShortcutsLocation.System, FilesToSave);
            
            MockFileToBeSaved.Verify((self) => self.Copy(SystemProgramsMock.Object), Times.Exactly(3));
        }
        
        [Test]
        public static void ShouldSaveStartMenuShortcutsFromSpecifiedLocationOnly()
        {
            StartMenuDataController controller = new ActiveStartMenuDataController(MockSystemStateController);

            controller.SaveProgramShortcuts(StartMenuShortcutsLocation.User, FilesToSave);
            
            MockFileToBeSaved.Verify((self) => self.Copy(UserProgramsMock.Object), Times.AtLeastOnce);
            MockFileToBeSaved.Verify((self) => self.Copy(SystemProgramsMock.Object), Times.Never);
        }
        
        [Test]
        public static void SavedStartMenuDataControllerShouldDoNothingWhenRequestedToSaveStartMenuShortcuts()
        {
            StartMenuDataController controller = new SavedStartMenuDataController(MockSystemStateController);

            controller.SaveProgramShortcuts(StartMenuShortcutsLocation.System, FilesToSave);
            
            MockFileToBeSaved.Verify((self) => self.Copy(SystemProgramsMock.Object), Times.Never);
        }
    }
}