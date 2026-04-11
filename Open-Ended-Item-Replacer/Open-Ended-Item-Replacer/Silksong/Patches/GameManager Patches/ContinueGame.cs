using HarmonyLib;
using System.Collections;
using System.Threading.Tasks;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using static Open_Ended_Item_Replacer.Silksong.Utils.LoadSaveFileUtils;

namespace Open_Ended_Item_Replacer.Silksong.Patches.GameManager_Patches
{
    [HarmonyPatch(typeof(GameManager), "ContinueGame")]
    internal class ContinueGame
    {
        public static bool RunContinueGamePatched = true;

        public static bool Prefix()
        {
            if (RunContinueGamePatched)
            {
                // Starts the async function without awaiting as you cannot await a prefix to halt the continuation of the function from the looks of it
                DoLoadSaveFileExtras();

                return false;
            }
            else
            {
                RunContinueGamePatched = true;
                return true;
            }
        }
    }
}
