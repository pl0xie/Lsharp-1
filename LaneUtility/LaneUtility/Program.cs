using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing.Text;
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
            Rekt = new Geometry.Rectangle(new Vector2(3522, 3538), new Vector2(11824, 11870), 400);
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
        }


        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Config.Item("Freeze").GetValue<KeyBind>().Active)
            {
                return;
            }
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
                        foreach (var point in PolyBot.Points)
                        {
                            if (point.Distance(Game.CursorPos.To2D()) < 200)
                            {
                                Game.PrintChat(point.ToString());
                            }
                        }
                    }
                    else
                    {
                        foreach (var point in PolyTop.Points)
                        {
                            if (point.Distance(Game.CursorPos.To2D()) < 200)
                            {
                                Game.PrintChat(point.ToString());
                            }
                        }
                    }
                }
            }

        }
        private static void CheckPolygon(Geometry.Polygon Poly)
        {
            var enemyhealth = 0.0;
            var allyhealth = 0.0;
            var allycount = 0.0;
            var damage = 0.0;

            List<Obj_AI_Minion> enemyList = new List<Obj_AI_Minion>();

            //maybe add distance check for ally minions above 1000 are not relevent !
            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => !Poly.IsOutside(minion.Position.To2D()) && !minion.IsDead))
            {
                if (minion.IsEnemy)
                {
                    enemyList.Add(minion);
                    enemyhealth += minion.Health;
                    damage = Player.GetAutoAttackDamage(minion, true) * 2; //adjust to give more health to enemy
                }
                else
                {
                    if (minion.IsAlly) //blue team
                    {
                        if (Player.Team == GameObjectTeam.Chaos && minion.Distance(Poly.Points[10]) > Player.Distance(minion))
                        {
                            allyhealth += minion.Health;
                            allycount++;
                        }
                        else
                        {
                            if (minion.Distance(Poly.Points[0]) > Player.Distance(minion)) // must be Order team.. if not chaos.. right..? RIGHT?!!?
                            {
                                allyhealth += minion.Health;
                                allycount++;
                            }
                        }
                    }
                }
            }

            Game.PrintChat("Enemy Health : " + enemyhealth);
            //Game.PrintChat("Enemy count : " + enemyList.Count);
            Game.PrintChat("ally Health : " + allyhealth);
            //Game.PrintChat("ally Count : " + allycount);

            //attack logic comes here you can use enemyList to fine the target
            if (enemyhealth > (allyhealth + damage) && !Player.IsWindingUp)
            // example if enemy health is bigger then allyhealth + 1 half minion extra
            {
                Player.IssueOrder(GameObjectOrder.AttackUnit, enemyList.OrderBy(minion => minion.Health).First());
            }

        }
        private static void PolyTopPos()
        {
            PolyTop.Add(new Vector2((float)658.5462, (float)3846.877));// second
            PolyTop.Add(new Vector2((float)805.353, (float)10307.55));
            PolyTop.Add(new Vector2((float)945.2174, (float)11681.11));
            PolyTop.Add(new Vector2((float)1368.957, (float)12436.9));
            PolyTop.Add(new Vector2((float)2235.054, (float)13289.45));
            PolyTop.Add(new Vector2((float)3014.264, (float)13905.53));
            PolyTop.Add(new Vector2((float)3629.025, (float)14164.73));
            PolyTop.Add(new Vector2((float)5173.758, (float)14227.4));
            PolyTop.Add(new Vector2((float)10748.04, (float)14180.32));
            PolyTop.Add(new Vector2((float)10827.2, (float)13279.28)); //red team
            PolyTop.Add(new Vector2((float)9013.054, (float)13284.26));
            PolyTop.Add(new Vector2((float)6889.029, (float)13183.19));
            PolyTop.Add(new Vector2((float)3474.043, (float)12871.82));
            PolyTop.Add(new Vector2((float)3138.901, (float)12287.6));
            PolyTop.Add(new Vector2((float)2651.419, (float)11933.04));
            PolyTop.Add(new Vector2((float)1990.024, (float)11690.21));
            PolyTop.Add(new Vector2((float)1705.696, (float)10593.16));
            PolyTop.Add(new Vector2((float)1666.856, (float)8649.982));
            PolyTop.Add(new Vector2((float)1769.804, (float)6974.243));
            PolyTop.Add(new Vector2((float)1745.595, (float)4830.107));
            PolyTop.Add(new Vector2((float)1332.146, (float)3819.446)); //first
        }
        private static void PolyBotPos()
        {
            PolyBot.Add(new Vector2((float)3993.993, (float)1621.394)); // second
            PolyBot.Add(new Vector2((float)6560.813, (float)1595.721));
            PolyBot.Add(new Vector2((float)7304.941, (float)1665.319));
            PolyBot.Add(new Vector2((float)11348.71, (float)1878.183));
            PolyBot.Add(new Vector2((float)11872.98, (float)2665.871));
            PolyBot.Add(new Vector2((float)12439.73, (float)3205.568));
            PolyBot.Add(new Vector2((float)13058.5, (float)3794.414));
            PolyBot.Add(new Vector2((float)13122.41, (float)9787.474));
            PolyBot.Add(new Vector2((float)13281.6, (float)10128.9));
            PolyBot.Add(new Vector2((float)13268.63, (float)10888.28)); //red team
            PolyBot.Add(new Vector2((float)14121.51, (float)9844.741));
            PolyBot.Add(new Vector2((float)14171.9, (float)6370.759));
            PolyBot.Add(new Vector2((float)14147.16, (float)4213.343));
            PolyBot.Add(new Vector2((float)13407.83, (float)2513.221));
            PolyBot.Add(new Vector2((float)12254.61, (float)1316.929));
            PolyBot.Add(new Vector2((float)10870.9, (float)896.8506));
            //PolyBot.Add(new Vector2((float)10856.9, (float)891.1924));
            PolyBot.Add(new Vector2((float)8523.145, (float)879.8599));
            PolyBot.Add(new Vector2((float)5363.11, (float)865.3258));
            PolyBot.Add(new Vector2((float)4006.466, (float)827.9229)); //FIRST positoon
        }
    }
}
