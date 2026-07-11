using Microsoft.AspNetCore.Http;
using System;
using System.IO;

namespace PROG7311_GLMS_API.Services
{
    public static class FileValidator // this class is used to double check that uploaded files are safe and correct
    {
        public static bool IsValidPdf (IFormFile file) // this method returns true if the file is a valid pdf, and false if it is not
        {
            if (file == null || file.Length == 0) // first we check if the file actually exists or if it is completely empty
            {
                return false;
            }
            
            var extension = Path.GetExtension(file.FileName)?.ToLower();
            
            if (extension == ".exe") // this is a security check to block executable files which could contain viruses
            {
                throw new InvalidOperationException("Security Alert: Only PDF files are allowed.");
            } 
                
            if (extension != ".pdf") // if the file type is anything other than a .pdf, we reject it
            {
                return false;
            }

            return true;
        }
    }
}