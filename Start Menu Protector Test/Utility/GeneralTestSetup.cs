using System.Collections.Generic;
using Moq;
using StartMenuProtector.Data;

namespace StartMenuProtectorTest.Utility
{
	public static class GeneralTestSetup
	{
		public static Mock<MockableStartMenuDirectory> CreateStartMenuDirectoryMockWithContents()
		{
			var directoryMock = new Mock<MockableStartMenuDirectory> {CallBase = true};
			
			var testDirectoryFileMock = new Mock<MockableStartMenuFile> {CallBase = true};
			var testDirectorySubdirectoryPartialMock = new Mock<MockableStartMenuDirectory> {CallBase = true};
			var subDirectoryFiles = new List<IFile>();
			var subDirectoryContents = new List<IFileSystemItem>();


			for (ushort i = 0; i < 3; i++)
			{
				var subdirectoryFileMock = new Mock<MockableStartMenuFile> {CallBase = true};
				subdirectoryFileMock.Setup((MockableStartMenuFile self) => self.FullName).Returns($"StartMenuDirectory/Subdirectory/File{i}");
				subDirectoryFiles.Add(subdirectoryFileMock.Object);
				subDirectoryContents.Add(subdirectoryFileMock.Object);
			}

			testDirectorySubdirectoryPartialMock.Setup((MockableStartMenuDirectory self) => self.Path).Returns("StartMenuDirectory/Subdirectory");
			testDirectoryFileMock.Setup((MockableStartMenuFile self) => self.FullName).Returns("StartMenuDirectory/File");
			directoryMock.Setup((MockableStartMenuDirectory self) => self.Path).Returns("StartMenuDirectory");

			testDirectorySubdirectoryPartialMock.Setup((MockableStartMenuDirectory self) => self.Files).Returns(subDirectoryFiles);
			testDirectorySubdirectoryPartialMock.Setup((MockableStartMenuDirectory self) => self.Directories).Returns(new List<IDirectory>());
			testDirectorySubdirectoryPartialMock.Setup((MockableStartMenuDirectory self) => self.Contents).Returns(subDirectoryContents);
			directoryMock.Setup((MockableStartMenuDirectory self) => self.Files).Returns(new List<IFile> {testDirectoryFileMock.Object});
			directoryMock.Setup((MockableStartMenuDirectory self) => self.Directories).Returns(new List<IDirectory> {testDirectorySubdirectoryPartialMock.Object});
			directoryMock.Setup((MockableStartMenuDirectory self) => self.Contents).Returns(new List<IFileSystemItem> {testDirectoryFileMock.Object, testDirectorySubdirectoryPartialMock.Object});

			return directoryMock;
		}
	}
}