using Moq;
using Microsoft.AspNetCore.Http;
using PROG7311_GLMS_ST10435542.Services;

namespace PROG7311_GLMS_ST10435542.Tests
{
    public class FileValidatorTests // tests if the file security rules are actually working correctly
    {
        [Fact]
        public void IsValidPdf_ValidPdf_ReturnsTrue() // tests if the file is a valid PDF
        {
            var mockFile = new Mock<IFormFile>();

            mockFile.Setup(f => f.FileName).Returns("contract.pdf");
            mockFile.Setup(f => f.Length).Returns(1024);

            Assert.True(FileValidator.IsValidPdf(mockFile.Object));
        }

        [Fact]
        public void IsValidPdf_ExeFile_ThrowsSecurityException() // tests if the file is an executable and should throw a security exception
        {
            var mockFile = new Mock<IFormFile>();

            mockFile.Setup(f => f.FileName).Returns("virus.exe");
            mockFile.Setup(f => f.Length).Returns(2048);

            var exception = Assert.Throws<InvalidOperationException>(() => FileValidator.IsValidPdf(mockFile.Object));

            Assert.Contains("Security Alert", exception.Message);
        }

        [Fact]
        public void IsValidPdf_WordDocument_ReturnsFalse() // tests if the file is a Word document
        {
            var mockFile = new Mock<IFormFile>();

            mockFile.Setup(f => f.FileName).Returns("document.docx");
            mockFile.Setup(f => f.Length).Returns(1024);

            Assert.False(FileValidator.IsValidPdf(mockFile.Object));
        }

        [Fact]
        public void IsValidPdf_NullFile_ReturnsFalse() // tests if the file is null
        {
            Assert.False(FileValidator.IsValidPdf(null));
        }

        [Fact]
        public void IsValidPdf_EmptyFile_ReturnsFalse() // tests if the file is empty
        {
            var mockFile = new Mock<IFormFile>();

            mockFile.Setup(f => f.FileName).Returns("empty.pdf");
            mockFile.Setup(f => f.Length).Returns(0); // 0 bytes

            Assert.False(FileValidator.IsValidPdf(mockFile.Object));
        }

        [Fact]
        public void IsValidPdf_NoExtension_ReturnsFalse() // tests if the file has no extension
        {
            var mockFile = new Mock<IFormFile>();

            mockFile.Setup(f => f.FileName).Returns("unknownfile");
            mockFile.Setup(f => f.Length).Returns(100);

            Assert.False(FileValidator.IsValidPdf(mockFile.Object));
        }
    }
}