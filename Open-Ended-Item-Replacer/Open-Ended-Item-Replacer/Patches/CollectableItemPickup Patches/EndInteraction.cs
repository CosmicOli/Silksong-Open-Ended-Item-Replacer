using HarmonyLib;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using static Open_Ended_Item_Replacer.Utils.Replace_Utils.SpawnUtils;

namespace Open_Ended_Item_Replacer.Patches.CollectableItemPickup_Patches
{
    [HarmonyPatch(typeof(CollectableItemPickup), "EndInteraction")]
    internal class EndInteraction
    {
        private static void Prefix(CollectableItemPickup __instance, ref bool didPickup)
        {
            if (choosing)
            {
                didPickup = bought;
                logSource.LogMessage("ASDasdsasdas");
            }
        }
    }
}
