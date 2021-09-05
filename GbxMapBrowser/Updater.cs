using Octokit;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GbxMapBrowser
{
    public class Updater
    {
        private IReleasesClient _releaseClient;
        private GitHubClient Github;
        string RepositoryOwner;
        string RepositoryName;
        Version CurrentVersion;
        Version LatestVersion;

        public Updater()
        {
            RepositoryOwner = "ArkadySK";
            RepositoryName = "GbxMapBrowser";
            CurrentVersion = GetCurrentVersion();

            Github = new GitHubClient(new ProductHeaderValue(RepositoryName + @"-UpdateCheck"));
            _releaseClient = Github.Repository.Release;
        }

        private Version RemoveV(string version)
        {
            return new Version(version.Replace("v", ""));
        }

        private Version GetCurrentVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        private async Task<Version> GetNewVersion()
        {
            if (String.IsNullOrWhiteSpace(RepositoryName) || String.IsNullOrWhiteSpace(RepositoryOwner)) return null;

            var allReleases = await _releaseClient.GetAll(RepositoryOwner, RepositoryName);
            var latestRelease = allReleases.FirstOrDefault(release => !release.Prerelease && 
                                                                    (RemoveV(release.TagName) > CurrentVersion));
            if (latestRelease != null)
                LatestVersion = RemoveV(latestRelease.TagName);
            else
                LatestVersion = CurrentVersion;
            return LatestVersion;
        }

        public async Task<bool> IsUpToDate()
        {
            Version newVersion = await GetNewVersion();

            return (newVersion == CurrentVersion);       
        }

        public void DownloadUpdate()
        {
            const string urlTemplate = "https://github.com/{0}/{1}/releases/download/{2}/{3}";
            var url = string.Format(urlTemplate, RepositoryOwner, RepositoryName, "v" + LatestVersion, "GbxMapBrowser.zip");
            
            url = url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });

        }
    }
}
