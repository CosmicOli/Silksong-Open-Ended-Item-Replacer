using HarmonyLib;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Silksong.Patches.CustomSceneManager_Patches
{
    [HarmonyPatch(typeof(CustomSceneManager), "Start")]
    public class Start
    {
        public static bool Prefix()
        {
            return LoadGameRunPatched; // Continue if not currently loading in scene assets for prefabs
        }
    }
}
