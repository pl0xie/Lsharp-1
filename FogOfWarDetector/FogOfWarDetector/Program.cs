using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace FogOfWarDetector
{
    class Program
    {
        private static Obj_AI_Base Player = ObjectManager.Player;
        private static Obj_AI_Base Near = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault();
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
        
        }

        static void Drawing_OnEndScene(EventArgs args)
        {
      

            }

        

        static void Drawing_OnDraw(EventArgs args)
        {

            foreach (var Object2 in ObjectManager.Get<Obj_AI_Base>())
            {
                if (Object2.Distance(Player) < Near.Distance(Player) && Object2.IsEnemy && Object2.IsValid)
                {
                    Near = Object2;
                }
            }
            if (Near.IsValid && !Near.IsDead && Near != null)
            {
                Drawing.DrawCircle(Near.Position, 1200, Color.Red);
                var pos = Near.Position.To2D().Extend(Player.Position.To2D(), 1200);
                var Mpos = Near.Position.To2D() - pos;
                Mpos.Perpendicular();
                var la = Mpos.Extend(Mpos, 600);
                Drawing.DrawLine(Mpos, la, 10, Color.White);
                
            }

        }

    }
}
