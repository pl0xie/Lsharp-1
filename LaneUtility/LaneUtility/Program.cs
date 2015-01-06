using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using SimpleLib;

namespace LaneUtility
{
    class Program
    {
        private static Obj_AI_Base Player = ObjectManager.Player;
        private static Menu Config;
        private static Geometry.Rectangle Rekt;
        private static Geometry.Polygon Poly;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Rekt = new Geometry.Rectangle(new Vector2(3522, 3538), new Vector2(11824, 11870), 300);
            Poly = new Geometry.Polygon();
            Config = new Menu("Lane Utility", "LaneUtility", true);
            Config.AddSubMenu(new Menu("Keys", "Keys"));
            Config.SubMenu("Keys")
                .AddItem(new MenuItem("Freeze", "Freeze Lane"))
                .SetValue(new KeyBind('X', KeyBindType.Press));
            Config.AddToMainMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            Geometry.Draw.DrawRectangle(Rekt, Color.Cyan, 1);
            Geometry.Draw.DrawPolygon(Poly, Color.Cyan, 1);

            if (Config.Item("Freeze").GetValue<KeyBind>().Active)
            {
                if (Poly.Points.All(pa => pa != Game.CursorPos.To2D()))
                {
                    Poly.Add(Game.CursorPos.To2D());
                    Game.PrintChat(Game.CursorPos.ToString( ));
                }

            }
            

        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Config.Item("Freeze").GetValue<KeyBind>().Active)
            {
                return;
            }
            var EnemyMinion = MinionManager.GetMinions(5000, MinionTypes.All, MinionTeam.Enemy);
            var AllyMinion = MinionManager.GetMinions(5000, MinionTypes.All, MinionTeam.Ally);
            var enemyhealth = 0.0;
            var allyhealth = 0.0;
            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
            {
                if (!Rekt.IsOutside(minion.Position.To2D()))
                {
                    if (minion.IsEnemy)
                    {
                        enemyhealth += minion.Health;
                    }
                    if (minion.IsAlly)
                    {
                        allyhealth += minion.Health;
                    }
                }
            }

            if (EnemyMinion.Count < AllyMinion.Count + 1)
            {
                return;
            }

            
            var Etotalhealth = EnemyMinion.Aggregate(0.0, (current, minion) => current + minion.Health);
            var Atotalhealth = AllyMinion.Aggregate(0.0, (current, minion) => current + minion.Health);

            if (Etotalhealth > Atotalhealth + AllyMinion.FirstOrDefault().MaxHealth * 1)
            {
                if (EnemyMinion.FirstOrDefault().Distance(Player) < Player.AttackRange)
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, EnemyMinion.FirstOrDefault());
                }         
            }
        }
    }
}
