using System.Security.Cryptography;

namespace Comet.Launcher.Files.Helpers
{
    public static class StringHelper
    {
        public static string GetMd5(this string path)
        {
            try
            {
                using var md5 = MD5.Create();
                using FileStream stream = File.OpenRead(path);
                return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}