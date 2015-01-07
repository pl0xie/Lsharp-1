using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
//using Color = System.Drawing.Color;

namespace Over9000_Rockets
{
    class Program
    {
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R;
        public static Menu Config;
        private static Obj_AI_Hero Player;
        private static SpellSlot _igniteSlot;



        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
            //  Drawing.OnEndScene += OnEndScene;
        }
        /*
        private static void OnEndScene(EventArgs args)
        {

        }
        */
        private static void OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 550); //need to update range
            R = new Spell(SpellSlot.R, 550); // need to update range

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

        }

        private static void game_Update(EventArgs args)
        {
            if (Orbwalker.ActiveMode.ToString().ToLower() == "combo")
            {
                Combo();
            }
            if (Orbwalker.ActiveMode.ToString().ToLower() == "mixed")
            {
                Harras();
            }
        }

        public static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (W.IsReady())
            {
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
                            var pos = gapcloser.End.Extend(
                                Player.Position, Player.Distance(gapcloser.End) + W.Range);

                            if (!pos.IsWall() && !pos.UnderTurret(true) && pos.CountEnemysInRange(700) <= 1) //try to find better escape
                            {
                                W.Cast(pos);
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
                if (R.IsReady() && (W.Instance.CooldownExpires - Game.Time > W.Instance.Cooldown - 0.5) && (CalcDamage(gapcloser.Sender) < gapcloser.Sender.Health) || Player.CountEnemysInRange(1000) > 2)
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
                E.CastOnUnit(vTarget, UsePackets());
            }

        }
        private static void Escape()
        {
            var enemycount = 0;
            var zed = 0;
            var pos = Player.Position.To2D().Extend(Player.Direction.To2D(), W.Range).To3D();
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsEnemy && Player.Distance(enemy) < enemy.AttackRange)
                {
                    enemycount++;
                }
                if (Player.HasBuff("zedulttargetmark", true) && enemy.BaseSkinName == "Zed" && enemy.IsValid)
                {
                    zed = 1;
                }

            }
            if (enemycount >= 3 || zed == 1)
            {
                if (ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy && hero.Distance(Player.Position) < W.Range && CalcDamage(hero) > hero.Health).ToList().Count == 0)
                {
                    var angle = Geometry.DegreeToRadian(45);
                    for (var i = 1; i < 9; i++)
                    {
                        var newpos = pos.To2D().RotateAroundPoint(Player.Position.To2D(), angle * i);
                        if (!pos.UnderTurret(true) && pos.UnderTurret(false))
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

            if (E.IsReady() && Config.Item("UseE").GetValue<bool>()) // edamage
            {
                damage += E.GetDamage(target);
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
                W.Cast(vTarget.Position);
            }
            Escape();
            if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>() && vTarget.Distance(Player.Position) < Player.AttackRange) //Q Logic
            {
                Q.Cast(UsePackets());
            }

            if (Config.Item("UseE").GetValue<bool>() && E.IsReady() && vTarget.Distance(Player.Position) < E.Range)
            {
                E.CastOnUnit(vTarget, UsePackets());
            }

            if (Config.Item("UseR").GetValue<bool>() && R.IsReady() && vTarget.Distance(Player.Position) < R.Range && R.GetDamage(vTarget) > vTarget.Health + 50)
            {
                R.CastOnUnit(vTarget, UsePackets());
            }
        }

        private static bool UsePackets()
        {
            return Config.Item("UsePackets").GetValue<bool>();
        }
    }
}
