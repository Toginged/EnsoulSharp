using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Prediction;
using SPrediction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Easy_Mid.Champions
{
    class Diana
    {
        private static Spell Q, W, E, R;
        private static Menu _config;
        private static AIHeroClient _Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        #region
        //Q
        public static readonly MenuBool Qcombo = new MenuBool("qcombo", "Use [Q] on Combo");
        public static readonly MenuBool Qks = new MenuBool("qks", "Use [Q] to KS");
        public static readonly MenuBool Qharass = new MenuBool("qharass", "Use [Q] on Harass");
        public static readonly MenuBool Qfarm = new MenuBool("qfarm", "Use [Q] to farm if Minion is out AA range");
        public static readonly MenuBool Qclear = new MenuBool("qclear", "Use [Q] to Clear Wave");
        public static readonly MenuList qhit = new MenuList("qhit", "HitChance :", new[] { "High", "Medium", "Low" });

        //W
        public static readonly MenuBool Wcombo = new MenuBool("wcombo", "Use [W] on Combo");
        public static readonly MenuBool Wharass = new MenuBool("wharass", "Use [W] on Harass");
        public static readonly MenuBool Wclear = new MenuBool("wclear", "Use [W] to Clear Wave");

        //E
        public static readonly MenuBool Ecombo = new MenuBool("ecombo", "Use [E] on Combo");
        public static readonly MenuBool Eharass = new MenuBool("eharass", "Use [E] on Harass");
        public static readonly MenuBool Eint = new MenuBool("eint", "Use [E] on Interrupt Spell");

        //R
        public static readonly MenuBool Rcombo = new MenuBool("rcombo", "Use [R] on Combo");
        public static readonly MenuBool Rkill = new MenuBool("rkill", "Use R to kill even if not reset(On Combo)");
        public static readonly MenuBool Rks = new MenuBool("rks", "Use R to KS");
        public static readonly MenuSlider Rturret = new MenuSlider("rturret", "Don't use ult under Turret if HP% <  ",60,0,100);
        public static readonly MenuList rmode = new MenuList("rmode", "Combo Mode :", new[] { "Q+R", "R+Q(Misaya Combo)"});
        #endregion


        public static float GetComboDamage(AIBaseClient enemy)
        {
            float damage = 0;

            if (Q.IsReady())
            {
                damage += Q.GetDamage(enemy);
            }

            if (W.IsReady())
            {
                damage += W.GetDamage(enemy);
            }

            if (E.IsReady())
            {
                damage += E.GetDamage(enemy);
            }

            if (R.IsReady())
            {
                damage += R.GetDamage(enemy);
            }

            return damage;
        }


        public static void CreateMenu()
        {
            _config = new Menu("easymid.diana", "Easy_Mid.Diana", true);
            var _q = new Menu("_qmenu", "[Q] - Crescent Strike Settings");
            _q.Add(Qcombo);
            _q.Add(Qharass);
            _q.Add(Qfarm);
            _q.Add(Qclear);
            _q.Add(qhit);
            _q.Add(Qks);

            var _w = new Menu("_wmenu", "[W] - Pale Cascade Settings");
            _w.Add(Wcombo);
            _w.Add(Wharass);
            _w.Add(Wclear);

            var _e = new Menu("_emenu", "[E] - Moonfall Settings");
            _e.Add(Ecombo);
            _e.Add(Eharass);
            _e.Add(Eint);

            var _r = new Menu("_rmenu", "[R] - Lunar Rush Settings");
            _r.Add(Rcombo);
            _r.Add(Rkill);
            _r.Add(rmode);
            _r.Add(Rturret);
            _r.Add(Rks);

            var _pred = new Menu("_pred", "[SPREDICTION]");
            Prediction.Initialize(_pred);

            _config.Add(_q);
            _config.Add(_w);
            _config.Add(_e);
            _config.Add(_r);
            _config.Add(_pred);
            _config.Attach();
        }

        public static void OnLoad()
        {
            Q = new Spell(SpellSlot.Q, 895);
            W = new Spell(SpellSlot.W, 240);
            E = new Spell(SpellSlot.E, 450);
            R = new Spell(SpellSlot.R, 825);

            Q.SetSkillshot(0.25f, 150f, 1400f, false, SkillshotType.Circle);

            CreateMenu();
            Game.OnUpdate += OnUpdate;

            Interrupter.OnInterrupterSpell += (source, eventArgs) =>
            {
                var eSlot = E;
                if (Eint.Enabled && eSlot.IsReady()
                    && eSlot.Range >= source.DistanceToPlayer())
                {
                    eSlot.Cast();
                }
            };
        }

        private static void OnUpdate(EventArgs args)
        {
            KS();

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo(GetEnemy);
                    break;
                case OrbwalkerMode.Harass:
                    Harass(GetEnemy);
                    break;
                case OrbwalkerMode.LastHit:
                    Farm();
                    break;
                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    break;
            }
        }

        static AIHeroClient GetEnemy
        {
            get
            {
                var assassinRange = Q.Range;
                var vEnemy = ObjectManager.Get<AIHeroClient>()
                    .Where(enemy =>
                    enemy.Team != ObjectManager.Player.Team && !enemy.IsDead && enemy.IsVisible &&
                    ObjectManager.Player.Distance(enemy.Position) < assassinRange);

                vEnemy = (from vEn in vEnemy select vEn).OrderBy(vEn => vEn.MaxHealth);
                AIHeroClient[] objAiHeroes = vEnemy as AIHeroClient[] ?? vEnemy.ToArray();
                AIHeroClient t = !objAiHeroes.Any()
                ? TargetSelector.GetTarget(Q.Range) : objAiHeroes[0];

                return t;
            }
        }


        private static HitChance getQhit()
        {
            var hit = HitChance.Medium;
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


        private static void Combo(AIHeroClient target)
        {
            if (target == null)
            {
                return;
            }

            var qhit = getQhit();
            var useQ = Qcombo.Enabled;
            var useW = Wcombo.Enabled;
            var useE = Ecombo.Enabled;
            var useR = Rcombo.Enabled;
            var secondR = Rkill.Enabled;

            var hptodive = Rturret.Value;

            switch (rmode.Index)
            {
                case 0:
                    if(useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                    {
                        Q.SPredictionCast(target, qhit);
                    }
                    if(useR && R.IsReady() && target.IsValidTarget(R.Range) && target.HasBuff("dianamoonlight"))
                    {
                        if (target.IsUnderEnemyTurret() && _Player.HealthPercent < hptodive)
                            return;
                        R.Cast(target, true);
                    }
                    if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                        W.Cast();
                    if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                        E.Cast();
                    break;
                case 1:
                    if(useQ && useR && Q.IsReady() && R.IsReady())
                    {
                        R.Cast(target, true);
                        Q.Cast(target, true);
                    }
                    if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                        W.Cast();
                    if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                        E.Cast();
                    break;
            }
            if(Rkill && R.IsReady() && R.GetDamage(target) > target.Health)
            {
                R.Cast(target);
            }
        }

        private static void Harass(AIHeroClient target)
        {
            if (target == null)
                return;

            var qhit = getQhit();
            var useQ = Qharass.Enabled;
            var useW = Wharass.Enabled;
            var useE = Eharass.Enabled;

            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
            {
                Q.SPredictionCast(target, qhit);
            }
            if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                W.Cast();
            if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                E.Cast();
        }

        private static void LaneClear()
        {
            List<AIBaseClient> minions = MinionManager.GetMinions(_Player.Position, Q.Range);

            var qfarm = Q.GetCircularFarmLocation(minions);
            var wfarm = W.GetCircularFarmLocation(minions);
            if(qfarm.MinionsHit > 1 && Qclear.Enabled)
            {
                Q.Cast(qfarm.Position, true);
            }
            if (wfarm.MinionsHit > 1 && Wclear.Enabled)
            {
                W.Cast();
            }
        }
        private static void Farm()
        {
            var Qmin = GameObjects.EnemyMinions.Where(x => x.IsMinion && x.Health < Q.GetDamage(x) && x.DistanceToPlayer() > _Player.GetRealAutoAttackRange());
            var useQ = Qmin.FirstOrDefault();
            if(Qmin != null && Qfarm.Enabled && Q.IsReady())
            {
                Q.Cast(useQ.Position, true);
            }
        }
        private static void KS()
        {
            try
            {
                var t = TargetSelector.GetTarget(Q.Range);

                if(t != null)
                {
                    if (Qks.Enabled && Q.IsReady() && t.Health < Q.GetDamage(t))
                    {
                        Q.SPredictionCast(t, HitChance.Medium);
                    }
                    if(Rks.Enabled && R.IsReady() && t.Health < R.GetDamage(t))
                    {
                        R.Cast(t, true);
                    }
                }
            }
            catch
            {

            }
        }
    }
}
