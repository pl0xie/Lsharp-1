using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace RektSai_OVER_9000
{
    class Program
    {
        private static Spell Q;
        private static Spell W;
        private static Spell E;
        private static Spell Q_Burrow;
        private static Spell E_Burrow;
        private static Obj_AI_Hero Player = ObjectManager.Player;


        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;

        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Spell Q = new Spell(SpellSlot.Q, 325);
            Spell W = new Spell(SpellSlot.W);
            Spell E = new Spell(SpellSlot.E, 250);
            Spell Q_Burrow = new Spell(SpellSlot.Q, 1500);
            Spell E_Burrow = new Spell(SpellSlot.E, 500);
            

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
        }

        static void Drawing_OnEndScene(EventArgs args)
        {
            Drawing.DrawCircle(Player.Position, Q.Range, Color.Red);
            Drawing.DrawCircle(Player.Position, E.Range, Color.Green);
            Drawing.DrawCircle(Player.Position, Q_Burrow.Range, Color.DarkCyan);
            Drawing.DrawCircle(Player.Position, E_Burrow.Range, Color.Blue);
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            Drawing.DrawCircle(Player.Position, Q.Range, Color.Red);
            Drawing.DrawCircle(Player.Position, E.Range, Color.Green);
            Drawing.DrawCircle(Player.Position, Q_Burrow.Range, Color.DarkCyan);
            Drawing.DrawCircle(Player.Position, E_Burrow.Range, Color.Blue);
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                Game.PrintChat(args.SData.Name);
            }
        }


        private static bool IsBurrowMode()
        {
            return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name.Contains("burrow");
        }
    }
}
