using HarmonyLib;
using static Open_Ended_Item_Replacer.Silksong.Utils.LoadSaveFileUtils;

namespace Open_Ended_Item_Replacer.Silksong.Patches.GameManager_Patches
{
    [HarmonyPatch(typeof(GameManager), "StartNewGame")]
    public class StartNewGame
    {
        public static bool Prefix()
        {
            return HandleLoadSave(true);
        }
    }
}
