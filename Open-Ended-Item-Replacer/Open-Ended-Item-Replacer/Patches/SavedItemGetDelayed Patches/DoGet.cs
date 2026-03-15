using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Patches.SavedItemGetDelayed_Patches
{
    [HarmonyPatch(typeof(SavedItemGetDelayed), "DoGet")]
    internal class DoGet
    {
        // Handles when FSMs run SavedItemGetDelayed
        // Should handle the vast majority of cases of being given an item from an NPC
        private static bool Prefix(SavedItemGet __instance)
        {
            ReplaceFsmItemGet(__instance, __instance.Item.Value as SavedItem);

            __instance.Finish();
            return false;
        }
    }
}
