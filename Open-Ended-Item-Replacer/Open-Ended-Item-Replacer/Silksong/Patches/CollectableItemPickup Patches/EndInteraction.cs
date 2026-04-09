using HarmonyLib;
using static Open_Ended_Item_Replacer.Silksong.Utils.Replace_Utils.SpawnUtils;

namespace Open_Ended_Item_Replacer.Silksong.Patches.CollectableItemPickup_Patches
{
    [HarmonyPatch(typeof(CollectableItemPickup), "EndInteraction")]
    internal class EndInteraction
    {
        private static void Prefix(CollectableItemPickup __instance, ref bool didPickup)
        {
            if (choosing)
            {
                choosing = false;
                didPickup = purchased;
            }
        }
    }
}
