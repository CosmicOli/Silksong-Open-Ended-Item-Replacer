using HarmonyLib;
using System;

namespace Open_Ended_Item_Replacer.Silksong.Patches.PlayMakerFSM_Patches
{
    public class AwakePatchEventArgs : EventArgs
    {
        public PlayMakerFSM __instance;

        public AwakePatchEventArgs(PlayMakerFSM __instance) 
        { 
            this.__instance = __instance;
        }
    }

    [HarmonyPatch(typeof(PlayMakerFSM), "Awake")]
    internal class Awake
    {
        public delegate void AwakePatchEventHandler(PlayMakerFSM e);

        public static event AwakePatchEventHandler AwakePatchEvent;

        private static void Postfix(PlayMakerFSM __instance)
        {
            AwakePatchEvent?.Invoke(__instance);
        }
    }
}
