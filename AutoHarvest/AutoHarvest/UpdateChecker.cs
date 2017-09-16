using StardewModdingAPI;
using System.Net;
using System.Threading.Tasks;

namespace AutoHarvest
{
    class UpdateChecker
    {
        public static void CheckUpdate(IMonitor monitor)
        {
            Task.Run(() =>
            {
                try
                {
                    string latest_version = new WebClient().DownloadString("https://raw.githubusercontent.com/pomepome/AutoHarvest/master/current_version.txt");
                    if (!IsLatest(latest_version, ModEntry.VERSION))
                    {
                        monitor.Log(string.Format("New version of AutoHarvest available! Consider updating version:{0} -> {1}.", ModEntry.VERSION, latest_version), LogLevel.Alert);
                    }
                    else
                    {
                        monitor.Log(string.Format("Your AutoHarvest(version:{0}) is up to date.", ModEntry.VERSION), LogLevel.Alert);
                    }
                }
                catch (WebException ex)
                {
                    monitor.Log("Update Checker couldn't download latest version info! Message:" + ex.Message, LogLevel.Error);
                }
            });
        }

        private static bool IsLatest(string latest, string current)
        {
            string[] splitLatest = latest.Split(".".ToCharArray());
            string[] splitCurrent = current.Split(".".ToCharArray());

            int.TryParse(splitLatest[0], out int major_latest);
            int.TryParse(splitCurrent[0], out int major_current);
            int.TryParse(splitLatest[1], out int minor_latest);
            int.TryParse(splitCurrent[1], out int minor_current);
            int.TryParse(splitLatest[2], out int patch_latest);
            int.TryParse(splitCurrent[2], out int patch_current);

            if (major_latest > major_current || minor_latest > minor_current || patch_latest > patch_current)
            {
                return false;
            }
            return true;
        }
    }
}
