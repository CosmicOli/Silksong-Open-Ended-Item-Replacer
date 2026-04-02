using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using static Open_Ended_Item_Replacer.Utils.LoadSaveFileUtils;

namespace Open_Ended_Item_Replacer.Patches.GameManager_Patches
{
    internal class RunStartNewGame
    {
        [HarmonyPatch(typeof(GameManager), "RunStartNewGame")]
        public static void Postfix()
        {
            DoLoadSaveFileExtras();
        }
    }
}
