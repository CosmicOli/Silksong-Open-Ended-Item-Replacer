using HutongGames.PlayMaker;
using Open_Ended_Item_Replacer.Silksong.FsmStateActions;

namespace Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components
{
    internal class KeyOfHereticHandler
    {
        public static void Handle_KeyOfHeretic(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Control" && __instance.gameObject?.name == "Item Placer" && __instance.gameObject.scene.name == "Slab_16")
            {
                FsmState flingAndEndBattle = __instance.Fsm.GetState("Fling and End Battle");
                if (flingAndEndBattle == null) { return; }

                GetCheck replacementGetCheck = new GetCheck("Collectable Item Pickup", "Slab Key B");
                flingAndEndBattle.Actions[0] = replacementGetCheck;
            }
        }
    }
}
