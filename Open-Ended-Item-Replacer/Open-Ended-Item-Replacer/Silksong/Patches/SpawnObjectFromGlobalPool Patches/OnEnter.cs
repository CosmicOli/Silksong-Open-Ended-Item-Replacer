using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using static Open_Ended_Item_Replacer.Silksong.Utils.Replace_Utils.UiReplaceUtils;

namespace Open_Ended_Item_Replacer.Silksong.Patches.SpawnObjectFromGlobalPool_Patches
{
    [HarmonyPatch(typeof(SpawnObjectFromGlobalPool), "OnEnter")]
    internal class OnEnter
    {
        private static void Prefix(SpawnObjectFromGlobalPool __instance)
        {
            PlayMakerFSM playMakerFsm = __instance.gameObject?.Value?.transform?.GetComponent<PlayMakerFSM>();
            if (playMakerFsm == null) { return; }

            HandleUiMsgGetItemMelody(playMakerFsm, __instance);
        }
    }
}
