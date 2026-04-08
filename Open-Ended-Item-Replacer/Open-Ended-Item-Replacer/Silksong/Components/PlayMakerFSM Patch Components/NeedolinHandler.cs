using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Open_Ended_Item_Replacer.Silksong.FsmStateActions;
using UnityEngine;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using static Open_Ended_Item_Replacer.Silksong.Utils.FsmStateActionUtils;
using static Open_Ended_Item_Replacer.Silksong.Utils.GetBoolFuncs;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;


namespace Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components
{
    internal class NeedolinHandler
    {
        public static void Handle_NeedolinPreMemory(PlayMakerFSM __instance)
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

        public static void Handle_NeedolinInMemory(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Memory Control" && __instance.gameObject?.name == "Memory Control")
            {
                if (__instance.Fsm.GetState("Get Rune Bomb") != null) { return; } // Don't want to trigger on first sinner

                FsmState needolinPrompt = __instance.Fsm.GetState("Needolin Prompt");
                FsmState endScene = __instance.Fsm.GetState("End Scene");
                if (needolinPrompt == null || endScene == null) { return; }

                // Ensures this is the needolin fsm
                if ((endScene.Actions[0] as SetPlayerDataVariable)?.VariableName.Value != "hasNeedolin")
                {
                    return;
                }

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

        // Defined for refencing elsewhere
        public static string BeastlingCall = "Beastling Call";
        public static string BeastlingCallGameObjectName = "Bell Beast DefeatedCentipede NPC";
        public static void Handle_BeastlingCall(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Control" && __instance.gameObject?.name == BeastlingCallGameObjectName)
            {
                FsmState timePasses = __instance.Fsm.GetState("Time Passes");
                if (timePasses == null) { return; }

                timePasses.Actions[1] = new GetCheck(__instance.gameObject, BeastlingCall);
            }

            if (__instance.Fsm.Name == "Hero Fling Out" && __instance.gameObject?.name == "Bone Beast NPC")
            {
                FsmState endQuest = __instance.Fsm.GetState("End Quest?");
                FsmState idle = __instance.Fsm.GetState("Idle");
                if (endQuest == null) { return; }

                PersistentItemData<bool> persistent = GeneratePersistentBoolData_SameScene(BeastlingCallGameObjectName, BeastlingCall);
                persistent.SceneName = "Bellway_Centipede_Arena";

                endQuest.Actions[0] = new SetFsmActiveState(__instance.Fsm, endQuest, idle, GetPersistentBoolFromDataFunc(persistent), GetFalseFunc());
            }
        }

        public static void Handle_ElegyOfTheDeep(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Dialogue" && __instance.gameObject?.name == "Snail Shamans Set")
            {
                FsmState talk = __instance.Fsm.GetState("Talk?");
                FsmState updateQuest = __instance.Fsm.GetState("Update Quest");
                if (talk == null || updateQuest == null) { return; }

                string name = "SuperJump";
                PersistentItemData<bool> persistent = GeneratePersistentBoolData_SameScene(name, name);
                persistent.SceneName = "Abyss_08";

                talk.Actions[1] = new GetPersistentBoolUsingPersistentItemBool(persistent, __instance.Fsm.GetFsmBool("Has Superjump"));
                updateQuest.Actions[0] = new GetCheck(__instance.gameObject, "Elegy of the Deep");
            }
        }
    }
}
