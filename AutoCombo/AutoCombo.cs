using System;
using System.Collections.Generic;
using System.Linq;
using Evade;
using LeagueSharp;
using LeagueSharp.Common;
using Geometry = LeagueSharp.Common.Geometry;
using Color = System.Drawing.Color;

namespace Autocombo
{
    
    internal class Autocombo
    {
        private static readonly List<String> Spells = new List<string>();
        public static Obj_AI_Hero Player;
        public Menu Config;
        public static Spell R;
        public DamageSpell Allydamage;
        public DamageSpell Mydamage;
        private static Geometry.Polygon.Rectangle _skillshot;
        private static Geometry.Polygon.Circle _skillshotAOE;
        private float drawTime;


        public Autocombo()
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private void OnGameLoad(EventArgs args)
        {
            //Debugger.Launch();
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Player = ObjectManager.Player;
            Config = new Menu("Auto Sick Combo", "AutoCombo", true);
            Config.AddSubMenu(new Menu("AutoCombo Settings", "AutoCombo"));
            Config.AddSubMenu(new Menu("Spells", "AutoCombo_Spells"));
            Config.SubMenu("AutoCombo").AddItem(new MenuItem("Killable", "Combo Only Killable?").SetValue(true));
            Config.SubMenu("AutoCombo").AddItem(new MenuItem("Range", "Detect Range").SetValue(new Slider(1000, 200, 6000)));
            Config.SubMenu("AutoCombo").AddItem(new MenuItem("Draw", "Draw Range").SetValue(true));
            
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.IsAlly && !ally.IsMe)) //!ally.isme
            {
                if (!Spells.Contains(ally.GetSpell(SpellSlot.R).SData.Name) && SpellDatabase.GetByName(ally.GetSpell(SpellSlot.R).SData.Name).Range > 0)
                {
                    Spells.Add(ally.GetSpell(SpellSlot.R).SData.Name);
                    Config.SubMenu("AutoCombo_Spells").AddItem(new MenuItem(ally.GetSpell(SpellSlot.R).SData.Name, ally.BaseSkinName + " Ultimate")).SetValue(true);
                }
            }
            Config.AddToMainMenu();
            Game.PrintChat("<font color='#F7A100'>Auto Combo by XcxooxL Loaded 1.0 .</font>");
            Game.PrintChat("<font color='#F7A100'>Credits to Diabaths and Pingo for helping me test =]]] </font>");
            Drawing.OnEndScene += Drawing_OnEndScene;
            checkChamp();
            setUltimate();
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var targetable = false;
            var line = false;
            var aoe = false;
            PredictionInput input = null;
            if (sender.IsMe || sender.IsEnemy || !(sender is Obj_AI_Hero))
            {
                return;
            }
            if (!Spells.Contains(args.SData.Name) || Config.SubMenu("AutoCombo_Spells").Item(args.SData.Name).GetValue<bool>() == false)
            {
                return;
            }
            
            drawTime = Game.Time;
            var result = SpellDatabase.GetByName(args.SData.Name);

