using System.Threading.Tasks;
using Comet.Shared;

namespace Comet.Launcher.Server
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            Log.DefaultFileName = "LauncherServer";
        }
    }
}