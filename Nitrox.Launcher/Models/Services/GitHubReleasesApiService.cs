using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Nitrox.Launcher.Models.Attributes;
using Nitrox.Model.Constants;
using Nitrox.Model.Core;

namespace Nitrox.Launcher.Models.Services;

/// <summary>
///     Update source backed by GitHub Releases of the configured repository (<see cref="NitroxConstants.GITHUB_RELEASES_REPOSITORY" />).
///     Maps a GitHub release onto the same <see cref="NitroxWebsiteApiService.NitroxRelease" /> shape as the website API so the
///     existing update/download flow can consume either source transparently.
/// </summary>
[HttpService]
internal sealed class GitHubReleasesApiService
{
    private readonly HttpClient httpClient;
    private readonly HttpFileService httpFileService;

    public GitHubReleasesApiService(HttpClient httpClient, HttpFileService httpFileService)
    {
        this.httpClient = httpClient;
        this.httpFileService = httpFileService;
        httpClient.BaseAddress = new Uri("https://api.github.com/");
        // GitHub's REST API rejects requests without a User-Agent.
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"{NitroxConstants.LAUNCHER_APP_NAME}/{NitroxEnvironment.Version}");
        httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
    }

    public async Task<NitroxWebsiteApiService.NitroxRelease?> GetNitroxLatestVersionAsync(CancellationToken cancellationToken = default)
    {
        GitHubRelease? release = await httpClient.GetFromJsonAsync<GitHubRelease>($"repos/{NitroxConstants.GITHUB_RELEASES_REPOSITORY}/releases/latest", cancellationToken);
        return MapToNitroxRelease(release);
    }

    /// <summary>
    ///     Gets a downloader for the latest GitHub release asset matching the current platform/architecture.
    /// </summary>
    public async Task<HttpFileService.FileDownloader?> GetLatestNitroxAsync(CancellationToken cancellationToken = default)
    {
        if (await GetNitroxLatestVersionAsync(cancellationToken) is not { CurrentPlatformInfo: { } downloadInfo })
        {
            return null;
        }

        return await httpFileService.GetFileAsync(downloadInfo.DownloadUrl, cancellationToken);
    }

    private static NitroxWebsiteApiService.NitroxRelease? MapToNitroxRelease(GitHubRelease? release)
    {
        if (release is null || release.Draft || !Version.TryParse((release.TagName ?? "").TrimStart('v', 'V'), out Version? version))
        {
            return null;
        }

        Dictionary<string, NitroxWebsiteApiService.PlatformInfo>? platforms = null;
        if (FindLauncherAsset(release.Assets) is { BrowserDownloadUrl: { } url } asset)
        {
            platforms = new Dictionary<string, NitroxWebsiteApiService.PlatformInfo>
            {
                [NitroxEnvironment.PlatformName] = new()
                {
                    Architectures = new Dictionary<string, NitroxWebsiteApiService.ArchitectureInfo>
                    {
                        [NitroxEnvironment.ArchitectureName] = new()
                        {
                            DownloadUrl = url,
                            Md5Hash = "",
                            // GitHub exposes a per-asset content digest ("sha256:..."); use it for integrity verification.
                            // Empty when unavailable, in which case the download flow skips hashing (HTTPS still applies).
                            Sha256Hash = ParseSha256Digest(asset.Digest),
                            FileSize = (asset.Size / 1024d / 1024d).ToString("F1")
                        }
                    }
                }
            };
        }

        return new NitroxWebsiteApiService.NitroxRelease { Version = version, Platforms = platforms };
    }

    /// <summary>
    ///     Finds the full launcher package (e.g. <c>Nitrox-win-x64-1.9.1.zip</c>) for the running platform, excluding the
    ///     standalone server package.
    /// </summary>
    private static GitHubRelease.Asset? FindLauncherAsset(GitHubRelease.Asset[]? assets)
    {
        if (assets is null)
        {
            return null;
        }

        string runtimeIdentifier = $"{NitroxEnvironment.PlatformName switch { "windows" => "win", "macos" => "osx", _ => "linux" }}-{NitroxEnvironment.ArchitectureName}";
        return assets.FirstOrDefault(asset =>
            asset.Name is { } name &&
            name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) &&
            name.Contains(runtimeIdentifier, StringComparison.OrdinalIgnoreCase) &&
            !name.Contains("Server", StringComparison.OrdinalIgnoreCase));
    }

    private static string ParseSha256Digest(string? digest) =>
        digest is { } value && value.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase) ? value["sha256:".Length..] : "";

    private sealed record GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string? TagName { get; init; }

        [JsonPropertyName("draft")]
        public bool Draft { get; init; }

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; init; }

        [JsonPropertyName("assets")]
        public Asset[]? Assets { get; init; }

        public sealed record Asset
        {
            [JsonPropertyName("name")]
            public string? Name { get; init; }

            [JsonPropertyName("browser_download_url")]
            public string? BrowserDownloadUrl { get; init; }

            [JsonPropertyName("size")]
            public long Size { get; init; }

            /// <summary>Content digest GitHub computes for the asset, formatted as "sha256:&lt;hex&gt;".</summary>
            [JsonPropertyName("digest")]
            public string? Digest { get; init; }
        }
    }
}
