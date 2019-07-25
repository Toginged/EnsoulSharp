using EnsoulSharp.SDK.MenuUI.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rengar
{
    class RengarMenu
    {
        public class combat
        {
            public static readonly MenuBool Q = new MenuBool("q", "Use Q");
            public static readonly MenuBool W = new MenuBool("w", "Use W");
            public static readonly MenuBool E = new MenuBool("e", "Use E");
            public static readonly MenuSlider focus = new MenuSlider("focus", "Prioritize : 1-Q 2-W 3-E", 1, 1, 3);
        }
        public class misc
        {
            public static readonly MenuBool Wheal = new MenuBool("wheal", "Auto W for Heal");
            public static readonly MenuSlider wlife = new MenuSlider("wlife", "^ X% Life", 60, 0, 100);
            public static readonly MenuBool autoE = new MenuBool("autoE", "Auto E on Immobile target");
        }
        public class jg
        {
            public static readonly MenuBool Q = new MenuBool("qj", "Use Q");
            public static readonly MenuBool W = new MenuBool("wj", "Use W");
            public static readonly MenuBool E = new MenuBool("ej", "Use E");
        }
    }
}
