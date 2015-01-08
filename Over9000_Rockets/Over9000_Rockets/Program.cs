using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using SimpleLib;

namespace Over9000_Rockets
{
    class Program
    {
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R;
        public static Menu Config;
        private static Obj_AI_Hero Player;
        private static SpellSlot _igniteSlot;
        private static float Time = 10;
        private static float ZedTime = 10;
        private static float eTime = 0;
        public static HpBarIndicator hpi = new HpBarIndicator();
        



        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
              Drawing.OnEndScene += OnEndScene;
        }
        
        private static void OnEndScene(EventArgs args)
        {
            foreach (
    var enemy in
        ObjectManager.Get<Obj_AI_Hero>()
            .Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
            {
                hpi.unit = enemy;
                hpi.drawDmg(CalcDamage(enemy), Color.DarkGreen);
            }
        }
        
        private static void OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 630); //need to update range
            R = new Spell(SpellSlot.R, 630); // need to update range

            W.SetSkillshot(0.25f, 80f, 1150, true, SkillshotType.SkillshotLine); // need to update values
            E.SetTargetted(0.25f, 2000f);

            Config = new Menu("Over9000 Rockets", "Tristana", true);
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q?").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseW", "Use W?").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseE", "Use E?").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R?").SetValue(true));
            Config.AddSubMenu(new Menu("Harras", "Harras"));
            Config.SubMenu("Harras").AddItem(new MenuItem("UseEH", "Use E?")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UsePackets", "Use Packets?").SetValue(false));
            Config.AddToMainMenu();

            _igniteSlot = Player.GetSpellSlot("SummonerDot");
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Game.OnGameUpdate += game_Update;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }
        // wtf is wrong with gapcloser fps abuse!?

        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.BaseSkinName == "Zed" && sender.IsEnemy && args.SData.Name.ToLower() == "zedult")
            {
                ZedTime = Game.Time;
            }

        }

        private static void game_Update(EventArgs args)
        {
            E.Range = 630 + (Player.Level - 1) * 9;
            R.Range = E.Range;
            if (Orbwalker.ActiveMode.ToString().ToLower() == "combo")
            {
                Combo();
            }
            if (Orbwalker.ActiveMode.ToString().ToLower() == "mixed")
            {
                Harras();
            }
            EscapeCombo();
        }

        private static void CastE(Obj_AI_Base unit)
        {
            eTime = Game.Time;
            E.CastOnUnit(unit, UsePackets());
        }
        public static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (W.IsReady())
            {
                Time = Game.Time;
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (CalcDamage(gapcloser.Sender) > gapcloser.Sender.Health * 1.5) //NO YOU DONT.. run away ^^
                    {
                        W.Cast(gapcloser.End);
                    }
                    else
                    {
                        if (W.GetDamage(hero) > hero.Health + 50 && hero.Distance(Player) < W.Range) // if anyone else is killable
                        {
                            W.Cast(hero.Position);
                        }
                        else
                        {
                            Vector2 dada = new Vector2(gapcloser.End.Extend(
                                Player.Position, Player.Distance(gapcloser.End) + W.Range).X, gapcloser.End.Extend(
                                Player.Position, Player.Distance(gapcloser.End) + W.Range).Y);
                            dada.Normalize();         
                            if (!dada.IsWall() && !dada.To3D().UnderTurret(true) && dada.To3D().CountEnemysInRange(700) <= 1) //try to find better escape
                            {
                                W.Cast(dada);
                            }
                            else
                            {
                                Escape(); //use full escape mechanism
                            }
                        }
                    }
                }
            }
            else //if not killable or more then 2 enemies around..
            {
                if (R.IsReady() && (Game.Time - Time > 1) && (CalcDamage(gapcloser.Sender) < gapcloser.Sender.Health) || Player.CountEnemysInRange(1000) > 2)
                {
                    R.CastOnUnit(gapcloser.Sender, UsePackets());
                }
            }

        }

        private static void Harras()
        {
            var vTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (E.IsReady() && Config.Item("UseEH").GetValue<bool>())
            {
                CastE(vTarget);
            }

        }
        private static void EscapeCombo()
        {
            var enemycount = 0;
            var zed = 0;
            var pos = Player.Position.To2D().Extend(Player.Direction.To2D(), W.Range).To3D();
            pos.Normalize();
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsEnemy && Player.Distance(enemy) < enemy.AttackRange + enemy.BoundingRadius + Player.BoundingRadius + 50)
                {
                    enemycount++;
                }

                if (Player.HasBuff("zedulttargetmark", true) && enemy.BaseSkinName == "Zed" && enemy.IsTargetable)
                {
                    zed = 1; // move this to regular escape
                    //regular escape should be in Ongameupdate  
                }
            }
            if (enemycount >= 3 || zed == 1)
            {
                if (ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy && hero.Distance(Player.Position) <= W.Range && CalcDamage(hero) > hero.Health).ToList().Count == 0)
                {
                    Escape();
                }

            }
        }
        private static void Escape() //gapcloser
        {
            var angle = Geometry.DegreeToRadian(30);

                    for (var i = 1; i < 13; i++)
                    {
                        var newpos = Geometry.RotateAroundPoint(Player.Position.To2D(), Player.Position.To2D(), angle * i);
                        newpos.Normalize();
                        if (!Player.Position.UnderTurret(true) && Player.Position.UnderTurret(false))
                        {
                            W.Cast(newpos);
                        }
                        else
                        {
                            if (!newpos.IsWall() && newpos.To3D().CountEnemysInRange(700) <= 1 && !newpos.To3D().UnderTurret(true))
                            {
                                W.Cast(newpos);
                            }
                        }
                    }
        }
        private static int CalcDamage(Obj_AI_Base target)
        {
            //var vTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            // normal damage 2 Auto Attacks
            var AA = Player.CalcDamage(target, Damage.DamageType.Physical, Player.FlatPhysicalDamageMod + Player.BaseAttackDamage) * (1 + Player.Crit);
            var damage = AA;

            if (_igniteSlot != SpellSlot.Unknown &&
        Player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);

            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                damage += Player.GetItemDamage(target, Damage.DamageItems.Botrk); //ITEM BOTRK

            if (Config.Item("UseE").GetValue<bool>()) // edamage
            {
                if (E.IsReady())
                {
                    damage += E.GetDamage(target);
                }
                else
                {
                    if (target.HasBuff("explosiveshotdebuff", true))
                    {
                        damage += (((eTime + 5 - Game.Time) * E.GetDamage(target))/5);
                    }
                }           
            }
            if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>()) // qdamage
            {
                damage += AA * 2;
            }

            if (R.IsReady() && Config.Item("UseR").GetValue<bool>()) // rdamage
            {

                damage += R.GetDamage(target);
            }
            return (int)damage;
        }


        private static void Combo()
        {
            var vTarget = TargetSelector.GetTarget(W.Range + Player.AttackRange, TargetSelector.DamageType.Physical);

            if (CalcDamage(vTarget) > vTarget.Health && W.IsReady() && vTarget.CountEnemysInRange(700) < 3 && !vTarget.Position.UnderTurret(true))
            {
                W.Cast(vTarget.Position, UsePackets());
            }
            
            if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>() && vTarget.Distance(Player.Position) < Player.AttackRange) //Q Logic
            {
                Q.Cast(UsePackets());
            }

            if (Config.Item("UseE").GetValue<bool>() && E.IsReady() && vTarget.Distance(Player.Position) < E.Range)
            {
                CastE(vTarget);
            }

            if (Config.Item("UseR").GetValue<bool>() && R.IsReady() && vTarget.Distance(Player.Position) < R.Range && (R.GetDamage(vTarget) > vTarget.Health || !E.IsReady() && CalcDamage(vTarget) > vTarget.Health))
            {
                R.CastOnUnit(vTarget, UsePackets());
            }

            if (!Player.HasBuff("zedulttargetmark", true))
            {
                return;
            }
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.BaseSkinName == "Zed" && hero.IsTargetable && !W.IsReady() && Player.Distance(hero) <= hero.AttackRange+hero.BoundingRadius+Player.BoundingRadius + 50 && Game.Time - ZedTime > 3)) {
                R.CastOnUnit(hero,UsePackets());
            }
        }

        private static bool UsePackets()
        {
            return Config.Item("UsePackets").GetValue<bool>();
        }
    }
}
