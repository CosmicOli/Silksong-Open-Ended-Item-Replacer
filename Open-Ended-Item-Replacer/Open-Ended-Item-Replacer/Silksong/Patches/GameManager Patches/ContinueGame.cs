using HarmonyLib;
using static Open_Ended_Item_Replacer.Silksong.Utils.LoadSaveFileUtils;

namespace Open_Ended_Item_Replacer.Silksong.Patches.GameManager_Patches
{
    [HarmonyPatch(typeof(GameManager), "ContinueGame")]
    public class ContinueGame
    {
        public static bool Prefix()
        {
            return HandleLoadSave(false);
        }

        //public static void Postfix()
        //{
          //  HandleLoadSave(false);
        //}
    }
}
