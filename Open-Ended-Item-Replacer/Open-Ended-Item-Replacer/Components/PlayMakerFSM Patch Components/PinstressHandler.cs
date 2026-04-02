using HutongGames.PlayMaker;
using Open_Ended_Item_Replacer.FsmStateActions;
using UnityEngine;
using static Open_Ended_Item_Replacer.Utils.GetBoolFuncs;
using static Open_Ended_Item_Replacer.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components
{
    internal class PinstressHandler
    {
        public static void Handle_Pinstress(PlayMakerFSM __instance)
        {
            string needleStrike = "Needle Strike";

            if (__instance.Fsm.Name == "States" && __instance.gameObject?.name == "Pinstress States")
            {
                FsmState check = __instance.Fsm.GetState("Check");
                FsmState ground = __instance.Fsm.GetState("Ground");
                if (check == null || ground == null) { return; }

                check.Actions[0] = new SetFsmActiveState(__instance.Fsm, check, ground, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene(needleStrike, needleStrike)), GetFalseFunc());
            }

            if (__instance.Fsm.Name == "Behaviour" && __instance.gameObject?.name == "Pinstress Interior Ground Sit")
            {
                FsmState save = __instance.Fsm.GetState("Save");
                FsmState met = __instance.Fsm.GetState("Met?");
                FsmState reofferDlg = __instance.Fsm.GetState("Reoffer Dlg");
                if (save == null || met == null) { return; }

                met.Actions[4] = new SetFsmActiveState(__instance.Fsm, met, reofferDlg, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene(needleStrike, needleStrike)), GetFalseFunc());

                GameObject dummyGameObject = new GameObject(needleStrike);
                save.Actions[2] = new GetCheck(dummyGameObject, needleStrike);
            }
        }
    }
}
