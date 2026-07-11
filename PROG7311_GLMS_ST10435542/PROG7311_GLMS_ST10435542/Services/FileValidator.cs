namespace PROG7311_GLMS_ST10435542.Services
{
    // Frontend copy of the Part 2 file validator: uploads are checked here before the file ever
    // touches the server disk. (The frontend cannot reference the API project's validator anymore
    // because the projects are fully decoupled.)
    public static class FileValidator
    {
        public static bool IsValidPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }

            var extension = Path.GetExtension(file.FileName)?.ToLower();

            if (extension == ".exe") // block executables outright - these could carry malware
            {
                throw new InvalidOperationException("Security Alert: Only PDF files are allowed.");
            }

            return extension == ".pdf";
        }
    }
}
