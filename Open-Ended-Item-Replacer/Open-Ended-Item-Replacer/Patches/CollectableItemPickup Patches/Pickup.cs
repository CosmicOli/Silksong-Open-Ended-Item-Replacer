using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TeamCherry.NestedFadeGroup;
using UnityEngine;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using static Open_Ended_Item_Replacer.Utils.Replace_Utils.SpawnUtils;

namespace Open_Ended_Item_Replacer.Patches.CollectableItemPickup_Patches
{
    [HarmonyPatch(typeof(CollectableItemPickup), "Pickup")]
    internal class Pickup
    {
        private static void Postfix(CollectableItemPickup __instance)
        {
            
        }
    }
}
