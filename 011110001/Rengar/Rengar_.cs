using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Prediction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rengar
{
    class Rengar_
    {
        private static Spell Q, W, E, R;
        private static Menu Rmenu;


        public static void OnLoad()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 450f);
            E = new Spell(SpellSlot.E, 1000f);

            E.SetSkillshot(0.25f, 70f, 1500f, true, SkillshotType.Line);

            var Rmenu = new Menu("rengar.port", "Rengar", true);

            var combo = new Menu("Combo", "Combo Config");
            combo.Add(RengarMenu.combat.Q);
            combo.Add(RengarMenu.combat.W);
            combo.Add(RengarMenu.combat.E);
            combo.Add(RengarMenu.combat.focus);
            combo.Add(new MenuList("target", "Target Focus", new[] { "Most AD", "Most AP", "Squishy" , "Lower HP" }, 2));

            var jg = new Menu("jg", "Jungle Clear/Lane Clear Config");
            jg.Add(RengarMenu.jg.Q);
            jg.Add(RengarMenu.jg.W);
            jg.Add(RengarMenu.jg.E);

            var misc = new Menu("misc", "Misc Config");
            misc.Add(RengarMenu.misc.autoE);
            misc.Add(RengarMenu.misc.Wheal);
            misc.Add(RengarMenu.misc.wlife);

            Rmenu.Add(combo);
            Rmenu.Add(jg);
            Rmenu.Add(misc);
            Rmenu.Attach();

            Game.OnUpdate += OnUpdate;
            Dash.OnDash += Rengar_Dash;
        }

        public static void OnPlayAnimation(AIBaseClient sender, AIBaseClientPlayAnimationEventArgs args)
        {
            if (sender.IsMe && args.Animation == "Spell5")
            {
                Console.WriteLine(args.Animation);
            }
        }

        private static void Orbwalker_OnAction(object sender, OrbwalkerActionArgs args)
        {
            if (args.Type == OrbwalkerType.AfterAttack)
            {
                var enemy = args.Target as AIHeroClient;
                if (enemy == null || !(args.Target is AIHeroClient))
                {
                    return;
                }

                if (Orbwalker.ActiveMode == OrbwalkerMode.Combo
                    || Orbwalker.ActiveMode == OrbwalkerMode.Harass)
                {
                    if (ObjectManager.Player.CountEnemyHeroesInRange(ObjectManager.Player.GetRealAutoAttackRange() + ObjectManager.Player.BoundingRadius + 100) != 0)
                    {
                        Q.Cast();
                    }
                }
            }
            else if (args.Type == OrbwalkerType.BeforeAttack)
            {
                if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && Ferocity == 4 && RengarMenu.combat.focus.Value != 1)
                {
                    return;
                }
                if (ObjectManager.Player.CountEnemyHeroesInRange(ObjectManager.Player.GetRealAutoAttackRange() + ObjectManager.Player.BoundingRadius + 100) != 0)
                {
                    args.Process = false;
                    Q.Cast();
                }
            }
        }

        private static void Heal()
        {
            if (RengarR || ObjectManager.Player.IsRecalling() || ObjectManager.Player.InFountain() || Ferocity != 4)
            {
                return;
            }

            if (ObjectManager.Player.CountEnemyHeroesInRange(W.Range) >= 1 && W.IsReady())
            {
                if (RengarMenu.misc.Wheal.Enabled && ObjectManager.Player.HealthPercent <= RengarMenu.misc.wlife.Value)
                {
                    W.Cast();
                }
            }
        }

        private static void Rengar_Dash(AIBaseClient sender, Dash.DashArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }
            var target = TargetSelector.GetTarget(1500f);
            if (!target.IsValidTarget())
            {
                return;
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo
                   || Orbwalker.ActiveMode == OrbwalkerMode.Harass)
            {
                if (Ferocity == 4)
                {
                    switch (RengarMenu.combat.focus.Value)
                    {
                        case 1:
                            if (Q.IsReady() && RengarMenu.combat.Q.Enabled &&
                                ObjectManager.Player.CountEnemyHeroesInRange(ObjectManager.Player.GetRealAutoAttackRange() + ObjectManager.Player.BoundingRadius + 100) != 0)
                            {
                                Q.Cast();
                            }
                            break;
                        case 3:
                            if (E.IsReady() && RengarMenu.combat.E.Enabled)
                            {
                                var targetE = TargetSelector.GetTarget(
                                    E.Range);
                                if (targetE.IsValidTarget())
                                {
                                    var pred = E.GetPrediction(targetE);
                                    if (pred.Hitchance >= HitChance.High)
                                    {
                                        EnsoulSharp.SDK.Utility.DelayAction.Add(300, () => E.Cast(target));
                                    }
                                }
                            }
                            break;

                    }
                }
                else
                {
                    if (Q.IsReady() && RengarMenu.combat.Q.Enabled)
                        Q.Cast();
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;

                case OrbwalkerMode.LaneClear:
                    JungleClear();
                    LaneClear();
                    break;
            }
            Heal();
            //R.Range = 1000 + R.Level * 1000;
            if (E.IsReady() && RengarMenu.misc.autoE.Enabled)
            {
                if (!RengarR)
                {
                    var target = TargetSelector.GetTarget(E.Range);
                    var pred = E.GetPrediction(target);
                    if (target != null && pred.Hitchance == HitChance.Immobile)
                    {
                        E.Cast(target);
                    }
                }
            }
        }

        public static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range);
            if (!target.IsValidTarget())
            {
                return;
            }
            if (Ferocity < 4)
            {
                if(Q.IsReady() && RengarMenu.combat.Q.Enabled && ObjectManager.Player.CountEnemyHeroesInRange(ObjectManager.Player.GetRealAutoAttackRange() + ObjectManager.Player.BoundingRadius + 100) != 0)
                {
                    Q.Cast();
                }
                if (!RengarR)
                {
                    if (!HasPassive)
                    {
                        if (E.IsReady() && RengarMenu.combat.E.Enabled)
                        {
                            CastE(target);
                        }
                    }
                    else
                    {
                        if (E.IsReady() && RengarMenu.combat.E.Enabled)
                        {
                            if (ObjectManager.Player.IsDashing())
                            {
                                CastE(target);
                            }
                        }
                    }
                    if (W.IsReady() && RengarMenu.combat.W.Enabled)
                    {
                        CastW();
                    }
                }
            }
            if (Ferocity == 4)
            {
                switch (RengarMenu.combat.focus.Value)
                {
                    case 1:

                        if(Q.IsReady() && RengarMenu.combat.Q.Enabled  && ObjectManager.Player.CountEnemyHeroesInRange(ObjectManager.Player.GetRealAutoAttackRange() + ObjectManager.Player.BoundingRadius + 100) != 0)
                        {
                            Q.Cast();
                        }
                        break;
                    case 2:
                        if (W.IsReady() && RengarMenu.combat.W.Enabled)
                        {
                            W.Cast();
                        }
                        break;
                    case 3:
                        if (!RengarR)
                        {
                            if (E.IsReady() && !HasPassive && RengarMenu.combat.E.Enabled)
                            {
                                CastE(target);
                            }
                        }
                        else
                        {
                            if (E.IsReady() && ObjectManager.Player.IsDashing() && RengarMenu.combat.E.Enabled)
                            {
                                CastE(target);
                            }
                        }
                        break;
                }
            }
        }

        private static void CastE(AIBaseClient target)
        {
            if (!E.IsReady() || !target.IsValidTarget(E.Range))
                return;

            var pred = E.GetPrediction(target);
            if (pred.Hitchance >= HitChance.High)
            {
                E.Cast(pred.CastPosition);
            }
        }

        private static void CastW()
        {
            if (!W.IsReady())
            {
                return;
            }

            if (GetWHits().Item1 > 0)
            {
                W.Cast();
            }
        }

        private static Tuple<int, List<AIHeroClient>> GetWHits()
        {
            try
            {
                var hits =
                    GameObjects.EnemyHeroes.Where(
                        e =>
                        e.IsValidTarget() && e.Distance(ObjectManager.Player) < 450f
                        || e.Distance(ObjectManager.Player) < 450f).ToList();

                return new Tuple<int, List<AIHeroClient>>(hits.Count, hits);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return new Tuple<int, List<AIHeroClient>>(0, null);
        }
        private static void JungleClear()
        {
            try
            {
                var mob = GameObjects.Jungle
                            .Where(x => x.IsValidTarget(E.Range) && x.GetJungleType() != JungleType.Unknown)
                            .OrderByDescending(x => x.MaxHealth).FirstOrDefault();
                if (!mob.IsValid())
                    return;

                if (mob.IsValidTarget(ObjectManager.Player.GetRealAutoAttackRange()) && Q.IsReady() && RengarMenu.jg.Q.Enabled)
                {
                    Q.Cast();
                }
                if (W.IsReady() && Ferocity == 4 && (ObjectManager.Player.Health / ObjectManager.Player.MaxHealth) * 100 <= 60 && mob.IsValidTarget(W.Range) && RengarMenu.jg.W.Enabled)
                {
                    W.Cast();
                }
                if (E.IsReady() && mob.IsValidTarget(E.Range) && RengarMenu.jg.E.Enabled)
                {
                    if (Ferocity < 4)
                    {
                        CastE(mob);
                    }
                }
                if (W.IsReady() && mob.IsValidTarget(W.Range) && RengarMenu.jg.W.Enabled)
                {
                    W.Cast();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void LaneClear()
        {
            var qminions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(ObjectManager.Player.GetRealAutoAttackRange()) && x.IsMinion())
                            .Cast<AIBaseClient>().ToList();
            var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(W.Range) && x.IsMinion())
    .Cast<AIBaseClient>().ToList();
            if (minions.Any())
            {
                if (minions.Count >= 3)
                {
                    W.Cast();
                }
            }
            if (qminions.Any())
            {
                Q.Cast();
            }
        }

        public static int Ferocity => (int)ObjectManager.Player.Mana;

        public static bool HasPassive => ObjectManager.Player.Buffs.Any(x => x.Name.ToLower().Contains("rengarpassivebuff"));

        public static bool RengarR => ObjectManager.Player.Buffs.Any(x => x.Name.ToLower().Contains("rengarrbuff"));

    }
}

