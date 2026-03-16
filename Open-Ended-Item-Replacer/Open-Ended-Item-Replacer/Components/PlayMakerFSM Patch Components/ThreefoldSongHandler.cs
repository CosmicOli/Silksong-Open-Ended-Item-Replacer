using HutongGames.PlayMaker;
using Open_Ended_Item_Replacer.FsmStateActions;
using static HutongGames.PlayMaker.FsmEventTarget;
using static Open_Ended_Item_Replacer.Utils.FsmStateActionUtils;
using static Open_Ended_Item_Replacer.Utils.GetBoolFuncs;
using static Open_Ended_Item_Replacer.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components
{
    internal class ThreefoldSongHandler
    {
        public static void HandleArchitectMelody(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Cylinder States" && __instance.gameObject?.name == "puzzle cylinders")
            {
                FsmState waitForNotify = __instance.Fsm.GetState("Wait For Notify");
                FsmState startLock = __instance.Fsm.GetState("Start Lock");
                FsmState hasMelody = __instance.Fsm.GetState("Has Melody");
                if (startLock == null || waitForNotify == null || hasMelody == null) { return; }

                waitForNotify.Actions[0] = new SetFsmActiveState(__instance.Fsm, hasMelody, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene("puzzle cylinders", "Citadel Ascent Melody Architect")), GetTrueFunc()); // Replaces original persistence check with custom

                int numberOfNewActions = 1;
                FsmStateAction[] newActions = new FsmStateAction[numberOfNewActions];

                newActions[0] = new SetFsmActiveState(__instance.Fsm, startLock, waitForNotify, GetPlayerDataBoolFunc("hasNeedolin"), GetFalseFunc()); // Disables allowing getting the song part without needolin

                //Array.Copy(startLock.Actions, 0, newActions, numberOfNewActions, startLock.Actions.Length);
                startLock.Actions = ReturnCombinedActions(newActions, startLock.Actions);

                //startLock.Actions = newActions;
            }
        }

        public static void HandleConductorMelody(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Dialogue" && __instance.gameObject?.name == "Last Conductor NPC")
            {
                FsmState hasItem = __instance.Fsm.GetState("Has Item?");
                FsmState repeatDlg = __instance.Fsm.GetState("Repeat Dlg");
                FsmState questActive = __instance.Fsm.GetState("Quest Active?");
                FsmState melodyNoQuest = __instance.Fsm.GetState("Melody NoQuest");

                int numberOfNewActions = 1;
                FsmStateAction[] newActions = new FsmStateAction[numberOfNewActions];

                newActions[0] = new SetFsmActiveState(__instance.Fsm, questActive, melodyNoQuest, GetPlayerDataBoolFunc("hasNeedolin"), GetFalseFunc()); // Disables allowing getting the song part without needolin
                questActive.Actions = ReturnCombinedActions(newActions, questActive.Actions);

                hasItem.Actions[8] = new SetFsmActiveState(__instance.Fsm, hasItem, repeatDlg, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene("Last Conductor NPC", "Citadel Ascent Melody Conductor")), GetTrueFunc());
            }
        }

        public static void HandleLibrarianMelody(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Dialogue" && __instance.gameObject?.name == "Librarian")
            {
                FsmState openRelicBoard = __instance.Fsm.GetState("Open Relic Board");
                if (openRelicBoard == null) { return; }

                FsmState needolinPreWait = __instance.Fsm.GetState("Needolin Pre Wait");
                FsmState dlgEnd = __instance.Fsm.GetState("Dlg End");
                FsmStateAction[] newActions = new FsmStateAction[2];

                newActions[0] = new SetFsmActiveState(__instance.Fsm, needolinPreWait, dlgEnd, GetPlayerDataBoolFunc("hasNeedolin"), GetFalseFunc());
                newActions[1] = new SetFsmActiveState(__instance.Fsm, needolinPreWait, dlgEnd, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene("Librarian", "Citadel Ascent Melody Librarian Return")), GetTrueFunc());

                needolinPreWait.Actions = ReturnCombinedActions(newActions, needolinPreWait.Actions);
            }

            if (__instance.Fsm.Name == "Behaviour" && __instance.gameObject?.name == "Gramaphone")
            {
                FsmState start = __instance.Fsm.GetState("Start");
                if (start == null || __instance.gameObject.scene.name != "Library_08") { return; }

                bool GetIfLibrarianMelodyCylinderDeposited()
                {
                    if (PlayerData.instance.Relics.GetData("Librarian Melody Cylinder").IsDeposited == true)
                    {
                        if (!GetPersistentBoolFromData(GeneratePersistentBoolData_SameScene("Librarian", "Citadel Ascent Melody Librarian Return")))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                FsmEventTarget eventTarget = new FsmEventTarget();
                eventTarget.target = EventTarget.BroadcastAll;
                SendEventOnComparison sendEventOnComparison = new SendEventOnComparison(eventTarget, FsmEvent.GetFsmEvent("MELODY CYLINDER PLAYED"), 0, false, GetIfLibrarianMelodyCylinderDeposited, GetTrueFunc());

                start.Actions = ReturnCombinedActions(new FsmStateAction[] { sendEventOnComparison }, start.Actions);
            }
        }

        public static void HandleThreefoldSongLift(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Sequence" && __instance.gameObject?.name == "Boss Scene")
            {
                FsmState lockInspectIdle = __instance.Fsm.GetState("Lock Inspect Idle");
                FsmState npcTalksEnd = __instance.Fsm.GetState("NPC Talks End");
                if (lockInspectIdle == null || npcTalksEnd == null) { return; }

                npcTalksEnd.Actions = ReturnCombinedActions(npcTalksEnd.Actions, new FsmStateAction[] { npcTalksEnd.Actions[6] });
                npcTalksEnd.Actions[6] = npcTalksEnd.Actions[5];

                npcTalksEnd.Actions[5] = new SetFsmActiveState(__instance.Fsm, npcTalksEnd, lockInspectIdle, GetPlayerDataBoolFunc("hasNeedolin"), GetFalseFunc());
            }
        }
    }
}
