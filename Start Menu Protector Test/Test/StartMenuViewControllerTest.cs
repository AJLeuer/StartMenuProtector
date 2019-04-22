using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using StartMenuProtector.Control;
using StartMenuProtector.Data;
using StartMenuProtectorTest.Utility;
using static StartMenuProtectorTest.Utility.StartMenuViewControllerTestSetupUtility;

namespace StartMenuProtectorTest.Test
{
    public static class StartMenuViewControllerTest
    {
        public static SystemStateService MockSystemStateService = new Mock<SystemStateService>().Object;
        public static Mock<ActiveStartMenuDataService> ActiveDataServiceMock;
        public static Mock<SavedStartMenuDataService>  SavedDataServiceMock;
        

        [SetUp]
        public static void Setup()
        {
            ActiveDataServiceMock = new Mock<ActiveStartMenuDataService>(MockSystemStateService);
            SavedDataServiceMock = new Mock<SavedStartMenuDataService>(MockSystemStateService);

            ActiveDataServiceMock.Setup(
                (self) => self.GetStartMenuContents(It.IsAny<StartMenuShortcutsLocation>()))
                .Returns((StartMenuShortcutsLocation location) => { return CreateStubbedStartMenuContentsRetrievalTask(view: StartMenuProtectorViewType.Active, location: location); });

            ActiveDataServiceMock
                .Setup((self) => self.SaveStartMenuItems(It.IsAny<StartMenuShortcutsLocation>(), It.IsAny<IEnumerable<FileSystemInfo>>()));
        
            SavedDataServiceMock.Setup(
                    (self) => self.GetStartMenuContents(It.IsAny<StartMenuShortcutsLocation>()))
                .Returns((StartMenuShortcutsLocation location) => { return CreateStubbedStartMenuContentsRetrievalTask(view: StartMenuProtectorViewType.Saved, location: location); });

            SavedDataServiceMock
                .Setup((self) => self.SaveStartMenuItems(It.IsAny<StartMenuShortcutsLocation>(), It.IsAny<IEnumerable<FileSystemInfo>>()));
        }
        
        [Test]
        public static void ShouldSetCurrentShortcutsToUsersWhenUserIsSelected()
        {
            var activeViewController = new ActiveStartMenuViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };
            var savedViewController  = new SavedStartMenuViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };

            activeViewController.StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.System;
            savedViewController.StartMenuStartMenuShortcutsLocation  = StartMenuShortcutsLocation.User;
            
            activeViewController.UpdateCurrentShortcuts().Wait();
            savedViewController.UpdateCurrentShortcuts().Wait();

            CollectionAssert.AreEquivalent(ActiveSystemStartMenuItems, activeViewController.StartMenuContents);
            CollectionAssert.AreEquivalent(SavedUserStartMenuItems,    savedViewController.StartMenuContents);
        }
        
        [Test]
        public static void ActiveStartMenuViewControllerShouldSaveUserShortcuts()
        {
            var viewController = new ActiveStartMenuViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };

            viewController.SaveCurrentShortcuts();
            
            SavedDataServiceMock.Verify((self) => self.SaveStartMenuItems(viewController.StartMenuStartMenuShortcutsLocation, viewController.StartMenuContents), Times.Once());
        }


        [Test]
        public static void SavedStartMenuViewControllerShouldDoNothingWhenRequestedToSaveStartMenuShortcuts()
        {
            var viewController = new SavedStartMenuViewController(ActiveDataServiceMock.Object, SavedDataServiceMock.Object, MockSystemStateService) { StartMenuStartMenuShortcutsLocation = StartMenuShortcutsLocation.User };

            viewController.SaveCurrentShortcuts();
            
            ActiveDataServiceMock.Verify((self) => self.SaveStartMenuItems(viewController.StartMenuStartMenuShortcutsLocation, viewController.StartMenuContents), Times.Never);
        }
    }
}