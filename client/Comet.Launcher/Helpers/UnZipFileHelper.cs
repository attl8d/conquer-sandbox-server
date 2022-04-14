using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace Comet.Launcher.Helpers
{
    internal static class UnZipFileHelper
    {
        public delegate Task ProgressCallback(int currentFile, int totalFiles, string fileName);

        public static async Task<bool> UnZipAsync(string path, string destination, ProgressCallback callback = null)
        {
            try
            {
                using IArchive archive = ArchiveFactory.Open(path);
                int count = 0;
                int amount = archive.Entries.Count(x => !x.IsDirectory);
                int tick = Environment.TickCount;
                using IReader reader = archive.ExtractAllEntries();

                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        reader.WriteEntryToDirectory(destination, new ExtractionOptions
                        {
                            ExtractFullPath = true,
                            Overwrite = true,
                            PreserveFileTime = true,
                            PreserveAttributes = true
                        });
                        count++;

                        if (callback != null && Environment.TickCount - tick > 100)
                        {
                            await callback.Invoke(count, amount, reader.Entry.Key);
                            tick = Environment.TickCount;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                if (callback != null)
                    await callback.Invoke(-1, -1, ex.ToString());
                return false;
            }
        }
    }
}