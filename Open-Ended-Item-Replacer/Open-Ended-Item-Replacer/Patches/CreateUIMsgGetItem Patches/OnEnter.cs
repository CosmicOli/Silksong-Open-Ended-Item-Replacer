using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Patches.CreateUIMsgGetItem_Patches
{
    [HarmonyPatch(typeof(CreateUIMsgGetItem), "OnEnter")]
    internal class OnEnter
    {
        private static void Prefix(CreateUIMsgGetItem __instance)
        {
            PlayMakerFSM playMakerFsm = __instance.storeObject.Value.transform.GetComponent<PlayMakerFSM>();

            HandleUiMsgGetItem(playMakerFsm);
        }
    }
}
