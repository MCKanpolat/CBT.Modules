﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace CBT.NuGet.Internal
{
    internal class FileUtilities
    {
        // Need to find a proper directory copy routine.
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool overWriteFiles)
        {
            if (!Directory.Exists(sourceDirName))
            {
                throw new DirectoryNotFoundException("Source Directory does not exist or could not be found: " + sourceDirName);
            }
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, overWriteFiles);
            }
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs, overWriteFiles);
                }
            }
        }

        public static void DirectoryRemove(string sourceDirName, string destDirName, bool recurseSubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source Directory does not exist or could not be found: " + sourceDirName);
            }
            DirectoryInfo[] dirs = dir.GetDirectories();

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                if (File.Exists(tempPath))
                {
                    File.SetAttributes(tempPath, File.GetAttributes(tempPath) & ~FileAttributes.ReadOnly);
                    File.Delete(tempPath);
                }
            }
            if (recurseSubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryRemove(subdir.FullName, temppath, recurseSubDirs);
                }
            }
        }

        public static void AcquireMutex(Mutex mutex)
        {
            try
            {
                bool owner = mutex.WaitOne(); // Perhaps specify a timeout value so builds don't hang.
                if (!owner)
                    throw new TimeoutException("Timeout waiting for mutex");
            }
            catch (AbandonedMutexException)
            {
                // Unlikely we care.
                Trace.TraceWarning("mutex for Aggregate package was abandoned.");
            }
        }
    }
}
