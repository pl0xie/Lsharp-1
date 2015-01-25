using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Over9000_Axes
{
    class Program
    {
        public static Orbwalking.Orbwalker Orbwalker;
        private static List<GameObject> Reticles = new List<GameObject>();
        public static Spell Q, W, E, R;
        public static Menu Config;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 20000);
            Config = new Menu("Over9000 Rockets", "Tristana", true);
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            Config.AddToMainMenu();

            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveMode.ToString().ToLower() == "laneclear")
            {
                Q.Cast();

            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (!(sender.Name.Contains("Q_reticle_self")))
            {
                return;
            }
            Reticles.Add(sender);
            Orbwalker.SetOrbwalkingPoint(sender.Position);
        }

        static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (!(sender.Name.Contains("Q_reticle_self")))
            {
                return;
            }
            Reticles.Remove(sender);
            Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
        }


    }
}
