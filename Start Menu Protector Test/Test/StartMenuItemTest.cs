using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using StartMenuProtector.Data;
using StartMenuProtector.ViewModel;
using StartMenuProtectorTest.Utility;

namespace StartMenuProtectorTest.Test
{
	public static class StartMenuItemTest
	{
		/* partially mocking the class under test (usually discouraged) in order to smooth out some of the complications
		/  that come along with filesystem IO */ 
		public static Mock<MockableStartMenuDirectory> DirectoryPartialMock = new Mock<MockableStartMenuDirectory> { CallBase = true };

		[SetUp]
		public static void Setup()
		{
			var testDirectoryFileMock = new Mock<MockableStartMenuFile> { CallBase = true };
			var testDirectorySubdirectoryPartialMock = new Mock<MockableStartMenuDirectory> { CallBase = true };
			var subDirectoryFiles = new List<IFile>();
			var subDirectoryContents = new List<IFileSystemItem> { };

			
			for (ushort i = 0; i < 3; i++)
			{
				var subdirectoryFileMock = new Mock<MockableStartMenuFile> { CallBase = true };
				subdirectoryFileMock.Setup((MockableStartMenuFile self) => self.FullName).Returns($"StartMenuDirectory/Subdirectory/File{i}");
				subDirectoryFiles.Add(subdirectoryFileMock.Object);
				subDirectoryContents.Add(subdirectoryFileMock.Object);
			}
			
			testDirectorySubdirectoryPartialMock.Setup((MockableStartMenuDirectory self) => self.Path).Returns("StartMenuDirectory/Subdirectory");
			testDirectoryFileMock.Setup((MockableStartMenuFile self) => self.FullName).Returns("StartMenuDirectory/File");
			DirectoryPartialMock.Setup((MockableStartMenuDirectory self) => self.Path).Returns("StartMenuDirectory");

			testDirectorySubdirectoryPartialMock.Setup((MockableStartMenuDirectory self) => self.Files).Returns(subDirectoryFiles);
			testDirectorySubdirectoryPartialMock.Setup((MockableStartMenuDirectory self) => self.Directories).Returns(new List<IDirectory>());
			testDirectorySubdirectoryPartialMock.Setup((MockableStartMenuDirectory self) => self.Contents).Returns(subDirectoryContents);
			DirectoryPartialMock.Setup((MockableStartMenuDirectory self) => self.Files).Returns(new List<IFile> { testDirectoryFileMock.Object });
			DirectoryPartialMock.Setup((MockableStartMenuDirectory self) => self.Directories).Returns(new List<IDirectory> { testDirectorySubdirectoryPartialMock.Object });
			DirectoryPartialMock.Setup((MockableStartMenuDirectory self) => self.Contents).Returns(new List<IFileSystemItem> { testDirectoryFileMock.Object, testDirectorySubdirectoryPartialMock.Object });
		}
		
		[Test]
		public static void DirectorySelectedEventShouldPropogateToAllStartMenuViewsRecursively()
		{
			StartMenuDirectory directory = DirectoryPartialMock.Object;

			directory.IsSelected = true;
			
			foreach (IFileSystemItem item in directory.GetFlatContents())
			{
				if (item is IStartMenuItem startMenuItem)
				{
					Assert.True(startMenuItem.IsSelected);
				}
			}
		}
		
		
	}
}