using HutongGames.PlayMaker;
using Open_Ended_Item_Replacer.Silksong.FsmStateActions;
using UnityEngine;
using static Open_Ended_Item_Replacer.Silksong.Utils.GetBoolFuncs;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components
{
    internal class SilkNeedleHandler
    {
        public static void Handle_SilkNeedle(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Control" && __instance.gameObject?.name == "Silk Needle Spell Get")
            {
                FsmState checkUnlocked = __instance.Fsm.GetState("Check Unlocked");
                FsmState inactive = __instance.Fsm.GetState("Inactive");
                FsmState msg = __instance.Fsm.GetState("Msg");
                FsmState end = __instance.Fsm.GetState("End");
                if (checkUnlocked == null || inactive == null || msg == null || end == null) { return; }

                string paleNails = "Pale Nails";

                checkUnlocked.Actions[0] = new SetFsmActiveState(__instance.Fsm, checkUnlocked, inactive, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene("Pale Nails", "Pale Nails")), GetTrueFunc());

                msg.Actions[0].Enabled = false;
                msg.Actions[2].Enabled = false;

                end.Actions[0].Enabled = false;
                end.Actions[1].Enabled = false;
                end.Actions[2].Enabled = false;

                GameObject dummyGameObject = new GameObject(paleNails);
                end.Actions[3] = new GetCheck(dummyGameObject, paleNails);
            }
        }
    }
}
