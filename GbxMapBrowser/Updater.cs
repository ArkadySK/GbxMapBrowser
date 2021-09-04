using GitHubUpdate;
using System;
using System.Threading.Tasks;

namespace GbxMapBrowser
{
    public static class Updater
    {
        public async static Task<bool> IsUpToDate()
        {
            var checker = new UpdateChecker("arkadySK", "GbxMapBrowser");

            UpdateType update = await checker.CheckUpdate();
            if (update == UpdateType.None) // up to date
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void DownloadUpdate()
        {
            var checker = new UpdateChecker("arkadySK", "GbxMapBrowser");
            checker.DownloadAsset("GbxMapBrowser.zip"); // opens it in the user's browser
        }
    }
}
