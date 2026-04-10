using HarmonyLib;
using System.Collections;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using static Open_Ended_Item_Replacer.Silksong.Utils.LoadSaveFileUtils;

namespace Open_Ended_Item_Replacer.Silksong.Patches.GameManager_Patches
{
    internal class RunContinueGame
    {
        [HarmonyPatch(typeof(GameManager), "RunContinueGame")]
        public static async void Postfix()
        {
            await DoLoadSaveFileExtras();
            logSource.LogWarning("FINISHED");
        }
    }
}
