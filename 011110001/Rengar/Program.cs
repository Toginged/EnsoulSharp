using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rengar
{
    class Program
    {

        private static void Main(string[] args)
        {
            GameEvent.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad()
        {
            if (ObjectManager.Player.CharacterName != "Rengar")
            {
                return;
            }
            Chat.Print("Simple Rengar Script Load");
            Chat.Print("This script is a Port of ElRengar (Code of jQuery)");
            Rengar_.OnLoad();

        }
    }
}
