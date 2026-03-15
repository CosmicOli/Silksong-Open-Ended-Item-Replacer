using HarmonyLib;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Patches.GameManager_Patches
{
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.TimePassesElsewhere))]
    internal class TimePassesElsewhere
    {
        public static void Postfix(GameManager __instance)
        {
            logSource.LogInfo("Time Passes Elsewhere");
        }
    }
}
