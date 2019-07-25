using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Prediction;
using EnsoulSharp.SDK.Utility;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Zed_is_Back_Bitches.ZedMenu;
using Color = System.Drawing.Color;

namespace Zed_is_Back_Bitches
{
    class Program
    {

        private const string ChampionName = "Zed";
        private static List<Spell> SpellList = new List<Spell>();
        private static Spell _q, _w, _e, _r;
        public static Menu _config;
        private static AIHeroClient _player;
        private static SpellSlot _igniteSlot;
        private static Items.Item _tiamat, _hydra, _blade, _bilge, _rand, _lotis, _youmuu;
        private static Vector3 linepos;
        private static Vector3 castpos;
        private static int clockon;
        private static int countults;
        private static int countdanger;
        private static int ticktock;
        private static Vector3 rpos;
        private static int shadowdelay = 0;
        private static int delayw = 500;


        static void Main(string[] args)
        {

            GameEvent.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad()
        {
            try
            {
                Chat.Print("This script is a Port of Zed is Back (Code of Jackisback)");

                _player = ObjectManager.Player;
                if (ObjectManager.Player.CharacterName != ChampionName) return;
                _q = new Spell(SpellSlot.Q, 900f);
                _w = new Spell(SpellSlot.W, 700f);
                _e = new Spell(SpellSlot.E, 270f);
                _r = new Spell(SpellSlot.R, 650f);

                _q.SetSkillshot(0.25f, 50f, 1700f, false, SkillshotType.Line);

                _bilge = new Items.Item(3144, 475f);
                _blade = new Items.Item(3153, 425f);
                _hydra = new Items.Item(3074, 250f);
                _tiamat = new Items.Item(3077, 250f);
                _rand = new Items.Item(3143, 490f);
                _lotis = new Items.Item(3190, 590f);
                _youmuu = new Items.Item(3142, 10);
                _igniteSlot = _player.GetSpellSlot("SummonerDot");
                var enemy = from hero in ObjectManager.Get<AIHeroClient>()
                            where hero.IsEnemy == true
                            select hero;



                var _config = new Menu("zedisback", "011110001.Zed", true);
                var combo = new Menu("combat", "[COMBO] Settings");
                combo.Add(_combo.Ult);
                combo.Add(_combo.Wgap);
                combo.Add(_combo.W2);
                combo.Add(_combo.Ig);
                combo.Add(_combo.Cmode);

                var harass = new Menu("harass", "[HARASS] Settings");
                harass.Add(_harass.Wh);
                harass.Add(_harass.energia);

                var clear = new Menu("clear", "[LANE CLEAR] Settings");
                clear.Add(_clear.Qlane);
                clear.Add(_clear.Elane);
                clear.Add(_clear.energia);

                var misc = new Menu("clear", "[MISC] Settings");
                misc.Add(_misc.Qks);
                misc.Add(_misc.Eks);

                var dodge = new Menu("dodge", "[ULTIMATE] Settings");

                foreach (var e in enemy)
                {
                    SpellDataInstClient rdata = e.Spellbook.GetSpell(SpellSlot.R);
                    if (DangerDB.DangerousList.Any(spell => spell.Contains(rdata.SData.Name)))
                        dodge.Add(new MenuBool(rdata.SData.Name, rdata.SData.Name));
                }

                _config.Add(combo);
                _config.Add(harass);
                _config.Add(clear);
                _config.Add(misc);
                _config.Add(dodge);
                _config.Attach();


                Game.OnUpdate += Game_OnUpdate;
                Drawing.OnDraw += Drawing_OnDraw;
                //AIBaseClient.OnProcessSpellCast += OnProcessSpell;
            }
            catch
            {

            }
        }


        private static void Game_OnUpdate(EventArgs args)
        {
            try
            {
                KillSteal();
            }
            catch
            {
                //fail
            }

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    try
                    {
                        switch (_combo.Cmode.Index)
                        {
                            case 0:
                                Combo(GetEnemy);
                                break;
                            case 1:
                                TheLine(GetEnemy);
                                break;
                        }

                    }
                    catch
                    {
                        // error
                    }
                    break;
                case OrbwalkerMode.Harass:
                    try { Harass(GetEnemy); }
                    catch
                    {
                        //erro
                    }
                    break;
                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    break;
                case OrbwalkerMode.LastHit:
                    LastHit();
                    break;
            }
        }

