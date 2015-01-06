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

namespace VectorWriter
{
    class Program
    {
        private static Menu Config;
        private static Geometry.Polygon Poly;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            Poly = new Geometry.Polygon();
            Config = new Menu("Vector Writer", "VectorWriter", true);
            Config.AddSubMenu(new Menu("Keys", "Keys"));
            Config.SubMenu("Keys")
                .AddItem(new MenuItem("Write", "Write Polygon"))
                .SetValue(new KeyBind('X', KeyBindType.Press));
            Config.AddToMainMenu();
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("Write").GetValue<KeyBind>().Active)
            {
                if (Poly.Points.All(pa => pa != Game.CursorPos.To2D()))
                {
                    Poly.Add(Game.CursorPos.To2D());
                    Game.PrintChat(Game.CursorPos.ToString());
                        using (StreamWriter sw = File.AppendText("C:/Vector.txt"))
                        {
                            sw.WriteLine("new Vector2(" + Game.CursorPos.To2D().X + "," + Game.CursorPos.Y + ");" );
                            sw.Close();
                        }
                    
                }

            }
        }
    }
}
