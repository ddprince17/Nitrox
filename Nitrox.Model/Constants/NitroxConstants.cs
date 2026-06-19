namespace Nitrox.Model.Constants;

public static class NitroxConstants
{
    public const string LAUNCHER_APP_NAME = "Nitrox.Launcher";
    public const string PLAYER_NAME_VALID_REGEX = @"^[ a-zA-Z0-9._-]{3,25}$";

    /// <summary>
    ///     "owner/repo" used as an additional auto-update source (GitHub Releases). The CI release workflow publishes the
    ///     launcher/server zips here on every push to master.
    /// </summary>
    public const string GITHUB_RELEASES_REPOSITORY = "ddprince17/Nitrox";
}
