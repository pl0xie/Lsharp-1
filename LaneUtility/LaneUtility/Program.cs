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
        private static Geometry.Polygon PolyTop;
        private static Geometry.Polygon PolyBot;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Rekt = new Geometry.Rectangle(new Vector2(3522, 3538), new Vector2(11824, 11870), 300);
            PolyTop = new Geometry.Polygon();
            PolyBot = new Geometry.Polygon();
            PolyTopPos();
            PolyBotPos();
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
            Geometry.Draw.DrawPolygon(PolyTop, Color.Cyan, 1);
            Geometry.Draw.DrawPolygon(PolyBot, Color.Cyan, 1);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Config.Item("Freeze").GetValue<KeyBind>().Active)
            {
                return;
            }

            var EnemyMinion = MinionManager.GetMinions(5000, MinionTypes.All, MinionTeam.Enemy);
            var AllyMinion = MinionManager.GetMinions(5000, MinionTypes.All, MinionTeam.Ally);


            if (!PolyTop.IsOutside(Player.Position.To2D()))
            {
                CheckPolygon(PolyTop);
            }
            else
            {
                if (!Rekt.IsOutside(Player.Position.To2D()))
                {
                    CheckPolygon(Rekt.ToPolygon());
                }
                else
                {
                    if (!PolyBot.IsOutside(Player.Position.To2D()))
                    {
                        CheckPolygon(PolyBot);
                    }
                }
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

        private static void CheckPolygon(Geometry.Polygon Poly)
        {
            var enemyhealth = 0.0;
            var allyhealth = 0.0;
            var enemycount = 0.0;
            var allycount = 0.0;
            //maybe add distance check for ally minions above 1000 are not relevent !
            foreach (var minion in ObjectManager.Get<Obj_AI_Base>().Where(minion => !Poly.IsOutside(minion.Position.To2D()) && minion.IsMinion)) {
                if (minion.IsEnemy)
                {
                    enemyhealth += minion.Health;
                    enemycount++;
                }
                if (minion.IsAlly)
                {
                    allyhealth += minion.Health;
                    allycount++;
                }
                if (enemyhealth > (1 / allycount / 2 + 1) * allyhealth)
                    // example if enemy health is bigger then allyhealth + 1 half minion extra
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                }

                Game.PrintChat("Enemy Health : " + enemyhealth);
                Game.PrintChat("Enemy count : " + enemycount);
                Game.PrintChat("ally Health : " + allyhealth);
                Game.PrintChat("ally Count : " + allycount);
            }
        }
        private static void PolyTopPos()
        {
            PolyTop.Add(new Vector2((float)919.7371, (float)4028.275));
            PolyTop.Add(new Vector2((float)919.7371, (float)4028.275));
            PolyTop.Add(new Vector2((float)805.353, (float)10307.55));
            PolyTop.Add(new Vector2((float)1039.716, (float)11677.1));
            PolyTop.Add(new Vector2((float)1546.943, (float)12545.57));
            PolyTop.Add(new Vector2((float)2395.712, (float)13297.97));
            PolyTop.Add(new Vector2((float)2397.954, (float)13297.97));
            PolyTop.Add(new Vector2((float)3606.474, (float)13701.09));
            PolyTop.Add(new Vector2((float)5639.986, (float)13856.76));
            PolyTop.Add(new Vector2((float)5180.965, (float)13989.7));
            PolyTop.Add(new Vector2((float)10716.5, (float)14062.6));
            PolyTop.Add(new Vector2((float)10809.85, (float)13374.7));
            PolyTop.Add(new Vector2((float)9013.054, (float)13284.26));
            PolyTop.Add(new Vector2((float)6969.872, (float)13270.04));
            PolyTop.Add(new Vector2((float)3385.986, (float)13157.16));
            PolyTop.Add(new Vector2((float)3383.839, (float)13157.16));
            PolyTop.Add(new Vector2((float)2866.728, (float)12537.59));
            PolyTop.Add(new Vector2((float)2866.649, (float)12535.23));
            PolyTop.Add(new Vector2((float)1990.024, (float)11690.21));
            PolyTop.Add(new Vector2((float)1705.696, (float)10593.16));
            PolyTop.Add(new Vector2((float)1587.468, (float)8645.741));
            PolyTop.Add(new Vector2((float)1682.363, (float)6959.578));
            PolyTop.Add(new Vector2((float)1591.751, (float)4906.958));
            PolyTop.Add(new Vector2((float)1332.146, (float)3819.446));
        }
        private static void PolyBotPos()
        {
            PolyBot.Add(new Vector2((float)3993.993, (float)1621.394));
            PolyBot.Add(new Vector2((float)6560.813, (float)1595.721));
            PolyBot.Add(new Vector2((float)6563.053, (float)1595.72));
            PolyBot.Add(new Vector2((float)7304.941, (float)1665.319));
            PolyBot.Add(new Vector2((float)11348.71, (float)1878.183));
            PolyBot.Add(new Vector2((float)11346.32, (float)1878.186));
            PolyBot.Add(new Vector2((float)11872.98, (float)2665.871));
            PolyBot.Add(new Vector2((float)12439.73, (float)3205.568));
            PolyBot.Add(new Vector2((float)12439.34, (float)3202.141));
            PolyBot.Add(new Vector2((float)13058.5, (float)3794.414));
            PolyBot.Add(new Vector2((float)13122.41, (float)9787.474));
            PolyBot.Add(new Vector2((float)13122.24, (float)9790.635));
            PolyBot.Add(new Vector2((float)13124.61, (float)9790.042));
            PolyBot.Add(new Vector2((float)13126.98, (float)9789.443));
            PolyBot.Add(new Vector2((float)13281.6, (float)10128.9));
            PolyBot.Add(new Vector2((float)13281.37, (float)10131.81));
            PolyBot.Add(new Vector2((float)13281.13, (float)10134.74));
            PolyBot.Add(new Vector2((float)13268.63, (float)10888.28));
            PolyBot.Add(new Vector2((float)13268.46, (float)10891.19));
            PolyBot.Add(new Vector2((float)13270.7, (float)10891.19));
            PolyBot.Add(new Vector2((float)13270.54, (float)10894.1));
            PolyBot.Add(new Vector2((float)13272.78, (float)10894.1));
            PolyBot.Add(new Vector2((float)13886.69, (float)10888.28));
            PolyBot.Add(new Vector2((float)14123.54, (float)9845.071));
            PolyBot.Add(new Vector2((float)14121.51, (float)9844.741));
            PolyBot.Add(new Vector2((float)14171.9, (float)6370.759));
            PolyBot.Add(new Vector2((float)14147.16, (float)4213.343));
            PolyBot.Add(new Vector2((float)13407.83, (float)2513.221));
            PolyBot.Add(new Vector2((float)12254.61, (float)1316.929));
            PolyBot.Add(new Vector2((float)10873.14, (float)896.8494));
            PolyBot.Add(new Vector2((float)10870.9, (float)896.8506));
            PolyBot.Add(new Vector2((float)10868.37, (float)894.0178));
            PolyBot.Add(new Vector2((float)10856.9, (float)891.1924));
            PolyBot.Add(new Vector2((float)10843.21, (float)888.3785));
            PolyBot.Add(new Vector2((float)8523.145, (float)879.8599));
            PolyBot.Add(new Vector2((float)5363.11, (float)865.3258));
            PolyBot.Add(new Vector2((float)4006.466, (float)827.9229));
        }
    }
}
