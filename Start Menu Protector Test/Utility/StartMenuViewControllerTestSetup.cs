using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Moq;
using StartMenuProtector.Control;
using StartMenuProtector.Data;

namespace StartMenuProtectorTest.Utility
{
    public static class StartMenuViewControllerTestSetup
    {
        public static readonly Mock<MockableFile> SystemStartMenuItemMock = new Mock<MockableFile>(); 
        public static readonly Mock<MockableFile> UserStartMenuItemMock   = new Mock<MockableFile>();

        public static readonly List<IFileSystemItem> ActiveSystemStartMenuItems = new List<IFileSystemItem> { SystemStartMenuItemMock.Object, SystemStartMenuItemMock.Object, SystemStartMenuItemMock.Object };
        public static readonly List<IFileSystemItem> ActiveUserStartMenuItems   = new List<IFileSystemItem> { UserStartMenuItemMock.Object,   UserStartMenuItemMock.Object,   UserStartMenuItemMock.Object   };
        public static readonly List<IFileSystemItem> SavedSystemStartMenuItems  = new List<IFileSystemItem> { SystemStartMenuItemMock.Object, SystemStartMenuItemMock.Object, SystemStartMenuItemMock.Object };
        public static readonly List<IFileSystemItem> SavedUserStartMenuItems    = new List<IFileSystemItem> { UserStartMenuItemMock.Object,   UserStartMenuItemMock.Object,   UserStartMenuItemMock.Object   };

        public static Task<IDirectory> CreateStubbedStartMenuContentsRetrievalTask(StartMenuProtectorViewType view, StartMenuShortcutsLocation location)
        {
            var directoryMock = new Mock<MockableDirectory>();

            switch (view) 
            {
                case StartMenuProtectorViewType.Active:
                {
                    if (location == StartMenuShortcutsLocation.System)
                    {
                        directoryMock.Setup((self) => self.Contents).Returns(ActiveSystemStartMenuItems);
                        directoryMock.Setup((self) => self.RefreshContents()).Returns(ActiveSystemStartMenuItems);
                    }
                    else /* if (location == StartMenuShortcutsLocation.User) */
                    {
                        directoryMock.Setup((self) => self.Contents).Returns(ActiveUserStartMenuItems);
                        directoryMock.Setup((self) => self.RefreshContents()).Returns(ActiveUserStartMenuItems);
                    }
                    break;
                }
                case StartMenuProtectorViewType.Saved:
                {
                    if (location == StartMenuShortcutsLocation.System)
                    {
                        directoryMock.Setup((self) => self.Contents).Returns(SavedSystemStartMenuItems);
                        directoryMock.Setup((self) => self.RefreshContents()).Returns(SavedSystemStartMenuItems);
                    }
                    else /* if (location == StartMenuShortcutsLocation.User) */
                    {
                        directoryMock.Setup((self) => self.Contents).Returns(SavedUserStartMenuItems);
                        directoryMock.Setup((self) => self.RefreshContents()).Returns(SavedUserStartMenuItems);
                    }
                    break;
                }
                case StartMenuProtectorViewType.Quarantine:
                {
                    throw new InvalidEnumArgumentException("Quarantine view not implemented yet");
                }
                case StartMenuProtectorViewType.Excluded:
                {
                    throw new InvalidEnumArgumentException("Excluded view not implemented yet");
                }
                default:
                {
                    throw new InvalidEnumArgumentException();
                }
            }

            return Task.Run(() =>
            {
                return (IDirectory) directoryMock.Object;
            });
        }
    }
}