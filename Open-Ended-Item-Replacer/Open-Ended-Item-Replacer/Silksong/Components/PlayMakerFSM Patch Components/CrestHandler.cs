using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Open_Ended_Item_Replacer.Silksong.FsmStateActions;
using System.Collections.Generic;
using UnityEngine;
using static Open_Ended_Item_Replacer.Silksong.Utils.FsmStateActionUtils;
using static Open_Ended_Item_Replacer.Silksong.Utils.GetBoolFuncs;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components
{
    internal class CrestHandler
    {
        public static void Handle_Crest(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Control" && __instance.gameObject?.name == "Crest Get Shrine")
            {
                FsmState checkUnlocked = __instance.Fsm.GetState("Check Unlocked");
                FsmState inactive = __instance.Fsm.GetState("Inactive");
                FsmState crestMsg = __instance.Fsm.GetState("Crest Msg");
                FsmState setReturn = __instance.Fsm.GetState("Set Return");
                FsmState crestGetAntic = __instance.Fsm.GetState("Crest Get Antic");
                FsmState crestReturnAnim = __instance.Fsm.GetState("Crest Return Anim");
                if (checkUnlocked == null || inactive == null || crestMsg == null || setReturn == null || crestGetAntic == null || crestReturnAnim == null) { return; }

                string itemName = __instance.Fsm.Variables.GetFsmEnum("Crest Type").Value.ToString();
                PersistentItemData<bool> persistentData = GeneratePersistentBoolData(__instance.gameObject, itemName);
                checkUnlocked.Actions[0] = new SetFsmActiveState(__instance.Fsm, checkUnlocked, inactive, GetPersistentBoolFromDataFunc(persistentData), GetTrueFunc());

                crestMsg.Actions[2].Enabled = false;

                // Not entirely sure what this does, but usually it would change it and then change it back to this later, but the reverting is skipped
                (setReturn.Actions[0] as ToolsActiveStateControlV2).SetActiveState = ToolsActiveStates.Active;
                (setReturn.Actions[0] as ToolsActiveStateControlV2).SkipAnims = false;

                crestGetAntic.Actions[2].Enabled = false;
                crestGetAntic.Actions[3].Enabled = false;
                crestGetAntic.Actions = ReturnCombinedActions(crestGetAntic.Actions, new FsmStateAction[] { new SetFsmActiveState(__instance.Fsm, crestReturnAnim) });

                crestReturnAnim.Actions = ReturnCombinedActions(new FsmStateAction[] { new GetCheck(__instance.gameObject, itemName) }, crestReturnAnim.Actions);

                //Replace(__instance.gameObject, __instance.Fsm.Variables.GetFsmEnum("Crest Type").Value.ToString(), true, null);
            }
        }

        public static Dictionary<string, string> associatedChapelSceneName = new Dictionary<string, string>(); // Values added on mod load
        public static void Handle_CrestDoor(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "chapel_door_control" && __instance.gameObject?.name == "Chapel Door Control")
            {
                FsmState stateCheck = __instance.Fsm.GetState("State Check");
                FsmState open = __instance.Fsm.GetState("Open");
                if (stateCheck == null || open == null) { return; }

                string itemName = __instance.Fsm.Variables.GetFsmEnum("Crest Type").Value.ToString();

                try
                {
                    GameObject dummyGameObject = new GameObject("Crest Get Shrine");
                    PersistentItemData<bool> persistentData = GeneratePersistentBoolData(dummyGameObject, itemName);
                    persistentData.SceneName = associatedChapelSceneName[itemName];
                    stateCheck.Actions[0] = new SetFsmActiveState(__instance.Fsm, stateCheck, open, GetPersistentBoolFromDataFunc(persistentData), GetFalseFunc());
                }
                catch (KeyNotFoundException)
                {
                    logSource.LogError("Chapel door unable to find crest");
                }
            }

            if (__instance.Fsm.Name == "FSM" && __instance.gameObject?.name == "Architect Shrine Door")
            {
                FsmState gotCrest = __instance.Fsm.GetState("Got Crest?");
                FsmState gotCrest2 = __instance.Fsm.GetState("Got Crest? 2");
                FsmState lockState = __instance.Fsm.GetState("Lock");
                FsmState locked = __instance.Fsm.GetState("Locked");
                FsmState unlocked = __instance.Fsm.GetState("Unlocked");
                FsmState stay = __instance.Fsm.GetState("Stay");
                if (gotCrest == null || gotCrest2 == null || lockState == null || locked == null || unlocked == null || stay == null) { return; }

                string itemName = "Toolmaster";

                try
                {
                    GameObject dummyGameObject = new GameObject("Crest Get Shrine");
                    PersistentItemData<bool> persistentData = GeneratePersistentBoolData(dummyGameObject, itemName);
                    persistentData.SceneName = associatedChapelSceneName[itemName];

                    FsmStateAction[] newActionsGotCrest = new FsmStateAction[2];
                    newActionsGotCrest[0] = new SetFsmActiveState(__instance.Fsm, gotCrest, stay, GetPersistentBoolFromDataFunc(persistentData), GetFalseFunc());
                    newActionsGotCrest[1] = new SetFsmActiveState(__instance.Fsm, gotCrest, lockState, GetPersistentBoolFromDataFunc(persistentData), GetTrueFunc());

                    FsmStateAction[] newActionsGotCrest2 = new FsmStateAction[2];
                    newActionsGotCrest2[1] = new SetFsmActiveState(__instance.Fsm, gotCrest2, unlocked, GetPersistentBoolFromDataFunc(persistentData), GetFalseFunc());
                    newActionsGotCrest2[0] = new SetFsmActiveState(__instance.Fsm, gotCrest2, locked, GetPersistentBoolFromDataFunc(persistentData), GetTrueFunc());

                    gotCrest.Actions = newActionsGotCrest;
                    gotCrest2.Actions = newActionsGotCrest2;
                }
                catch (KeyNotFoundException)
                {
                    logSource.LogError("Chapel door unable to find crest");
                }
            }
        }
    }
}
