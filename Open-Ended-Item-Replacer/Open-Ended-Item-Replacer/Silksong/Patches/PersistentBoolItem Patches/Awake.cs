using HarmonyLib;
using static Open_Ended_Item_Replacer.Core.Utils.Replace_Utils.ReplaceUtils;

namespace Open_Ended_Item_Replacer.Silksong.Patches.PersistentBoolItem_Patches
{
    [HarmonyPatch(typeof(PersistentBoolItem), "Awake")]
    internal class Awake
    {
        // Replaces physical Mask Shards and Spool Fragments
        // All physically placed mask shards (heart piece) and spool fragments (silk spool) have persistent bools attributed to them
        private static void Postfix(PersistentBoolItem __instance)
        {
            if (__instance.ItemData.ID.ToLowerInvariant().StartsWith("heart piece"))
            {
                //logSource.LogInfo("Heart Piece");
                Replace(__instance.gameObject, "Heart Piece"); // INTERACTABLE false
            }

            if (__instance.ItemData.ID.ToLowerInvariant().StartsWith("silk spool"))
            {
                //logSource.LogInfo("Silk Spool");
                Replace(__instance.gameObject, "Silk Spool"); // INTERACTABLE false
            }
        }
    }
}
