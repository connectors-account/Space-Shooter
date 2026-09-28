using UnityEngine;

namespace Starfall
{
    // Small desktop HUD using built-in IMGUI: no TMP font import or UI package required.
    public sealed class FlightHud : MonoBehaviour
    {
        private FlightGame game;
        private GUIStyle title, heading, body, small, button, number;
        private readonly Color cyan = new Color(.3f,.95f,1);
        public void Initialize(FlightGame instance) { game=instance; }
        private void Styles()
        {
            if (title!=null) return;
            title=Text(66,FontStyle.Bold,TextAnchor.MiddleCenter,Color.white);
            heading=Text(28,FontStyle.Bold,TextAnchor.MiddleCenter,cyan);
            body=Text(20,FontStyle.Normal,TextAnchor.MiddleCenter,new Color(.85f,.91f,1));
            small=Text(16,FontStyle.Normal,TextAnchor.MiddleLeft,new Color(.65f,.77f,.9f));
            number=Text(26,FontStyle.Bold,TextAnchor.MiddleLeft,Color.white);
            button=new GUIStyle(GUI.skin.button) { fontSize=21, fontStyle=FontStyle.Bold };
            button.normal.textColor=Color.white;
            button.hover.textColor=cyan;
        }
        private static GUIStyle Text(int size,FontStyle weight,TextAnchor align,Color color)
        {
            var style=new GUIStyle(GUI.skin.label) { fontSize=size,fontStyle=weight,alignment=align,wordWrap=true };
            style.normal.textColor=color; return style;
        }
        private static void Fill(Rect rect,Color color)
        {
            Color previous=GUI.color; GUI.color=color; GUI.DrawTexture(rect,Texture2D.whiteTexture); GUI.color=previous;
        }
        private void OnGUI()
        {
            if (game==null) return;
            Styles();
            float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);
            Matrix4x4 old=GUI.matrix;
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)/2,(Screen.height-720*scale)/2,0),Quaternion.identity,Vector3.one*scale);
            if (game.Phase!=FlightPhase.Title) DrawHud();
            if (game.Phase==FlightPhase.Playing)
            {
                if (game.BannerTime>0)
                {
                    Fill(new Rect(360,140,560,110),new Color(.015f,.035f,.09f,.95f));
                    GUI.Label(new Rect(360,150,560,40),game.Wave==5 ? "FINAL WAVE · DREADNOUGHT" : "WAVE "+game.Wave+" / 5",heading);
                    GUI.Label(new Rect(360,195,560,35),"Hold the line. Do not let ships escape.",body);
                }
                float boss=game.BossFraction;
                if (boss>=0)
                {
                    GUI.Label(new Rect(400,76,480,25),"DREADNOUGHT",body);
                    Fill(new Rect(400,107,480,8),new Color(.2f,.06f,.12f));
                    Fill(new Rect(400,107,480*boss,8),new Color(1,.2f,.4f));
                }
            }
            else DrawOverlay();
            GUI.matrix=old;
        }
        private void DrawHud()
        {
            Fill(new Rect(0,0,1280,68),new Color(.018f,.03f,.075f,.98f));
            GUI.Label(new Rect(24,5,200,24),"HULL INTEGRITY",small);
            GUI.Label(new Rect(24,29,220,32),game.Health+" / "+GameRules.MaxHealth,number);
            GUI.Label(new Rect(235,5,170,24),"SCORE",small);
            GUI.Label(new Rect(235,29,190,32),game.Score.ToString("000000"),number);
            GUI.Label(new Rect(445,5,170,24),"PERSONAL BEST",small);
            GUI.Label(new Rect(445,29,190,32),game.Best.ToString("000000"),number);
            GUI.Label(new Rect(670,18,220,38),"WAVE "+game.Wave+" / 5",number);
            if (GUI.Button(new Rect(936,15,135,38),game.Muted ? "Sound: off" : "Sound: on",button)) game.ToggleMute();
            if (GUI.Button(new Rect(1086,15,170,38),game.Phase==FlightPhase.Paused ? "Resume" : "Pause",button)) game.TogglePause();
            Fill(new Rect(0,678,1280,42),new Color(.018f,.03f,.075f,.96f));
            string powers="";
            if (game.SpreadTime>0) powers+="TRIPLE SHOT  "+Mathf.CeilToInt(game.SpreadTime)+"s    ";
            if (game.ShieldTime>0) powers+="SHIELD  "+Mathf.CeilToInt(game.ShieldTime)+"s";
            GUI.Label(new Rect(24,684,630,30),powers.Length>0 ? powers : "Defeat five waves. Escaped ships cost one hull point.",small);
            GUI.Label(new Rect(740,684,520,30),"WASD / arrows · Space / LMB fire · Esc pause · M mute",small);
        }
        private void DrawOverlay()
        {
            Fill(new Rect(0,68,1280,610),new Color(.005f,.012f,.04f,.82f));
            Fill(new Rect(290,93,700,544),new Color(.025f,.05f,.11f,.97f));
            Fill(new Rect(290,93,700,3),cyan);
            bool menu=game.Phase==FlightPhase.Title;
            bool paused=game.Phase==FlightPhase.Paused;
            bool won=game.Phase==FlightPhase.Won;
            GUI.Label(new Rect(300,119,680,38),"S T A R F A L L   /   S Q U A D R O N",heading);
            GUI.Label(new Rect(300,174,680,84),menu ? "LAST LINE" : paused ? "PAUSED" : won ? "SECTOR CLEAR" : "SIGNAL LOST",title);
            GUI.Label(new Rect(320,263,640,62),menu ? "Five waves. One pilot.\nBreak the blockade and destroy the dreadnought." : paused ? "Your flight is on hold.\nReturn when you are ready." : "SCORE  "+game.Score.ToString("000000")+"     /     BEST  "+game.Best.ToString("000000"),body);
            string primary=paused ? "RESUME FLIGHT  [Enter]" : menu ? "LAUNCH  [Enter]" : "FLY AGAIN  [Enter]";
            if (GUI.Button(new Rect(425,347,430,54),primary,button)) { if (paused) game.TogglePause(); else game.StartRun(); }
            if (paused)
            {
                if (GUI.Button(new Rect(425,413,430,42),"RESTART RUN  [R]",button)) game.StartRun();
            }
            else if (menu)
            {
                if (GUI.Button(new Rect(425,413,430,42),game.Muted ? "ENABLE AUDIO  [M]" : "MUTE AUDIO  [M]",button)) game.ToggleMute();
            }
            else if (GUI.Button(new Rect(425,413,430,42),"MAIN MENU  [Backspace]",button)) game.ShowTitle();
            if (paused && GUI.Button(new Rect(425,466,430,40),"MAIN MENU  [Backspace]",button)) game.ShowTitle();
            if (menu && GUI.Button(new Rect(425,466,430,40),"QUIT",button)) Quit();
            GUI.Label(new Rect(320,526,640,83),menu ? "Move: WASD / arrows / gamepad left stick\nFire: Space / left mouse / gamepad A\nRepair +2  ·  Triple shot 12s  ·  Shield 8s" : "Dodge the red shots. Collect supply capsules.\nEscaped ships bypass shields and damage your hull.\nGamepad: A confirm · B menu · X mute · Y restart",body);
            GUI.Label(new Rect(320,615,640,20),"WINDOWS DESKTOP  •  SINGLE PLAYER",TextFooter());
        }
        private GUIStyle TextFooter() { return small; }
        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
    }
}
