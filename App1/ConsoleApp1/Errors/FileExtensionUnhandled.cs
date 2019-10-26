using System;

namespace ConsoleApp1.Errors
{
    class FileExtensionUnhandled:Exception
    {
        public FileExtensionUnhandled(string filePath) : base($"File extension unhandled: {filePath}")
        {
        }
    }
}
