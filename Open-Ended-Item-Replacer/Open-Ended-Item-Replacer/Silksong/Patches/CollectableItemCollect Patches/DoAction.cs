using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using static Open_Ended_Item_Replacer.Silksong.Utils.Replace_Utils.ReplaceUtils;

namespace Open_Ended_Item_Replacer.Silksong.Patches.CollectableItemCollect_Patches
{
    [HarmonyPatch(typeof(CollectableItemCollect), "DoAction")]
    internal class DoAction
    {
        // Handles when FSMs run CollectableItemCollect
        // Should handle the vast majority of cases of being given an item from an NPC
        private static bool Prefix(CollectableItemCollect __instance, CollectableItem item)
        {
            ReplaceFsmItemGet(__instance, item);

            return false;
        }
    }
}
