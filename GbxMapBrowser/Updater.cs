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
        private GitHubClient _github;
        string _repositoryOwner;
        string _repositoryName;
        Version _currentVersion;
        Version _latestVersion;

        public Updater()
        {
            _repositoryOwner = "ArkadySK";
            _repositoryName = "GbxMapBrowser";
            _currentVersion = GetCurrentVersion();

            _github = new GitHubClient(new ProductHeaderValue(_repositoryName + @"-UpdateCheck"));
            _releaseClient = _github.Repository.Release;
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
            if (String.IsNullOrWhiteSpace(_repositoryName) || String.IsNullOrWhiteSpace(_repositoryOwner)) return null;

            var allReleases = await _releaseClient.GetAll(_repositoryOwner, _repositoryName);
            var latestRelease = allReleases.FirstOrDefault(release => !release.Prerelease && 
                                                                    (RemoveV(release.TagName) > _currentVersion));
            if (latestRelease != null)
                _latestVersion = RemoveV(latestRelease.TagName);
            else
                _latestVersion = _currentVersion;
            return _latestVersion;
        }

        public async Task<bool> IsUpToDate()
        {
            Version newVersion = await GetNewVersion();

            return (newVersion == _currentVersion);       
        }

        public void DownloadUpdate()
        {
            const string urlTemplate = "https://github.com/{0}/{1}/releases/download/{2}/{3}";
            var url = string.Format(urlTemplate, _repositoryOwner, _repositoryName, "v" + _latestVersion, "GbxMapBrowser.zip");
            
            url = url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });

        }
    }
}