            switch (result.Type)
            {
                case SkillShotType.SkillshotLine:
                    _skillshot = new Geometry.Polygon.Rectangle(sender.Position, sender.Position.Extend(args.End, result.Range), result.Radius);
                    line = true;
                    break;
                case SkillShotType.SkillshotMissileLine:
                    _skillshot = new Geometry.Polygon.Rectangle(sender.Position, sender.Position.Extend(args.End, result.Range), result.Radius);
                    line = true;
                    break;
                case SkillShotType.SkillshotCircle:
                    _skillshotAOE = new Geometry.Polygon.Circle(args.End, result.Radius);
                    aoe = true;
                    break;
                case SkillShotType.SkillshotRing:
                    _skillshotAOE = new Geometry.Polygon.Circle(args.End, result.Radius);
                    aoe = true;
                    break;
            }
            foreach (
                var enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(enemy => enemy.IsEnemy && enemy.Distance(Player) <= Config.SubMenu("AutoCombo").Item("Range").GetValue<Slider>().Value))
            {
                if (!aoe && !line && args.Target == enemy)
                {
                    targetable = true;
                }
                if (!aoe && !line && !targetable)
                {
                    continue;
                }
                //Game.PrintChat(sender.BaseSkinName + " Ultimate Damage is : " + Allydamage.CalculatedDamage);
                //Game.PrintChat("My Ultimate Damage is : " + Mydamage.CalculatedDamage);
                //Game.PrintChat("Total damage is : " + (Allydamage.CalculatedDamage + Mydamage.CalculatedDamage));

                if (Config.Item("Killable").GetValue<bool>())
                {
                    Allydamage = sender.GetDamageSpell(enemy, args.SData.Name);
                    Mydamage = Player.GetDamageSpell(enemy, SpellSlot.R);

                    if ((Allydamage.CalculatedDamage + Mydamage.CalculatedDamage) < enemy.Health &&
                        Allydamage.CalculatedDamage > enemy.Health)
                    {
                        return;
                    }
                }

                if (line)
                {
                     input = new PredictionInput
                    {
                        Unit = enemy,
                        Type = SkillshotType.SkillshotLine,
                        Speed = result.MissileSpeed,
                        From = sender.Position,
                        Delay = result.Delay,
                        Aoe = false,
                        Radius = result.Radius,
                        Range = result.Range,
                        Collision = false
                    };
                }
                else
                {
                    if (aoe)
                    {
                        
                        input = new PredictionInput
                        {
                            Unit = enemy,
                            Type = SkillshotType.SkillshotCircle,
                            Speed = result.MissileSpeed,
                            From = sender.Position,
                            Delay = result.Delay,
                            Aoe = true,
                            Radius = result.Radius,
                            Range = result.Range,
                            Collision = false
                           
                        };
                    }
                }
                var output = Prediction.GetPrediction(input);

                var unit = output.CastPosition;
                if (line)
                {
                    if (!_skillshot.IsInside(unit) && !_skillshot.IsInside(enemy))
                    {
                        continue;
                    }
                }
                else
                {
                    if (aoe)
                    {
                        if (!_skillshotAOE.IsInside(unit) && !_skillshotAOE.IsInside(enemy))
                        {
                            continue;
                        }
                    }
                }

                if (enemy.Distance(Player.Position) <= Config.SubMenu("AutoCombo").Item("Range").GetValue<Slider>().Value)
                {
                    if (!R.IsSkillshot) // Casting for targetable spells
                    {
                        R.CastOnUnit(enemy, true);
                    }
                    else
                    {
                        R.Cast(enemy);
                    }
                }
            }
        }

        void Drawing_OnEndScene(EventArgs args)
        {
            if (_skillshot != null && Game.Time - drawTime <= 2)
            {
                _skillshot.Draw(Color.Blue, 2);
            }
            if (_skillshotAOE != null && Game.Time - drawTime <= 2)
            {
                _skillshotAOE.Draw(Color.Blue, 2);
            }
            if (Game.Time - drawTime > 2)
            {
                _skillshot = null;
                _skillshotAOE = null;
            }
            if (Config.SubMenu("AutoCombo").Item("Draw").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Player.Position, Config.SubMenu("AutoCombo").Item("Range").GetValue<Slider>().Value, Color.Red);
            }

        }

        private void checkChamp()
        {
            string[] champions = { "Ezreal", "Lux", "Ashe", "Draven", "Fizz", "Graves", "Riven", "Sona", "Jinx", "Caitlyn", "Riven" };
            for (int i = 0; i <= 9; i++)
            {
                if (Player.ChampionName == champions[i])
                {
                    Game.PrintChat("<font color='#F7A100'>Champion : " + champions[i] + " Detected And Loaded !!" + " .</font>");
                }
            }
        }

        private void setUltimate()
        {
            if (Player.BaseSkinName == "Ezreal")
            {
                R = new Spell(SpellSlot.R, 2000);
                R.SetSkillshot(0.25f, 150f, 2000f, false, SkillshotType.SkillshotLine);
            }
            if (Player.BaseSkinName == "Lux")
            {
                R = new Spell(SpellSlot.R, 3200);
                R.SetSkillshot(0.25f, 150f, 3000f, false, SkillshotType.SkillshotLine);
            }
            if (Player.BaseSkinName == "Ashe")
            {
                R = new Spell(SpellSlot.R, 2000);
                R.SetSkillshot(0.25f, 130f, 1600f, false, SkillshotType.SkillshotLine);
            }
            if (Player.BaseSkinName == "Draven")
            {
                R = new Spell(SpellSlot.R, 2000);
                R.SetSkillshot(0.25f, 120f, 2000f, false, SkillshotType.SkillshotLine);
            }
            if (Player.BaseSkinName == "Fizz")
            {
                R = new Spell(SpellSlot.R, 1275);
                R.SetSkillshot(0.25f, 80f, 1200f, false, SkillshotType.SkillshotLine);
            }
            if (Player.BaseSkinName == "Graves")
            {
                R = new Spell(SpellSlot.R, 1000);
                R.SetSkillshot(0.25f, 100f, 1400f, false, SkillshotType.SkillshotLine);
            }
            if (Player.BaseSkinName == "Sona")
            {
                R = new Spell(SpellSlot.R, 2400);
                R.SetSkillshot(0.25f, 140f, 1400f, false, SkillshotType.SkillshotLine);
            }
            if (Player.BaseSkinName == "Jinx")
            {
                R = new Spell(SpellSlot.R, 2000);
                R.SetSkillshot(0.25f, 120f, 1400f, false, SkillshotType.SkillshotLine);
            }
            if (Player.BaseSkinName == "Caitlyn")
            {
                R = new Spell(SpellSlot.R, 2000);
                R.SetTargetted(0.5f, 2000, Player.Position);
            }
            if (Player.BaseSkinName == "Riven")
            {
                R = new Spell(SpellSlot.R, 1100);
                R.SetSkillshot(0.25f, 125, 2200, false, SkillshotType.SkillshotCone);
            }
        }
    }
}