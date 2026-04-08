using HarmonyLib;
using HutongGames.PlayMaker;
using System;
using UnityEngine;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Silksong.Patches.FSMUtility_Patches
{
    [HarmonyPatch(typeof(FSMUtility), "SendEventUpwards", new Type[] { typeof(GameObject), typeof(FsmEvent) })]
    internal class SendEventUpwards
    {
        private static bool Prefix()
        {
            if (BlockNextFsmEventTransmition)
            {
                BlockNextFsmEventTransmition = false;
                return false;
            }

            return true;
        }
    }
}
