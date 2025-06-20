using System;
using System.IO;

namespace SSD_Assignment___Banking_Application
{
    public static class ResourceChecker
    {
        public static bool HasSufficientDiskSpace(long requiredBytes = 1024 * 1024) // 1MB by default
        {
            try
            {
                string rootPath = Path.GetPathRoot(Environment.CurrentDirectory);
                DriveInfo drive = new DriveInfo(rootPath);
                return drive.AvailableFreeSpace >= requiredBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Disk space check failed: " + ex.Message);
                return false;
            }
        }
    }
}
