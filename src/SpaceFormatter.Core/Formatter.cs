using SpaceFormatter.Core.Utils;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SpaceFormatter.Core
{
    public class Formatter
    {
        private readonly Random random = new Random();

        public async Task Format(FormatterParameters parameters, IProgress<FormatterProgress> progress, CancellationToken cancellationToken)
        {
            string tempPath = parameters.Path + @"\Temp\";

            string driveName = Path.GetPathRoot(tempPath);

            long freeSpace = GetDriveFreeSpace(driveName);

            if (freeSpace == -1)
            {
                return;
            }
            else if (freeSpace == 0)
            {
                Log("There is no free disk space.");
                return;
            }

            CreateTempDirectory(tempPath);

            long count = (long)((ulong)freeSpace / parameters.Size) + 1;

            var timer = new EtcCalculator(count);

            byte[] bytes = new byte[parameters.Size];

            long i = 1;

            while (freeSpace > 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                // Check the space for the last file if the amount of free space has changed
                if (freeSpace < bytes.Length)
                {
                    bytes = new byte[freeSpace];
                    random.NextBytes(bytes);
                }
                else
                {
                    if (parameters.Random)
                    {
                        random.NextBytes(bytes);
                    }
                }

                Log($"Creating temp files... {i}\\{count}");
                await CreateTempFile(tempPath, i.ToString(), bytes).ConfigureAwait(false);

                progress.Report(new FormatterProgress(i / (double)count, count, i, timer.GetEtc(i)));

                Log($"Estimate time: {timer.GetEtc(i)}");

                freeSpace = GetDriveFreeSpace(driveName);
                i++;
            }

            if (parameters.Clear)
            {
                DeleteTempDirectory(tempPath);
            }

            progress.Report(new FormatterProgress(1, count, count, TimeSpan.Zero));
            Log("Done!");
        }

        private void CreateTempDirectory(string path)
        {
            try
            {
                Log("Creating temp directory...");

                // Determine whether the directory exists.
                if (Directory.Exists(path))
                {
                    Log($"The directory \"{path}\" already exists.");
                    return;
                }

                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(path);
                Log($"The directory \"{path}\" was created successfully.");
            }
            catch (Exception e)
            {
                Log($"The process failed: {e}");
            }
        }

        private void DeleteTempDirectory(string path)
        {
            try
            {
                Log("Deleting temp directory...");

                // Determine whether the directory exists.
                if (!Directory.Exists(path))
                {
                    return;
                }

                // Try to create the directory.
                Directory.Delete(path, true);
                Log($"The directory \"{path}\" was deleted successfully.");
            }
            catch (Exception e)
            {
                Log($"The process failed: {e}");
            }
        }

        private async Task CreateTempFile(string path, string name, byte[] bytes)
        {
            try
            {
                // Determine whether the directory exists.
                if (File.Exists(path + name))
                {
                    Log($"The file \"{path}\" already exists.");
                    Log($"Trying another name...");

                    name = Path.GetRandomFileName();

                    await CreateTempFile(path, name, bytes).ConfigureAwait(false);
                    return;
                }

                using (FileStream fs = new FileStream(path + name, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.None))
                {
                    await fs.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                }

#if DEBUG
                Log($"The file \"{path + name}\" was created successfully.");
#endif
            }
            catch (Exception e)
            {
                Log($"The process failed: {e}");
            }
        }

        private long GetDriveFreeSpace(string driveName)
        {
            try
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (drive.Name == driveName)
                    {
                        if (drive.IsReady)
                        {
                            return drive.AvailableFreeSpace;
                        }
                        else
                        {
                            Log($"The drive \"{driveName}\" is't ready.");
                            return -1;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log($"The process failed: {e}");
            }

            Log($"The drive \"{driveName}\" not found.");
            return -1;
        }

        private void Log(string message)
        {
            System.Diagnostics.Debug.WriteLine($"{DateTime.Now.TimeOfDay} - {message}");
        }
    }
}