        private static void OnProcessSpell(AIBaseClient unit, AIBaseClientProcessSpellCastEventArgs castedSpell)
        {
            if (!unit.IsEnemy)
                return;

            if (unit.IsEnemy && unit.DistanceToPlayer() < _r.Range)
            {
                if (_r.IsReady() && UltStage == UltCastStage.First &&
                _config["dodge"].GetValue<MenuBool>(castedSpell.SData.Name).Enabled)
                {
                    if (DangerDB.DangerousList.Any(spell => spell.Contains(castedSpell.SData.Name)) &&
                        (unit.Distance(_player.Position) < 650f || _player.Distance(castedSpell.End) <= 250f))
                    {
                        if (castedSpell.SData.Name == "SyndraR")
                        {
                            clockon = Environment.TickCount + 150;
                            countdanger = countdanger + 1;
                        }
                        else
                        {
                            var target = TargetSelector.GetTarget(640);
                            _r.Cast(target);
                        }
                    }
                }
            }
            if (unit.IsMe && castedSpell.SData.Name == "zedult")
            {
                ticktock = Environment.TickCount + 200;

            }
        }


        private static void Combo(AIHeroClient t)
        {
            var target = t;
            if (target == null)
                return;
            if (!target.IsValidTarget(_w.Range + _q.Range))
            {
                return;
            }
            var overkill = _q.GetDamage(target) + _e.GetDamage(target) + _player.GetAutoAttackDamage(target) * 2;

            if (_combo.Ult.Enabled && UltStage == UltCastStage.First && overkill > target.Health)
            {
                if (target.DistanceToPlayer() > 700 && target.MoveSpeed > _player.MoveSpeed || target.Distance(_player.Position) > 800 && _combo.Wgap.Enabled)
                {
                    CastW(target);
                    if(_combo.W2.Enabled)
                        _w.Cast();
                }
                _r.Cast(target);
            }
            else
            {
                if (target != null && ShadowStage == ShadowCastStage.First && _combo.Wgap.Enabled &&
                        target.Distance(_player.Position) > 400 && target.Distance(_player.Position) < 1300)
                {
                    CastW(target);
                }
                if (target != null && ShadowStage == ShadowCastStage.Second && _combo.Wgap.Enabled &&
                    target.Distance(WShadow.Position) < target.Distance(_player.Position))
                {
                    if (_combo.W2.Enabled)
                        _w.Cast();
                }
                CastE();
                CastQ(target);
            }
        }

