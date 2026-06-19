#!/usr/bin/env bash
# Ensures Subnautica game files are available (mounted or downloaded via SteamCMD), then starts the Nitrox server.
set -euo pipefail

# NITROX_game-path contains a hyphen, which POSIX shells can't expand with $VAR, so read it via printenv.
GAME_DIR="$(printenv 'NITROX_game-path' 2>/dev/null || true)"
GAME_DIR="${GAME_DIR:-/data/game/Subnautica}"
APPID="${SUBNAUTICA_APPID:-264710}"
STEAMCMD_DIR="${STEAMCMD_DIR:-/opt/steamcmd}"

MANAGED_DLL="$GAME_DIR/Subnautica_Data/Managed/Assembly-CSharp.dll"

if [ -f "$MANAGED_DLL" ]; then
    echo "[nitrox] Using Subnautica game files at: $GAME_DIR"
elif [ -n "${STEAM_USERNAME:-}" ] && [ -n "${STEAM_PASSWORD:-}" ]; then
    echo "[nitrox] Subnautica not found at $GAME_DIR. Downloading once via SteamCMD (the /data/game volume caches it)..."
    # Force the Windows depot so the managed assemblies the server loads are present.
    "$STEAMCMD_DIR/steamcmd.sh" \
        +force_install_dir "$GAME_DIR" \
        +login "$STEAM_USERNAME" "$STEAM_PASSWORD" \
        +@sSteamCmdForcePlatformType windows \
        +app_update "$APPID" validate \
        +quit
    if [ ! -f "$MANAGED_DLL" ]; then
        echo "[nitrox] ERROR: SteamCMD finished but $MANAGED_DLL is missing." >&2
        echo "[nitrox]        Check that the Steam account owns Subnautica and that Steam Guard is handled." >&2
        exit 1
    fi
else
    cat >&2 <<EOF
[nitrox] ERROR: No Subnautica game files at "$GAME_DIR" and no Steam credentials provided.
Do one of the following:
  - Mount an existing Subnautica install there (recommended), e.g.:
        -v /path/to/Subnautica:/data/game/Subnautica
  - Provide STEAM_USERNAME and STEAM_PASSWORD for an account that owns Subnautica so the game is
    downloaded once into the /data/game volume.
EOF
    exit 1
fi

echo "[nitrox] Starting Nitrox server (save: ${NITROX_save:-My World})..."
exec dotnet /app/Nitrox.Server.Subnautica.dll "$@"
