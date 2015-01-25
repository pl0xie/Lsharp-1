using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Geometry = LeagueSharp.Common.Geometry;
using Color = System.Drawing.Color;

namespace Autocombo
{
internal class Autocombo
{

public static Obj_AI_Hero Player;
public Menu Config;
public static Spell R;
public DamageSpell Allydamage;
public DamageSpell Mydamage;
private static Geometry.Polygon.Rectangle _skillshot;

public Autocombo()
{
    CustomEvents.Game.OnGameLoad += OnGameLoad;
}

private void OnGameLoad(EventArgs args)
{
    Player = ObjectManager.Player;
    Config = new Menu("Auto Sick Combo", "AutoCombo", true);
    Config.AddSubMenu(new Menu("AutoCombo Settings", "AutoCombo"));
    Config.SubMenu("AutoCombo").AddItem(new MenuItem("Killable", "Combo Only Killable?").SetValue(true));
    Config.AddToMainMenu();
    Game.PrintChat("<font color='#F7A100'>Auto Combo by XcxooxL Loaded 1.0 .</font>");
    Game.PrintChat("<font color='#F7A100'>Credits to Diabaths and Pingo for helping me test =]]] </font>");
    checkChamp();
    setUltimate();

    Drawing.OnEndScene += Drawing_OnEndScene;
    Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
}

private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
{
    if (sender.IsAlly && !sender.IsMinion && !sender.IsMe)//&& !sender.IsMe)
    {
string[] spelllist = {"EzrealTrueshotBarrage", "LuxMaliceCannon" ,"EnchantedCrystalArrow", "DravenRCast", "FizzMarinerDoom", "GravesChargeShot",
 "RivenFengShuiEngine", "SejuaniGlacialPrisonStart", "shyvanatransformcast", "SonaCrescendo","CaitlynAceintheHole", "JinxRWrapper", "InfernalGuardian","LeonaSolarFlare","ZiggsR","UFSlash"};

        for (int i = 0; i <= spelllist.Length; i++)
        {
            if (args.SData.Name == spelllist[i])
            {
                if (i <= 12) // this is a skillshot
                {
                    _skillshot = new Geometry.Polygon.Rectangle(sender.Position, sender.Position.Extend(args.End, args.SData.CastRange.FirstOrDefault()), args.SData.LineWidth - 50);
                }
                else //this is aoe
                {
                    _skillshot = new Geometry.Polygon.Rectangle(sender.Position, sender.Position.Extend(args.End, sender.Distance(args.End)), args.SData.LineWidth - 50);
                }
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                {
                    var predict = Prediction.GetPrediction(enemy, 500f, args.SData.LineWidth - 50, args.SData.MissileSpeed);
                    var predictme = Prediction.GetPrediction(enemy, 500f, R.Width, R.Speed);


                    if (!_skillshot.IsInside(predict.UnitPosition))
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

                        if ((Allydamage.CalculatedDamage + Mydamage.CalculatedDamage) < enemy.Health && Allydamage.CalculatedDamage > enemy.Health)
                        {
                            return;
                        }
                    }

                    // Game.PrintChat("SkillShot Detected: " + args.SData.Name + " By: " + sender.BaseSkinName + " Ally Casted it right.. On : " + enemy.BaseSkinName); //Checks..

                    if (enemy.Distance(Player.Position) < 3200 && enemy.Distance(Player.Position) <= R.Range)
                    {
                        if (Player.BaseSkinName == "Riven")
                        {
                            R.Cast();
                        }
                        if (!R.IsSkillshot) // Casting for targetable spells
                        {
                            R.CastOnUnit(enemy, true);
                        }
                        else
                        {
                            R.Cast(predictme.CastPosition);
                        }
                    }
                }
            }
        }
    }
}

void Drawing_OnEndScene(EventArgs args)
{
    if (_skillshot != null)
    {
        _skillshot.Draw(Color.Blue, 2);
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