using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Over9000_Rockets
{
    class Program
    {
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R;
        public static Menu Config;
        private static Obj_AI_Hero _player;
        private static SpellSlot _igniteSlot;
        private static float _time = 10;

        private static float _eTime;

        //Anti champs logic
        private static float _zedTime = 10;
        private static bool Fizz = false;


        //end
        public static HpBarIndicator Hpi = new HpBarIndicator();




        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
            Drawing.OnEndScene += OnEndScene;
        }

        private static void OnEndScene(EventArgs args)
        {
            if (Config.SubMenu("Settings").Item("DrawD").GetValue<bool>())
            {
                foreach (var enemy in
                    ObjectManager.Get<Obj_AI_Hero>().Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
                {
                    Hpi.unit = enemy;
                    Hpi.drawDmg(CalcDamage(enemy), Color.DarkGreen);
                }
            }

        }

        private static void OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 630);
            R = new Spell(SpellSlot.R, 630);

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
            Config.SubMenu("Combo")
                .AddItem(new MenuItem("PressR", "Cast R").SetValue(new KeyBind('R', KeyBindType.Press)));

            Config.SubMenu("Combo")
               .AddItem(new MenuItem("Escape", "Escape").SetValue(new KeyBind('Z', KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Harras", "Harras"));
            Config.SubMenu("Harras").AddItem(new MenuItem("UseEH", "Use E?")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UsePackets", "Use Packets?").SetValue(false));
            Config.AddSubMenu(new Menu("Settings", "Settings"));
            Config.SubMenu("Settings").AddItem(new MenuItem("DrawD", "Draw damage?").SetValue(true));
            Config.AddToMainMenu();

            _igniteSlot = _player.GetSpellSlot("SummonerDot");

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.BaseSkinName == "Fizz")
                {
                    Fizz = true;
                }
            }


            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Game.OnGameUpdate += GameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Orbwalking.AfterAttack += OrbwalkingAfterAttack;
        }
        static void OrbwalkingAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
           
            var vTarget = target as Obj_AI_Hero;
            if (vTarget == null || !unit.IsMe || Orbwalker.ActiveMode.ToString().ToLower() != "combo")
            {
                return;
            }


            if (Config.Item("UseE").GetValue<bool>() && E.IsReady() && vTarget.Distance(_player.Position) <= E.Range)
            {
                CastE(vTarget);
            }

            var damage = _player.GetAutoAttackDamage(vTarget, true) + R.GetDamage(vTarget);


            if (vTarget.HasBuff("explosiveshotdebuff", true))
            {
                damage += ((((_eTime - Game.Time) * E.GetDamage(vTarget)) / 5) - ((vTarget.HPRegenRate/2) * (_eTime - Game.Time)) );
            }

            if ((damage > vTarget.Health) && ((damage - _player.GetAutoAttackDamage(vTarget,true)) < vTarget.Health) &&
                R.IsReady() && vTarget.Distance(_player.Position) <= R.Range)
            {
                R.CastOnUnit(vTarget, UsePackets());
            }


            //double jump combo?
            //maybe check auto attack resets?
            
        }
        // wtf is wrong with gapcloser fps abuse!?

        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.BaseSkinName == "Zed" && sender.IsEnemy && args.SData.Name.ToLower() == "zedult")
            {
                _zedTime = Game.Time;
            }

        }

        private static void GameUpdate(EventArgs args)
        {
            var vTarget = TargetSelector.GetTarget(W.Range + _player.AttackRange, TargetSelector.DamageType.Physical);
            E.Range = 630 + (_player.Level - 1) * 9;
            R.Range = E.Range;
            if (Orbwalker.ActiveMode.ToString().ToLower() == "combo")
            {
                Combo(vTarget);
            }
            if (Config.SubMenu("Combo").Item("PressR").GetValue<KeyBind>().Active)
            {
                R.CastOnUnit(vTarget, UsePackets());
            }
            if (Config.SubMenu("Combo").Item("Escape").GetValue<KeyBind>().Active)
            {
                Escape();
            }
            if (Orbwalker.ActiveMode.ToString().ToLower() == "mixed")
            {
                Harras();
            }
            EscapeCombo();
        }

        private static void CastE(Obj_AI_Base unit)
        {
            _eTime = 5 + Game.Time + _player.Distance(unit) / E.Instance.SData.MissileSpeed;
            E.CastOnUnit(unit, UsePackets());
        }
        public static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (W.IsReady())
            {
                _time = Game.Time;

                if (CalcDamage(gapcloser.Sender) > gapcloser.Sender.Health && gapcloser.End.CountEnemysInRange(700) < 2) //NO YOU DONT.. run away ^^
                {
                    W.Cast(gapcloser.End);
                    return;
                }
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
                {
                        if (W.GetDamage(hero) > hero.Health + 50 && hero.Distance(_player) < W.Range) // if anyone else is killable
                        {
                            W.Cast(hero.Position);
                            return;
                        }
                }
                var dada = new Vector2(gapcloser.End.Extend(
                    _player.Position, _player.Distance(gapcloser.End) + W.Range).X, gapcloser.End.Extend(
                    _player.Position, _player.Distance(gapcloser.End) + W.Range).Y);
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
            else //if not killable or more then 2 enemies around..
            {
                if (R.IsReady() && (Game.Time - _time > 1) && (CalcDamage(gapcloser.Sender) < gapcloser.Sender.Health) || _player.CountEnemysInRange(1000) > 2)
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
            var melee = 0;
            var zed = 0;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsEnemy && _player.Distance(enemy) < enemy.AttackRange + enemy.BoundingRadius)
                {
                    if (enemy.IsMelee())
                    {
                        melee++;
                    }
                    enemycount++;
                }

                if (_player.HasBuff("zedulttargetmark", true) && enemy.BaseSkinName == "Zed" && enemy.IsTargetable)
                {
                    zed = 1; // move this to regular escape
                    //regular escape should be in Ongameupdate  
                }
            }

            if (enemycount >= 3 || zed == 1 || melee > 1)
            {
                if (ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy && hero.Distance(_player.Position) <= W.Range && CalcDamage(hero) > hero.Health).ToList().Count == 0)
                {

                    Escape();
                }

            }
        }
        private static void Escape() //gapcloser
        {
            if (!W.IsReady())
            {
                return;
            }
            var angle = Geometry.DegreeToRadian(30);

            for (var i = 1; i < 13; i++)
            {
                var newpos = _player.Position.To2D().Extend(_player.Direction.To2D(), W.Range).RotateAroundPoint(_player.Position.To2D(), angle * i);

                if (!_player.Position.UnderTurret(true) && _player.Position.UnderTurret(false))
                {
                    W.Cast(newpos);
                }
            }

            for (var i = 1; i < 13; i++)
            {
                var newpos = _player.Position.To2D().Extend(_player.Direction.To2D(), W.Range).RotateAroundPoint(_player.Position.To2D(), angle * i);
                    if (!newpos.IsWall() && newpos.To3D().CountEnemysInRange(700) <= 1 && !newpos.To3D().UnderTurret(true))
                    {
                        W.Cast(newpos);
                    }
            }


        }
        private static int CalcDamage(Obj_AI_Base target)
        {
            //var vTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            // normal damage 2 Auto Attacks
            var aa = _player.GetAutoAttackDamage(target, true) * (1 + _player.Crit);
            var damage = aa;

            if (_igniteSlot != SpellSlot.Unknown &&
        _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);

            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                damage += _player.GetItemDamage(target, Damage.DamageItems.Botrk); //ITEM BOTRK

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
                        damage += (((_eTime - Game.Time) * E.GetDamage(target)) / 5);
                    }
                }
            }

            if (R.IsReady() && Config.Item("UseR").GetValue<bool>()) // rdamage
            {

                damage += R.GetDamage(target);
            }

            if (W.IsReady())
            {
                damage += W.GetDamage(target);
            }
            return (int)damage;
        }


        private static void Combo(Obj_AI_Hero vTarget)
        {
            if (Fizz)
            {
                if (ObjectManager.Get<Obj_AI_Hero>().Any(hero => vTarget != hero && hero.BaseSkinName == "Fizz" && !hero.IsTargetable && hero.Distance(_player) < _player.AttackRange && vTarget.Health > CalcDamage(vTarget))) {
                    return;
                }
            }


            if (CalcDamage(vTarget) > vTarget.Health && W.IsReady() && vTarget.CountEnemysInRange(700) < 3 && !vTarget.Position.UnderTurret(true))
            {
                W.Cast(vTarget.ServerPosition, UsePackets());
            }

            if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>() && vTarget.Distance(_player.Position) <= _player.AttackRange) //Q Logic
            {
                Q.Cast(UsePackets());
            }

            if (Config.Item("UseR").GetValue<bool>() && R.IsReady() && vTarget.Distance(_player.Position) <= R.Range)
            {
                if (R.GetDamage(vTarget) > vTarget.Health)
                {
                    R.CastOnUnit(vTarget, UsePackets());
                }
            }

            if (!_player.HasBuff("zedulttargetmark", true))
            {
                return;
            }
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.BaseSkinName == "Zed" && hero.IsTargetable && !W.IsReady() && _player.Distance(hero) <= hero.AttackRange + hero.BoundingRadius + _player.BoundingRadius + 50 && Game.Time - _zedTime > 3))
            {
                R.CastOnUnit(hero, UsePackets());
            }
        }

        private static bool UsePackets()
        {
            return Config.Item("UsePackets").GetValue<bool>();
        }
    }
}
