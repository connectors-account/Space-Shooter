# Audio Setup

`AudioManager` exposes seven clip slots. Import audio files into `Assets/Audio/`
and drag them onto the matching field of the **AudioManager** component.

| AudioManager field | Purpose | Suggested clip |
|--------------------|---------|----------------|
| `bgMusic` | Looping in-game music | `bg_music.ogg` |
| `bossMusic` | Boss-fight music | `boss_music.ogg` |
| `menuMusic` | Main-menu loop | `menu_music.ogg` |
| `shootSFX` | Player/enemy fire | `shoot.wav` |
| `explosionSFX` | Enemy/player death | `explosion.wav` |
| `powerUpSFX` | Power-up pickup | `powerup.wav` |
| `hitSFX` | Bullet hit / damage | `hit.wav` |

## Where to get free assets

All of these are free and license-friendly (check each asset's page for the exact
licence — most are CC0 / CC-BY):

### OpenGameArt.org (https://opengameart.org)
* Search **"space shooter sfx"** — e.g. *"512 Sound Effects (8-bit style)"* (CC0)
  gives you shoot/explosion/hit/powerup laser sounds in one pack.
* Search **"space music loop"** for background/boss tracks (many CC-BY).

### Kenney.nl (https://kenney.nl/assets) — CC0
* **"Sci-Fi Sounds"** and **"Digital Audio"** packs contain laser, explosion,
  power-up and UI blips. Rename the ones you pick to the file names above.

### freesound.org (https://freesound.org)
* Individual laser/explosion samples (filter by CC0). Download as WAV.

### Unity Asset Store (free tier)
* Search **"free sci-fi sound effects"** or **"free space shooter music"** and
  import via **Window → Package Manager → My Assets**.

## Import settings
* **SFX (shoot/hit/explosion/powerup):** Load Type = *Decompress On Load*,
  Force To Mono = on — keeps latency low for frequent one-shots.
* **Music (bg/boss/menu):** Load Type = *Streaming*, Compression = Vorbis,
  loopable. `AudioManager.PlayMusic` sets `loop = true` automatically.

## Assigning at runtime
The scripts call, for example, `AudioManager.Instance.PlaySFX(shootSFX)` and
`AudioManager.Instance.PlayMusic(bgMusic)`. As long as the clips are assigned in the
Inspector, no code changes are needed. Volumes are saved to `PlayerPrefs`
(`SpaceShooter.MusicVolume`, `SpaceShooter.SFXVolume`) and restored on load; the
pause menu's sliders call `SetMusicVolume` / `SetSFXVolume`.

If a clip slot is left empty the code safely no-ops (null-checked), so the game
still runs without audio while you gather assets.
