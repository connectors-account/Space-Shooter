using System.Collections.Generic;
using UnityEngine;

namespace Starfall
{
    public enum FlightPhase { Title, Playing, Paused, Lost, Won }

    public sealed class FlightGame : MonoBehaviour
    {
        [SerializeField] private GameAssets assets;
        private sealed class Enemy
        {
            public SpriteRenderer view;
            public Vector2 position, previous;
            public float age, nextShot, originX, radius;
            public int kind, health, maxHealth, volleys;
        }
        private sealed class Bullet
        {
            public SpriteRenderer view;
            public Vector2 position, velocity;
            public bool friendly;
        }
        private sealed class Pickup
        {
            public SpriteRenderer view;
            public Vector2 position;
            public int kind;
            public float age;
        }
        private sealed class Spark
        {
            public SpriteRenderer view;
            public Vector2 velocity;
            public float life;
        }
        public FlightPhase Phase { get; private set; }
        public int Score { get; private set; }
        public int Best { get; private set; }
        public int Health { get; private set; }
        public int Wave { get; private set; }
        public bool Muted { get; private set; }
        public float SpreadTime { get { return spreadTime; } }
        public float ShieldTime { get { return shieldTime; } }
        public float BannerTime { get; private set; }
        public float BossFraction
        {
            get
            {
                foreach (Enemy e in enemies) if (e.kind == 3) return (float)e.health/e.maxHealth;
                return -1;
            }
        }
        private readonly List<Enemy> enemies = new List<Enemy>(32);
        private readonly List<Bullet> bullets = new List<Bullet>(512);
        private readonly List<Pickup> pickups = new List<Pickup>(20);
        private readonly List<Spark> sparks = new List<Spark>(160);
        private readonly Stack<SpriteRenderer> boltPool = new Stack<SpriteRenderer>(512);
        private readonly Stack<SpriteRenderer> sparkPool = new Stack<SpriteRenderer>(160);
        private SpriteRenderer player;
        private Vector2 playerPosition, playerPrevious;
        private SpaceBackdrop backdrop;
        private AudioSource sound, ambience;
        private float fireTime, spawnTime, waveTime, invincible, spreadTime, shieldTime, enemySoundTime;
        private int pending, spawnIndex, kills;
        private Camera gameCamera;
        private int screenWidth, screenHeight;
        private static readonly Color Cyan = new Color(.25f,.95f,1);
        private static readonly Color Pink = new Color(1,.25f,.48f);

