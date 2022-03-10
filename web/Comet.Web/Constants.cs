using System.Reflection;

namespace Comet.Web
{
    public static class Constants
    {
        public static string Version => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}
