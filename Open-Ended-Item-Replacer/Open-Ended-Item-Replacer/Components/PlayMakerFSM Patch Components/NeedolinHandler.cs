using HutongGames.PlayMaker;
using Open_Ended_Item_Replacer.FsmStateActions;
using UnityEngine;
using static Open_Ended_Item_Replacer.Utils.FsmStateActionUtils;


namespace Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components
{
    internal class NeedolinHandler
    {
        public static void HandleNeedolinPreMemory(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Control" && __instance.gameObject?.name == "Spinner Boss")
            {
                FsmState finalBindBurst = __instance.Fsm.GetState("Final Bind Burst");
                FsmState getNeedolin = __instance.Fsm.GetState("Get Needolin");
                if (finalBindBurst == null || getNeedolin == null) { return; }

                finalBindBurst.Actions[3].Enabled = false; // disables giving needolin
                getNeedolin.Actions[1].Enabled = false; // disables needolin message
            }
        }

        public static void HandleNeedolinInMemory(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Memory Control" && __instance.gameObject?.name == "Memory Control")
            {
                if (__instance.Fsm.GetState("Get Rune Bomb") != null) { return; } // Don't want to trigger on first sinner

                FsmState needolinPrompt = __instance.Fsm.GetState("Needolin Prompt");
                FsmState endScene = __instance.Fsm.GetState("End Scene");
                if (needolinPrompt == null || endScene == null) { return; }

                string needolin = "Needolin";

                needolinPrompt.Actions[1].Enabled = false; // disables giving needolin
                endScene.Actions[0].Enabled = false; // disables giving needolin

                int numberOfNewActions = 1;

                FsmStateAction[] newActions = new FsmStateAction[numberOfNewActions];

                GameObject dummyGameObject = new GameObject(needolin);
                newActions[0] = new GetCheck(dummyGameObject, needolin); // Replace

                //Array.Copy(endScene.Actions, 0, newActions, numberOfNewActions, endScene.Actions.Length);
                endScene.Actions = ReturnCombinedActions(newActions, endScene.Actions);

                //endScene.Actions = newActions;
            }
        }
    }
}