        private static void TheLine(AIHeroClient t)
        {
            var target = t;

            if (target == null)
            {
                return;
            }

            if (!_r.IsReady() || target.Distance(_player.Position) >= 640)
            {
                return;
            }
            if (UltStage == UltCastStage.First && _combo.Ult.Enabled)
                _r.Cast(target);
            linepos = target.Position.Extend(_player.Position, -500);

            if (target != null && ShadowStage == ShadowCastStage.First && UltStage == UltCastStage.Second)
            {
                if (_combo.W2.Enabled)
                    _w.Cast(linepos);
                CastE();
                CastQ(target);
                if (target != null && _combo.Ig.Enabled && _igniteSlot != SpellSlot.Unknown &&
                            _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                {
                    _player.Spellbook.CastSpell(_igniteSlot, target);
                }
            }

            if (target != null && WShadow != null && UltStage == UltCastStage.Second && target.Distance(_player.Position) > 250 && (target.Distance(WShadow.Position) < target.Distance(_player.Position)))
            {
                if (_combo.W2.Enabled)
                    _w.Cast();
            }

        }

        private static void Harass(AIHeroClient t)
        {
            var target = t;

            if (target.IsValidTarget() && _harass.Wh.Enabled && _w.IsReady() && _q.IsReady() && ObjectManager.Player.Mana >
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost +
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ManaCost && target.Distance(_player.Position) > 850 &&
                target.Distance(_player.Position) < 1400)
            {
                CastW(target);
            }

            if (target.IsValidTarget() && (ShadowStage == ShadowCastStage.Second || ShadowStage == ShadowCastStage.Cooldown || !_harass.Wh.Enabled
                            && _q.IsReady() &&
                                (target.Distance(_player.Position) <= 900 || target.Distance(WShadow.Position) <= 900)))
            {
                CastQ(target);
            }

            if (target.IsValidTarget() && _w.IsReady() && _q.IsReady() && _harass.Wh.Enabled &&
                ObjectManager.Player.Mana >
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost +
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ManaCost)
            {
                if (target.Distance(_player.Position) < 750)

                    CastW(target);
            }

            CastE();
        }

        static AIHeroClient GetEnemy
        {
            get
            {
                var assassinRange = 1000f;
                var vEnemy = ObjectManager.Get<AIHeroClient>()
                    .Where(enemy =>
                    enemy.Team != ObjectManager.Player.Team && !enemy.IsDead && enemy.IsVisible &&
                    ObjectManager.Player.Distance(enemy.Position) < assassinRange);

                vEnemy = (from vEn in vEnemy select vEn).OrderByDescending(vEn => vEn.MaxHealth);
                AIHeroClient[] objAiHeroes = vEnemy as AIHeroClient[] ?? vEnemy.ToArray();
                AIHeroClient t = !objAiHeroes.Any()
                ? TargetSelector.GetTarget(1400) : objAiHeroes[0];

                return t;
            }
        }

        private static AIMinionClient WShadow
        {
            get
            {
                return
                    ObjectManager.Get<AIMinionClient>()
                        .FirstOrDefault(minion => minion.IsVisible && minion.IsAlly && (minion.Position != rpos) && minion.Name == "Shadow");
            }
        }
        private static AIMinionClient RShadow
        {
            get
            {
                return
                    ObjectManager.Get<AIMinionClient>()
                        .FirstOrDefault(minion => minion.IsVisible && minion.IsAlly && (minion.Position == rpos) && minion.Name == "Shadow");
            }
        }
        private static UltCastStage UltStage
        {
            get
            {
                if (!_r.IsReady()) return UltCastStage.Cooldown;

                return (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Name == "ZedR"
                //return (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Name == "zedult"
                    ? UltCastStage.First
                    : UltCastStage.Second);
            }
        }

        private static ShadowCastStage ShadowStage
        {
            get
            {
                if (!_w.IsReady()) return ShadowCastStage.Cooldown;

                return (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "ZedW"
                //return (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "ZedShadowDash"
                    ? ShadowCastStage.First
                    : ShadowCastStage.Second);

            }
        }

        private static void CastW(AIHeroClient target)
        {
            if (delayw >= Environment.TickCount - shadowdelay || ShadowStage != ShadowCastStage.First ||
                (target.HasBuff("zedulttargetmark") && UltStage == UltCastStage.Cooldown))
                return;

            var herew = target.Position.Extend(ObjectManager.Player.Position, -200);

            _w.Cast(herew, true);
            shadowdelay = Environment.TickCount;
        }

        private static void CastQ(AIBaseClient target)
        {
            if (!_q.IsReady()) return;

            if (WShadow != null && target.Distance(WShadow.Position) <= 900 && target.Distance(_player.Position) > 450)
            {

                var shadowpred = _q.GetPrediction(target);
                _q.UpdateSourcePosition(WShadow.Position, WShadow.Position);
                if (shadowpred.Hitchance >= HitChance.Medium)
                    _q.Cast(target);


            }
            else
            {

                _q.UpdateSourcePosition(_player.Position, _player.Position);
                var normalpred = _q.GetPrediction(target);

                if (normalpred.CastPosition.Distance(_player.Position) < 900 && normalpred.Hitchance >= HitChance.Medium)
                {
                    _q.Cast(target);
                }


            }
        }

        private static void CastE()
        {
            if (!_e.IsReady()) return;
            if (ObjectManager.Get<AIHeroClient>()
                .Count(
                    hero =>
                        hero.IsValidTarget() &&
                        (hero.Distance(ObjectManager.Player.Position) <= _e.Range ||
                         (WShadow != null && hero.Distance(WShadow.Position) <= _e.Range))) > 0)
                _e.Cast();
        }

        internal enum UltCastStage
        {
            First,
            Second,
            Cooldown
        }

        internal enum ShadowCastStage
        {
            First,
            Second,
            Cooldown
        }

        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(2000);
            if (target.IsValidTarget() && _q.IsReady() && _misc.Qks.Enabled && _q.GetDamage(target) > target.Health)
            {
                if (_player.Distance(target.Position) <= _q.Range)
                {
                    _q.Cast(target);
                }
                else if (WShadow != null && WShadow.Distance(target.Position) <= _q.Range)
                {
                    _q.UpdateSourcePosition(WShadow.Position, WShadow.Position);
                    _q.Cast(target);
                }
                else if (RShadow != null && RShadow.Distance(target.Position) <= _q.Range)
                {
                    _q.UpdateSourcePosition(RShadow.Position, RShadow.Position);
                    _q.Cast(target);
                }
            }
            if (target.IsValidTarget() && _q.IsReady() && _misc.Qks.Enabled && _q.GetDamage(target) > target.Health)
            {
                if (_player.Distance(target.Position) <= _q.Range)
                {
                    _q.Cast(target);
                }
                else if (WShadow != null && WShadow.Distance(target.Position) <= _q.Range)
                {
                    _q.UpdateSourcePosition(WShadow.Position, WShadow.Position);
                    _q.Cast(target);
                }
            }
            if (_e.IsReady() && _misc.Eks.Enabled)
            {
                var t = TargetSelector.GetTarget(_e.Range);
                if (_e.GetDamage(t) > t.Health && (_player.Distance(t.Position) <= _e.Range || WShadow.Distance(t.Position) <= _e.Range))
                {
                    _e.Cast();
                }
            }
        }

