using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Zed_is_Back_Bitches
{
    class ZedMenu
    {
        public class _combo
        {
            public static readonly MenuBool Wgap = new MenuBool("wgab", "Use W on Combo (also gap close)");
            public static readonly MenuBool W2 = new MenuBool("w2", "Use W2 on Combo");
            public static readonly MenuBool Ig = new MenuBool("UseIgnitecombo", "Use Ignite(On The Line Combo)");
            public static readonly MenuBool Ult = new MenuBool("UseUlt", "Use Ultimate");
            public static readonly MenuList Cmode = new MenuList("cmode", "Combo Mode :", new[] { "Common", "The Line(Need Use R Enable)"});
        }
        public class _harass
        {
            public static readonly MenuBool Wh = new MenuBool("wh", "Use W on Harass");
            public static readonly MenuSlider energia = new MenuSlider("harassenergia", "Energy Harass % >= ", 60, 0, 100);
        }
        public class _clear
        {
            public static readonly MenuBool Qlane = new MenuBool("qlane", "Use Q to Lane Clear");
            public static readonly MenuBool Elane = new MenuBool("elane", "Use E to Lane Clear");
            public static readonly MenuSlider energia = new MenuSlider("energia", "Energy Lane% >= ", 60, 0, 100);
        }
        public class _misc
        {
            public static readonly MenuBool Qks = new MenuBool("qks", "Use Q to KillSteal");
            public static readonly MenuBool Eks = new MenuBool("eks", "Use E to KillSteal");
        }
    }
}
