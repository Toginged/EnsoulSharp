using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Prediction;
using EnsoulSharp.SDK.Utility;
using SPrediction;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Easy_Mid.Champions
{
    class Brand
    {
        private static Menu _menu;
        private static Spell q, w, e, r;
        private static SpellSlot ignite;
        private static AIHeroClient _player { get { return ObjectManager.Player; } }
        #region
        //Combat
        public static readonly MenuBool comboQ = new MenuBool("comboQ", "Use Q on Combo");
        public static readonly MenuBool Qstun = new MenuBool("qstun", "^ Only use Q if target has passive");
        public static readonly MenuBool comboW = new MenuBool("comboW", "Use W on Combo");
        public static readonly MenuBool comboE = new MenuBool("comboE", "Use E on Combo");
        public static readonly MenuBool comboR = new MenuBool("comboR", "Use R on Combo");
        public static readonly MenuSlider Raoe = new MenuSlider("raoe", "^ Only use R if hits X enemies", 2, 1, 5);

        //Harass
        public static readonly MenuBool harassQ = new MenuBool("harassQ", "Use Q on Harass");
        public static readonly MenuBool harassW = new MenuBool("harassW", "Use W on Harass");
        public static readonly MenuBool harassE = new MenuBool("harassE", "Use E on Harass");
        public static readonly MenuSlider harassmana = new MenuSlider("harassmana", "^ Mana >= X%", 60, 0, 100);

        //Push Wave
        public static readonly MenuBool laneE = new MenuBool("laneE", "Use E on Clear Wave");
        public static readonly MenuBool laneW = new MenuBool("laneW", "Use W on Clear Wave");
        public static readonly MenuSlider clearsmana = new MenuSlider("clearsmana", "Clear Wave Mana >= X%", 60, 0, 100);

        //MISC
        public static readonly MenuBool autoKS = new MenuBool("autoKS", "Auto Try KS With Q, W, E");
        public static readonly MenuBool autoGap = new MenuBool("autoGap", "Auto Try Cast W+Q on Gapcloser");

        //Hit Chance
        public static readonly MenuList qhit = new MenuList("qhit", "Q - HitChance :", new[] { "High", "Medium", "Low" });
        public static readonly MenuList whit = new MenuList("whit", "W - HitChance :", new[] { "High", "Medium", "Low" });

        //Draw
        public static readonly MenuBool Qd = new MenuBool("qd", "Draw Q Range");
        public static readonly MenuBool Wd = new MenuBool("wd", "Draw W Range");
        public static readonly MenuBool Ed = new MenuBool("ed", "Draw E Range");
        public static readonly MenuBool Rd = new MenuBool("rd", "Draw R Range and Range around the target");
        #endregion

        public static void OnLoad()
        {
            q = new Spell(SpellSlot.Q, 1050);
            w = new Spell(SpellSlot.W, 900);
            e = new Spell(SpellSlot.E, 625);
            r = new Spell(SpellSlot.R, 750);

            q.SetSkillshot(0.25f, 60, 1550, true, SkillshotType.Line);
            w.SetSkillshot(1, 250, float.MaxValue, false, SkillshotType.Circle);
            e.SetTargetted(0.25f, float.MaxValue);
            r.SetTargetted(0.25f, 1000);

            ignite = _player.GetSpellSlot("summonerdot");

            MenuCreate();
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += OnDraw;
        }

        private static void MenuCreate()
        {
            var _menu = new Menu("easymidbrand", "Easy_Mid.Brand", true);
            var hitconfig = new Menu("hitconfig", "[HIT CHANCE] Settings");
            hitconfig.Add(qhit);
            hitconfig.Add(whit);

            var combat = new Menu("combat", "[COMBO] Settings");
            combat.Add(comboQ);
            combat.Add(Qstun);
            combat.Add(comboW);
            combat.Add(comboE);
            combat.Add(comboR);
            combat.Add(Raoe);

            var harass = new Menu("harass", "[HARASS] Settings");
            harass.Add(harassQ);
            harass.Add(harassW);
            harass.Add(harassE);
            harass.Add(harassmana);

            var clearwave = new Menu("clearwave", "[CLEAR WAVE] Settings");
            clearwave.Add(laneE);
            clearwave.Add(laneW);
            clearwave.Add(clearsmana);

            var misc = new Menu("misc", "[MISC] Settings");
            misc.Add(autoKS);
            misc.Add(autoGap);

            var draw = new Menu("draw", "[DRAW] Settings");
            draw.Add(Qd);
            draw.Add(Wd);
            draw.Add(Ed);
            draw.Add(Rd);

            var pred = new Menu("spred", "[SPREDICTION] Settings");
            SPrediction.Prediction.Initialize(pred);

            _menu.Add(hitconfig);
            _menu.Add(combat);
            _menu.Add(harass);
            _menu.Add(clearwave);
            _menu.Add(misc);
            _menu.Add(draw);
            _menu.Add(pred);
            _menu.Attach();
        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (_player.IsDead)
                return;


            if (autoKS.Enabled)
                KS();

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    DoCombo();
                    break;
                case OrbwalkerMode.Harass:
                    DoHarass();
                    break;
                case OrbwalkerMode.LaneClear:
                    DoLaneClear();
                    break;
            }
        }

        private static void OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.IsRecalling())
            {
                return;
            }

            if (MenuGUI.IsChatOpen || MenuGUI.IsShopOpen)
            {
                return;
            }
            if (!autoGap.Enabled)
                return;
            try
            {
                if (w.IsReady() && sender.IsValidTarget(w.Range))
                {
                    w.SPredictionCast(sender, HitChance.Medium);
                }
                if (q.IsReady() && sender.IsValidTarget(q.Range))
                {
                    q.SPredictionCast(sender, HitChance.Medium);
                }
            }
            catch
            {
                //error
            }
        }

        private static bool HasPassive(AIBaseClient unit)
        {
            return unit.HasBuff("brandablaze");
        }

        private double getComboDamage(AIHeroClient target)
        {
            double damage = _player.GetAutoAttackDamage(target);
            if (q.IsReady() && comboQ.Enabled)
                damage += _player.GetSpellDamage(target, SpellSlot.Q);
            if (w.IsReady() && comboW.Enabled)
                damage += _player.GetSpellDamage(target, SpellSlot.W);
            if (e.IsReady() && comboE.Enabled)
                damage += _player.GetSpellDamage(target, SpellSlot.E);
            if (r.IsReady() && comboR.Enabled)
                damage += _player.GetSpellDamage(target, SpellSlot.R);
            if (ignite.IsReady())
                damage += _player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            return damage;
        }


        public static HitChance hitchanceW()
        {
            var hit = HitChance.High;
            switch (whit.Index)
            {
                case 0:
                    hit = HitChance.High;
                    break;
                case 1:
                    hit = HitChance.Medium;
                    break;
                case 2:
                    hit = HitChance.Low;
                    break;
            }
            return hit;
        }

        public static HitChance hitchanceQ()
        {
            var hit = HitChance.High;
            switch (qhit.Index)
            {
                case 0:
                    hit = HitChance.High;
                    break;
                case 1:
                    hit = HitChance.Medium;
                    break;
                case 2:
                    hit = HitChance.Low;
                    break;
            }
            return hit;
        }

        private static void DoCombo()
        {
            var target = TargetSelector.GetTarget(w.Range - 50);
            var hitQ = hitchanceQ();
            var hitW = hitchanceW();

            if (target != null && target.IsValidTarget(e.Range))
            {
                if (e.IsReady() && comboE.Enabled)
                {
                    e.Cast(target,true);
                }
                if(Qstun.Enabled && q.IsReady() && HasPassive(target) && comboQ.Enabled)
                {
                    q.SPredictionCast(target, hitQ);
                }
                else if (q.IsReady() && comboQ.Enabled && !Qstun.Enabled)
                {
                    q.SPredictionCast(target, hitQ);
                }
                if (w.IsReady() && HasPassive(target) && comboW.Enabled)
                {
                    w.SPredictionCast(target, hitW);
                }
                if (r.IsReady() && HasPassive(target) && comboR.Enabled)
                {
                    if(Raoe.Value > 1)
                    {
                        if(target.CountEnemyHeroesInRange(750) >= Raoe.Value)
                        {
                            r.Cast(target, true);
                        }
                    }
                    else
                    {
                        r.Cast(target, true);
                    }
                }
            }
            if(target != null && target.IsValidTarget(w.Range))
            {
                if (w.IsReady() && comboW.Enabled)
                {
                    w.SPredictionCast(target, hitW);
                }
                if (e.IsReady() && comboE.Enabled && target.IsValidTarget(e.Range))
                {
                    e.Cast(target, true);
                }
                if (Qstun.Enabled && q.IsReady() && HasPassive(target) && comboQ.Enabled)
                {
                    q.SPredictionCast(target, hitQ);
                }
                else if (q.IsReady() && comboQ.Enabled && !Qstun.Enabled)
                {
                    q.SPredictionCast(target, hitQ);
                }
                if (r.IsReady() && HasPassive(target) && comboR.Enabled)
                {
                    if (Raoe.Value > 1)
                    {
                        if (target.CountEnemyHeroesInRange(750) >= Raoe.Value)
                        {
                            r.Cast(target, true);
                        }
                    }
                    else
                    {
                        r.Cast(target, true);
                    }
                }
            }
        }

        public static void DoHarass()
        {
            var target = TargetSelector.GetTarget(w.Range - 50);
            var hitQ = hitchanceQ();
            var hitW = hitchanceW();

            if (target == null)
                return;

            if (_player.ManaPercent < harassmana.Value)
                return;

            if (target.IsValidTarget(e.Range))
            {
                if (e.IsReady() && harassE.Enabled)
                {
                    e.Cast(target, true);
                }
                if (q.IsReady() && harassQ.Enabled)
                {
                    q.SPredictionCast(target, hitQ);
                }

                if (w.IsReady() && HasPassive(target) && harassW.Enabled)
                {
                    w.SPredictionCast(target, hitW);
                }
            }
            if (target.IsValidTarget(w.Range))
            {
                if (w.IsReady() && harassW.Enabled)
                {
                    w.SPredictionCast(target, hitW);
                }
                if (e.IsReady() && harassE.Enabled)
                {
                    e.Cast(target, true);
                }
                if (q.IsReady() && harassQ.Enabled)
                {
                    q.SPredictionCast(target, hitQ);
                }
            }

        }

        private static void DoLaneClear()
        {
            List<AIBaseClient> minions = MinionManager.GetMinions(_player.Position, w.Range);
            var wCastLocation = w.GetCircularFarmLocation(minions, w.Width);
            if (w.IsReady() && wCastLocation.MinionsHit > 2 && laneW)
            {
                w.Cast(wCastLocation.Position);
            }
            if (e.IsReady() && laneE)
            {
                foreach (AIBaseClient minion in minions)
                {
                    if (HasPassive(minion) && _player.Distance(minion.Position) < e.Range)
                    {
                        e.Cast(minion);
                        break;
                    }
                }
            }
        }

        private static void KS()
        {
            var wtarget = TargetSelector.GetTarget(w.Range);
            if(wtarget != null && wtarget.IsValidTarget(w.Range) && wtarget.Health < w.GetDamage(wtarget))
            {
                w.SPredictionCast(wtarget, HitChance.Medium);
            }
            if (wtarget != null && wtarget.IsValidTarget(q.Range) && wtarget.Health < q.GetDamage(wtarget))
            {
                q.SPredictionCast(wtarget, HitChance.Medium);
            }
            if (wtarget != null && wtarget.IsValidTarget(e.Range) && wtarget.Health < e.GetDamage(wtarget))
            {
                e.Cast(wtarget, true);
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.IsRecalling())
            {
                return;
            }

            if (MenuGUI.IsChatOpen || MenuGUI.IsShopOpen)
            {
                return;
            }
            if (Qd.Enabled && q.IsReady())
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, q.Range, Color.FromArgb(48, 120, 252), 1);
            }
            if (Wd.Enabled && w.IsReady())
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, w.Range, Color.FromArgb(120, 120, 252), 1);
            }
            if (Ed.Enabled && e.IsReady())
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, e.Range, Color.FromArgb(120, 252, 252), 1);
            }
            try
            {
                if (Rd.Enabled && r.IsReady())
                {
                    Render.Circle.DrawCircle(GameObjects.Player.Position, r.Range, Color.FromArgb(255, 10, 10), 1);
                    var t = TargetSelector.GetTarget(2000f);
                    if(t != null)
                    {
                        Render.Circle.DrawCircle(t.Position, r.Range, Color.FromArgb(255, 10, 10), 1);
                    }
                }
            }
            catch
            {
                //error
            }
        }
    }
}