        private static void LaneClear()
        {
            var minionsq = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(_q.Range) && x.IsMinion())
    .Cast<AIBaseClient>().ToList();
            var minionse = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(_e.Range) && x.IsMinion())
    .Cast<AIBaseClient>().ToList();
            if (_q.IsReady() && _clear.Qlane.Enabled && _player.ManaPercent >= _clear.energia.Value)
            {
                var fl2 = _q.GetLineFarmLocation(minionsq, _q.Width);
                if (fl2.MinionsHit >= 1)
                {
                    _q.Cast(fl2.Position);
                }
            }
            if (_e.IsReady() && _clear.Elane.Enabled && _player.ManaPercent >= _clear.energia.Value)
            {
                if (minionse.Count > 2)
                {
                    _e.Cast();
                }
            }
        }
        private static void LastHit()
        {
            var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(_q.Range) && x.IsMinion()).Cast<AIBaseClient>().ToList();
            if (minions.Any())
            {
                foreach (var minion in minions)
                {
                    if(minion.DistanceToPlayer() < _player.GetRealAutoAttackRange())
                    {
                        return;
                    }
                    if (minion.DistanceToPlayer() > _player.GetRealAutoAttackRange())
                    {
                        if (_q.GetDamage(minion) >= minion.Health)
                        {
                            _q.Cast(minion);
                        }
                    }
                }
            }
        }

        private static float ComboDamage(AIBaseClient enemy)
        {
            var damage = 0d;
            if (_q.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.Q);
            if (_w.IsReady() && _q.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.Q) / 2;
            if (_e.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.E);
            if (_r.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.R);

            return (float)damage;
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (
                    var enemyVisible in
                        ObjectManager.Get<AIHeroClient>().Where(enemyVisible => enemyVisible.IsValidTarget()))
            {
                if (ComboDamage(enemyVisible) > enemyVisible.Health)
                {
                    Drawing.DrawText(Drawing.WorldToScreen(enemyVisible.Position)[0] + 50,
                           Drawing.WorldToScreen(enemyVisible.Position)[1] - 40, Color.Red,
                           "Combo=Rekt");
                }
            }
            try
            {
                var modo = "null";
                switch (_combo.Cmode.Index)
                {
                    case 0:
                        modo = "Combo Mode : Common";
                        break;
                    case 1:
                        modo = "Combo Mode : The line";
                        break;
                }
                Drawing.DrawText(Drawing.WorldToScreen(_player.Position)[0] - 60,
                    Drawing.WorldToScreen(_player.Position)[1] + 20, Color.Red, modo);
            }
            catch
            {
                //null
            }
        }
    }
}