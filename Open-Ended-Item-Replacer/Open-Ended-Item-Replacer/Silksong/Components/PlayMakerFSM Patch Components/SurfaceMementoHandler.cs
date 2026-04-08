using HutongGames.PlayMaker;
using Open_Ended_Item_Replacer.Silksong.FsmStateActions;
using static Open_Ended_Item_Replacer.Silksong.Utils.GetBoolFuncs;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components
{
    internal class SurfaceMementoHandler
    {
        public static void Handle_SurfaceMemento(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Memory Sequence" && __instance.gameObject?.name == "Memory Group")
            {
                FsmState dropItemAnimator = __instance.Fsm.GetState("Drop Item Animator");
                FsmState activated = __instance.Fsm.GetState("Activated");
                if (dropItemAnimator == null || __instance.gameObject.scene.name != "Abandoned_town") { return; }

                dropItemAnimator.Actions[2].Enabled = false;
                dropItemAnimator.Actions[3] = new SetFsmActiveState(__instance.Fsm, activated, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene("Collectable Item Pickup", "Memento Surface")), GetFalseFunc());
            }
        }
    }
}
