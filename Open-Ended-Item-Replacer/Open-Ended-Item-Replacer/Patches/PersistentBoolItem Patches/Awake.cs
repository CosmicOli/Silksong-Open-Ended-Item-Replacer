using HarmonyLib;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Patches.PersistentBoolItem_Patches
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
                Replace(__instance.gameObject, "Heart Piece", false, null);
            }

            if (__instance.ItemData.ID.ToLowerInvariant().StartsWith("silk spool"))
            {
                //logSource.LogInfo("Silk Spool");
                Replace(__instance.gameObject, "Silk Spool", false, null);
            }
        }
    }
}
