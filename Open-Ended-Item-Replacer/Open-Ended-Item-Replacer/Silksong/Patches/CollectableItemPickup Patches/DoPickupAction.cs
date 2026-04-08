using HarmonyLib;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using static Open_Ended_Item_Replacer.Utils.Replace_Utils.SpawnUtils;

namespace Open_Ended_Item_Replacer.Silksong.Patches.CollectableItemPickup_Patches
{
    [HarmonyPatch(typeof(CollectableItemPickup), "DoPickupAction")]
    internal class DoPickupAction
    {
        private static void Postfix(CollectableItemPickup __instance, ref bool __result)
        {
            if (choosing)
            {
                __result = true;
            }
        }
    }
}