        private void Awake()
        {
            if (assets == null) { Debug.LogError("Missing game assets. Run Starfall > Rebuild Generated Content."); enabled = false; return; }
            gameCamera = Camera.main;
            Application.targetFrameRate = 60;
            Best = PlayerPrefs.GetInt("Starfall.Best", 0);
            Muted = PlayerPrefs.GetInt("Starfall.Mute", 0) == 1;
            sound = gameObject.AddComponent<AudioSource>();
            ambience = gameObject.AddComponent<AudioSource>();
            sound.playOnAwake = false; sound.volume = .45f;
            ambience.clip = assets.ambient; ambience.loop = true; ambience.volume = .4f;
            ambience.Play(); ApplyMute();
            player = Instantiate(assets.player, transform);
            backdrop = new GameObject("Parallax Starfield").AddComponent<SpaceBackdrop>();
            backdrop.transform.SetParent(transform, false);
            backdrop.Create(assets.particle);
            GetComponent<FlightHud>().Initialize(this);
            ShowTitle();
        }
        private void Update()
        {
            if (screenWidth != Screen.width || screenHeight != Screen.height) FitCamera();
            if (PilotInput.Mute) ToggleMute();
            if (PilotInput.Pause && (Phase == FlightPhase.Playing || Phase == FlightPhase.Paused)) TogglePause();
            else if (PilotInput.Confirm && (Phase == FlightPhase.Title || Phase == FlightPhase.Lost || Phase == FlightPhase.Won)) StartRun();
            else if (PilotInput.Confirm && Phase == FlightPhase.Paused) TogglePause();
            else if (PilotInput.Restart && Phase != FlightPhase.Title) StartRun();
            else if (PilotInput.Menu && Phase != FlightPhase.Playing) ShowTitle();
            float dt = Mathf.Min(Time.deltaTime, .05f);
            if (Phase == FlightPhase.Title) { backdrop.Tick(dt * .45f); return; }
            if (Phase != FlightPhase.Playing) return;
            backdrop.Tick(dt);
            invincible = Mathf.Max(0, invincible-dt);
            spreadTime = Mathf.Max(0, spreadTime-dt); shieldTime = Mathf.Max(0, shieldTime-dt);
            BannerTime = Mathf.Max(0, BannerTime-dt); enemySoundTime -= dt;
            playerPrevious = playerPosition;
            playerPosition += PilotInput.Movement * (GameRules.PlayerSpeed * dt);
            playerPosition.x = Mathf.Clamp(playerPosition.x, -GameRules.HalfWidth, GameRules.HalfWidth);
            playerPosition.y = Mathf.Clamp(playerPosition.y, -4.15f, 3.65f);
            player.transform.position = playerPosition;
            player.color = invincible > 0 && Mathf.Sin(invincible*40)>0 ? new Color(1,1,1,.3f) : shieldTime>0 ? new Color(.55f,.8f,1) : Color.white;
            player.transform.rotation = Quaternion.Euler(0,0,-PilotInput.Movement.x*10);
            fireTime -= dt;
            if (PilotInput.Fire && fireTime <= 0)
            {
                fireTime = .16f;
                Shoot(playerPosition + Vector2.up*.65f, Vector2.up*13, true);
                if (spreadTime > 0)
                {
                    Shoot(playerPosition + new Vector2(-.2f,.4f), new Vector2(-2.2f,12.8f), true);
                    Shoot(playerPosition + new Vector2(.2f,.4f), new Vector2(2.2f,12.8f), true);
                }
                Play(assets.fire, .28f);
            }
            TickEnemies(dt);
            if (Phase != FlightPhase.Playing) return;
            TickBullets(dt);
            if (Phase != FlightPhase.Playing) return;
            TickPickups(dt); TickSparks(dt); TickWaves(dt);
        }
        private void FitCamera()
        {
            screenWidth = Screen.width; screenHeight = Screen.height;
            float aspect = screenWidth/(float)Mathf.Max(1,screenHeight), target = 16f/9;
            gameCamera.rect = aspect > target ? new Rect((1-target/aspect)/2,0,target/aspect,1) : new Rect(0,(1-aspect/target)/2,1,aspect/target);
        }
        public void StartRun()
        {
            ClearArena(); Score = 0; Health = GameRules.MaxHealth; Wave = 0; kills = 0;
            spreadTime = shieldTime = fireTime = 0; invincible = 2;
            playerPosition = new Vector2(0,-3.5f); playerPrevious = playerPosition;
            player.transform.position = playerPosition; player.transform.rotation = Quaternion.identity;
            player.color = Color.white; player.gameObject.SetActive(true);
            Phase = FlightPhase.Playing; BeginWave();
        }
        public void ShowTitle()
        {
            SaveBest(); ClearArena(); Phase = FlightPhase.Title; player.gameObject.SetActive(false);
        }
        public void TogglePause()
        {
            if (Phase == FlightPhase.Playing) Phase = FlightPhase.Paused;
            else if (Phase == FlightPhase.Paused) Phase = FlightPhase.Playing;
            Play(assets.ui);
        }
        public void ToggleMute()
        {
            Muted = !Muted; ApplyMute(); PlayerPrefs.SetInt("Starfall.Mute", Muted ? 1 : 0); PlayerPrefs.Save();
        }
        private void ApplyMute() { sound.mute = ambience.mute = Muted; }
        private void BeginWave()
        {
            Wave++; pending = GameRules.EnemyCount(Wave); spawnIndex = 0;
            spawnTime = 2.3f; waveTime = 2.2f; BannerTime = 2.3f; Play(assets.wave);
        }
        private void TickWaves(float dt)
        {
            if (pending > 0)
            {
                spawnTime -= dt;
                if (spawnTime <= 0) { SpawnEnemy(); pending--; spawnTime = Mathf.Max(.45f,1.2f-Wave*.12f); }
            }
            else if (enemies.Count == 0)
            {
                // Clear enemy shots before the intermission, preserving friendly shots.
                for (int i=bullets.Count-1;i>=0;i--) if (!bullets[i].friendly) RemoveBullet(i);
                waveTime -= dt;
                if (waveTime <= 0) { if (Wave == GameRules.WaveCount) Finish(true); else BeginWave(); }
            }
        }
        private void SpawnEnemy()
        {
            int kind = Wave == GameRules.WaveCount ? 3 : (spawnIndex + Wave - 1) % Mathf.Min(3,Wave);
            float x = kind == 3 ? 0 : Mathf.Sin(spawnIndex*2.4f + Wave)*6.3f;
            var e = new Enemy { kind=kind, position=new Vector2(x,5.8f), originX=x, radius=kind==3 ? 1.1f : .4f+kind*.12f,
                health=GameRules.EnemyHealth(kind,Wave), nextShot=1.35f };
            e.previous=e.position; e.maxHealth=e.health;
            e.view=Instantiate(assets.enemies[kind], e.position, Quaternion.Euler(0,0,180),transform);
            enemies.Add(e); spawnIndex++;
        }
        private void TickEnemies(float dt)
        {
            for (int i=enemies.Count-1;i>=0;i--)
            {
                Enemy e=enemies[i]; e.previous=e.position; e.age+=dt;
                if (e.kind==3)
                {
                    e.position.y=Mathf.MoveTowards(e.position.y,2.6f,dt*1.8f);
                    e.position.x=Mathf.Sin(e.age*.65f)*5.5f;
                }
                else
                {
                    e.position.y-=dt*(.75f+Wave*.12f+(.2f*(2-e.kind)));
                    e.position.x=Mathf.Clamp(e.originX+Mathf.Sin(e.age*1.8f)*(.65f+e.kind*.25f),-7.8f,7.8f);
                }
                e.view.transform.position=e.position; e.nextShot-=dt;
                if (e.position.y<4 && e.position.y>-3.5f && e.nextShot<=0)
                {
                    EnemyVolley(e); e.nextShot=e.kind==3 ? .85f : 1.7f-Wave*.09f;
                }
                Vector2 a=e.previous-playerPrevious, b=e.position-playerPosition;
                if (GameRules.SegmentHit(a.x,a.y,b.x,b.y,0,0,e.radius+.26f))
                {
                    HurtPlayer();
                    if (e.kind!=3) { RemoveEnemy(i,false); if (Phase!=FlightPhase.Playing) return; continue; }
                }
                if (e.position.y < -5.7f) { RemoveEnemy(i,false); HurtPlayer(true); }
                if (Phase!=FlightPhase.Playing) return;
            }
        }
        private void EnemyVolley(Enemy e)
        {
            Vector2 origin=e.position+Vector2.down*(e.kind==3 ? 1 : .45f);
            Vector2 aimed=(playerPosition-origin).normalized;
            if (aimed.sqrMagnitude<.1f) aimed=Vector2.down;
            float speed=3.2f+Wave*.22f;
            if (e.kind==0) Shoot(origin,Vector2.down*speed,false);
            else if (e.kind==1) Shoot(origin,aimed*speed,false);
            else if (e.kind==2 || e.volleys%2==0)
            {
                int count=e.kind==3 ? 5 : 3;
                for (int j=0;j<count;j++) Shoot(origin,Rotate(aimed,(j-(count-1)*.5f)*18)*speed,false);
            }
            else for (int j=0;j<12;j++) Shoot(origin,Rotate(Vector2.down,j*30+e.volleys*11)*3.1f,false);
            e.volleys++;
            if (enemySoundTime<=0) { Play(assets.enemy,.22f); enemySoundTime=.12f; }
        }
        private static Vector2 Rotate(Vector2 value,float degrees)
        {
            float a=degrees*Mathf.Deg2Rad,c=Mathf.Cos(a),s=Mathf.Sin(a);
            return new Vector2(value.x*c-value.y*s,value.x*s+value.y*c);
        }
        private void Shoot(Vector2 position,Vector2 velocity,bool friendly)
        {
            if (bullets.Count>=500) return;
            SpriteRenderer view=boltPool.Count>0 ? boltPool.Pop() : Instantiate(assets.bolt,transform);
            view.gameObject.SetActive(true); view.transform.position=position;
            view.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(-velocity.x,velocity.y)*Mathf.Rad2Deg);
            view.color=friendly ? Cyan : Pink;
            bullets.Add(new Bullet { view=view, position=position, velocity=velocity, friendly=friendly });
        }
        private void TickBullets(float dt)
        {
            for (int i=bullets.Count-1;i>=0;i--)
            {
                Bullet b=bullets[i]; Vector2 previous=b.position;
                b.position+=b.velocity*dt; b.view.transform.position=b.position;
                bool consumed=false;
                if (b.friendly)
                {
                    for (int j=enemies.Count-1;j>=0;j--)
                    {
                        Enemy e=enemies[j]; Vector2 a=previous-e.previous, end=b.position-e.position;
                        if (!GameRules.SegmentHit(a.x,a.y,end.x,end.y,0,0,e.radius+.09f)) continue;
                        e.health--; Burst(b.position,Cyan,3); consumed=true;
                        if (e.health<=0) RemoveEnemy(j,true);
                        break;
                    }
                }
                else
                {
                    Vector2 a=previous-playerPrevious,end=b.position-playerPosition;
                    if (GameRules.SegmentHit(a.x,a.y,end.x,end.y,0,0,.32f)) { HurtPlayer(); consumed=true; }
                }
                if (consumed || Mathf.Abs(b.position.x)>10 || Mathf.Abs(b.position.y)>6.5f) RemoveBullet(i);
                if (Phase!=FlightPhase.Playing) return;
            }
        }
        private void RemoveBullet(int index)
        {
            SpriteRenderer view=bullets[index].view; view.gameObject.SetActive(false); boltPool.Push(view); bullets.RemoveAt(index);
        }
        private void RemoveEnemy(int index,bool reward)
        {
            Enemy e=enemies[index]; Burst(e.position,e.kind==3 ? Color.red : Pink,18); Play(assets.explosion,.6f);
            if (reward)
            {
                Score+=GameRules.Points(e.kind); Best=Mathf.Max(Best,Score); kills++;
                if (kills%4==0 && e.kind!=3)
                {
                    int kind=(kills/4-1)%3;
                    pickups.Add(new Pickup { kind=kind, position=e.position, view=Instantiate(assets.pickups[kind],e.position,Quaternion.identity,transform) });
                }
            }
            Destroy(e.view.gameObject); enemies.RemoveAt(index);
        }
        private void HurtPlayer(bool breach=false)
        {
            if (!breach && (invincible>0 || shieldTime>0)) return;
            Health--; invincible=1.3f; Burst(playerPosition,Cyan,16); Play(assets.hit);
            if (Health<=0) Finish(false);
        }
        private void TickPickups(float dt)
        {
            for (int i=pickups.Count-1;i>=0;i--)
            {
                Pickup p=pickups[i]; p.age+=dt; Vector2 old=p.position;
                p.position+=Vector2.down*dt*1.25f; p.view.transform.position=p.position;
                p.view.transform.rotation=Quaternion.Euler(0,0,Mathf.Sin(p.age*3)*10);
                Vector2 a=old-playerPrevious,b=p.position-playerPosition;
                bool collected=GameRules.SegmentHit(a.x,a.y,b.x,b.y,0,0,.7f);
                if (collected)
                {
                    if (p.kind==0) Health=GameRules.Heal(Health);
                    else if (p.kind==1) spreadTime=12;
                    else shieldTime=8;
                    Score+=25; Best=Mathf.Max(Best,Score); Play(assets.pickup); Burst(p.position,Color.yellow,12);
                }
                if (collected || p.position.y<-5.8f) { Destroy(p.view.gameObject); pickups.RemoveAt(i); }
            }
        }
        private void Burst(Vector2 position,Color color,int count)
        {
            for (int i=0;i<count && sparks.Count<150;i++)
            {
                SpriteRenderer view;
                if (sparkPool.Count>0) view=sparkPool.Pop();
                else
                {
                    var go=new GameObject("Spark"); go.transform.SetParent(transform,false);
                    view=go.AddComponent<SpriteRenderer>(); view.sprite=assets.particle; view.sortingOrder=8;
                }
                view.gameObject.SetActive(true); view.transform.position=position; view.transform.localScale=Vector3.one*.075f;
                view.color=color;
                sparks.Add(new Spark { view=view, velocity=Random.insideUnitCircle*3, life=.5f });
            }
        }
        private void TickSparks(float dt)
        {
            for (int i=sparks.Count-1;i>=0;i--)
            {
                Spark s=sparks[i]; s.life-=dt;
                s.view.transform.position+=(Vector3)(s.velocity*dt);
                Color c=s.view.color; c.a=Mathf.Clamp01(s.life*2); s.view.color=c;
                if (s.life<=0) { s.view.gameObject.SetActive(false); sparkPool.Push(s.view); sparks.RemoveAt(i); }
            }
        }
        private void Finish(bool won)
        {
            Phase=won ? FlightPhase.Won : FlightPhase.Lost;
            SaveBest(); Play(won ? assets.win : assets.lose);
        }
        private void ClearArena()
        {
            foreach (Enemy e in enemies) Destroy(e.view.gameObject); enemies.Clear();
            for (int i=bullets.Count-1;i>=0;i--) RemoveBullet(i);
            foreach (Pickup p in pickups) Destroy(p.view.gameObject); pickups.Clear();
            foreach (Spark s in sparks) { s.view.gameObject.SetActive(false); sparkPool.Push(s.view); } sparks.Clear();
        }
        private void Play(AudioClip clip,float volume=1) { if (clip!=null) sound.PlayOneShot(clip,volume); }
        private void SaveBest() { PlayerPrefs.SetInt("Starfall.Best",Best); PlayerPrefs.Save(); }
        private void OnApplicationFocus(bool focused) { if (!focused && Phase==FlightPhase.Playing) { Phase=FlightPhase.Paused; SaveBest(); } }
        private void OnApplicationPause(bool paused) { if (paused && Phase==FlightPhase.Playing) { Phase=FlightPhase.Paused; SaveBest(); } }
        private void OnApplicationQuit() { SaveBest(); }
    }
}
