namespace Comet.Launcher.Server.States.Patches
{
    public struct UpdateStruct
    {
        public int From;
        public int To;

        public string FileName => IsBundle ? $"{From}-{To}" : $"{From}";
        public string Extension;
        public string FullFileName => $"{FileName}.{Extension}";

        public bool IsBundle => To == 0 || From != To;
        /// <summary>
        /// If the current patch is a updater client patch.
        /// </summary>
        public bool IsUpdate => From >= 10000;
    }
}
