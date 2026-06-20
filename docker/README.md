# Nitrox dedicated server (Docker)

A containerized Nitrox Subnautica server, published to GHCR by CI on every push to `master`:

```
ghcr.io/ddprince17/nitrox-server:latest
ghcr.io/ddprince17/nitrox-server:<version>
```

> The image contains **only the server**. Subnautica's game files are copyrighted and are **not** included — you
> either mount an existing install or let the container download it once via SteamCMD into a cached volume.

## Quick start

### Option A — mount an existing Subnautica install (no Steam credentials needed)

```bash
docker run -d --name nitrox-server \
  -p 11000:11000/udp \
  -e NITROX_save="My World" \
  -v /path/to/Subnautica:/data/game/Subnautica:ro \
  -v nitrox_saves:/data/nitrox \
  ghcr.io/ddprince17/nitrox-server:latest
```

### Option B — let the container download the game (SteamCMD)

Requires a Steam account that **owns Subnautica**. The game is downloaded once and cached in the `nitrox_game` volume.

```bash
docker run -d --name nitrox-server \
  -p 11000:11000/udp \
  -e STEAM_USERNAME="your_steam_login" \
  -e STEAM_PASSWORD="your_steam_password" \
  -v nitrox_game:/data/game \
  -v nitrox_saves:/data/nitrox \
  ghcr.io/ddprince17/nitrox-server:latest
```

Or use [`docker-compose.example.yml`](./docker-compose.example.yml).

## Configuration via environment variables

All `ServerStartOptions` are settable with the `NITROX_` prefix (keys mirror the CLI options, so they contain
hyphens). Command-line args still override env vars.

| Env var               | Meaning                              | Default                  |
| --------------------- | ------------------------------------ | ------------------------ |
| `NITROX_save`         | Save/world name                      | `My World`               |
| `NITROX_game-path`    | Subnautica install root              | `/data/game/Subnautica`  |
| `NITROX_data-path`    | Where saves + config are stored      | `/data/nitrox`           |
| `NITROX_assets-path`  | Nitrox bundled resources             | `/app`                   |

In-game server options also bind from env vars using `Section__Key`, e.g.:

```
NITROX_GameServer__MaxConnections=10
NITROX_GameServer__ServerPassword=changeme
```

Steam download (Option B) only:

| Env var          | Meaning                                            |
| ---------------- | -------------------------------------------------- |
| `STEAM_USERNAME` | Steam login for an account that owns Subnautica    |
| `STEAM_PASSWORD` | Steam password                                     |

> **Steam Guard:** if the account has Steam Guard enabled, the non-interactive login will fail. Use a dedicated
> account with Steam Guard handled, or prefer Option A (mount the game).

## Volumes

| Path          | Purpose                                                      |
| ------------- | ----------------------------------------------------------- |
| `/data/game`  | Cached game download (skip mounting if you use Option A)    |
| `/data/nitrox`| Saves + server config (persist this)                        |

## Ports

`11000/udp` — the default Nitrox server port.

## Runs as non-root

The container runs as the base image's unprivileged `app` user (**UID 1654**), not root. Named volumes
(`/data/game`, `/data/nitrox`) inherit the correct ownership automatically. If you bind-mount a host directory
for the data instead, make it writable by UID 1654 (`chown -R 1654:1654 <dir>`). For Option A, the mounted game
files only need to be **readable** by UID 1654 (they normally are).
