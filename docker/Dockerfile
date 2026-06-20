# Nitrox dedicated server.
#
# The server binaries are pre-published by CI (publish/server-linux-x64) and copied in here, so this image performs
# no in-image compilation. Subnautica's (copyrighted) game files are NOT baked in: at runtime they are either
#   - read from a mounted volume (mount an existing Subnautica install), or
#   - downloaded once via SteamCMD into the /data/game volume (requires a Steam account that owns the game).
# See docker/README.md and docker/docker-compose.example.yml.
#
# The container runs unprivileged as the base image's non-root "app" user (UID 1654 / $APP_UID); everything it
# writes at runtime (SteamCMD, the game download, and the Nitrox data dir) is chowned to that user below.
FROM mcr.microsoft.com/dotnet/runtime:10.0

# SteamCMD + its 32-bit dependencies (used only when the game must be downloaded at runtime).
RUN dpkg --add-architecture i386 \
    && apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl tar lib32gcc-s1 bash \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /opt/steamcmd \
    && curl -sqL "https://steamcdn-a.akamaihd.net/client/installer/steamcmd_linux.tar.gz" | tar zxvf - -C /opt/steamcmd

WORKDIR /app
COPY publish/server-linux-x64/ /app/
COPY docker/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# Run unprivileged. Create a writable HOME (SteamCMD keeps its config/cache under $HOME) and hand the app user
# ownership of everything it must write at runtime: SteamCMD's own dir (it self-updates), the game download dir,
# and the Nitrox data dir. /app stays root-owned (read-only to the runtime user). These chowns run BEFORE the
# VOLUME instruction so a freshly created named volume inherits the app user's ownership.
ENV HOME="/home/app"
RUN mkdir -p "$HOME" /data/game /data/nitrox \
    && chown -R "$APP_UID:$APP_UID" "$HOME" /opt/steamcmd /data

# Defaults — override any NITROX_* (ServerStartOptions) or STEAM_* value at runtime.
# Note: keys mirror the server's config keys, so they contain hyphens (NITROX_game-path etc.).
ENV NITROX_game-path="/data/game/Subnautica" \
    NITROX_data-path="/data/nitrox" \
    NITROX_assets-path="/app" \
    NITROX_save="My World" \
    STEAMCMD_DIR="/opt/steamcmd" \
    SUBNAUTICA_APPID="264710"

# Persist the downloaded game (so it isn't re-downloaded) and the saves/config.
VOLUME ["/data/game", "/data/nitrox"]

# Default Nitrox server UDP port.
EXPOSE 11000/udp

# Drop to the base image's non-root user for the entrypoint.
USER $APP_UID

ENTRYPOINT ["/entrypoint.sh"]
