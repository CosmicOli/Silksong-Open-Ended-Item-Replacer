using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Open_Ended_Item_Replacer.FsmStateActions;
using UnityEngine;
using static Open_Ended_Item_Replacer.Utils.FsmStateActionUtils;
using static Open_Ended_Item_Replacer.Utils.GetBoolFuncs;
using static Open_Ended_Item_Replacer.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components
{
    internal class EvaHandler
    {
        public static void Handle_Eva(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Dialogue" && __instance.gameObject?.name == "Crest Upgrade Shrine")
            {
                FsmState checkCombo1 = __instance.Fsm.GetState("Check Combo 1");
                FsmState checkSlot1 = __instance.Fsm.GetState("Check Slot1");
                FsmState checkSlot2 = __instance.Fsm.GetState("Check Slot2");
                FsmState checkHunterv3 = __instance.Fsm.GetState("Check Hunter v3");
                FsmState checkFinalUpgrade = __instance.Fsm.GetState("Check Final Upgrade");
                FsmState showedPrompt = __instance.Fsm.GetState("Showed Prompt?");

                FsmState unlockCrestUpg1 = __instance.Fsm.GetState("Unlock Crest Upg 1");
                FsmState unlockFirstSlot = __instance.Fsm.GetState("Unlock First Slot");
                FsmState unlockOtherSlot = __instance.Fsm.GetState("Unlock Other Slot");
                FsmState unlockCrestUpg2 = __instance.Fsm.GetState("Unlock Crest Upg 2");
                FsmState setBound = __instance.Fsm.GetState("Set Bound");

                FsmState crestChangeAntic = __instance.Fsm.GetState("Crest Change Antic");
                FsmState crestChange = __instance.Fsm.GetState("Crest Change");
                FsmState crestChangeEnd = __instance.Fsm.GetState("Crest Change End");
                FsmState firstUpgDlg = __instance.Fsm.GetState("First Upg Dlg");
                FsmState upgradeSequence5 = __instance.Fsm.GetState("Upgrade Sequence 5");

                FsmState wasUpgraded = __instance.Fsm.GetState("Was Upgraded?");
                FsmState offerDlg = __instance.Fsm.GetState("Offer Dlg");

                FsmState init = __instance.Fsm.GetState("Init");
                FsmState endDialogue = __instance.Fsm.GetState("End Dialogue");
                FsmState breakState = __instance.Fsm.GetState("Break");
                FsmState broken = __instance.Fsm.GetState("Broken");

                if (checkCombo1 == null || checkSlot1 == null || checkSlot2 == null || checkHunterv3 == null || checkFinalUpgrade == null || showedPrompt == null) { return; }

                string hunter_v2 = "Hunter_v2";
                string hunter_v3 = "Hunter_v3";
                string yellowSlot = "Yellow Slot";
                string blueSlot = "Blue Slot";
                string sylphsong = "Sylphsong";

                PersistentItemData<bool> persistentBoolDataHunter_v2 = GeneratePersistentBoolData_SameScene(hunter_v2, hunter_v2);
                PersistentItemData<bool> persistentBoolDataHunter_v3 = GeneratePersistentBoolData_SameScene(hunter_v3, hunter_v3);
                PersistentItemData<bool> persistentBoolDataYellowSlot = GeneratePersistentBoolData_SameScene(yellowSlot, yellowSlot);
                PersistentItemData<bool> persistentBoolDataBlueSlot = GeneratePersistentBoolData_SameScene(blueSlot, blueSlot);
                PersistentItemData<bool> persistentBoolDataSylphsong = GeneratePersistentBoolData_SameScene(sylphsong, sylphsong);

                // The following handles replacing the majority of persistence checks

                // my one thought is that checkCombo1 is being run and not properly reset, causing it to hang on second entry from denying and reentering the dialogue
                checkCombo1.Actions[0] = new SetFsmActiveState(__instance.Fsm, checkCombo1, checkSlot1, GetPersistentBoolFromDataFunc(persistentBoolDataHunter_v2), GetTrueFunc()); // hunter 2
                checkCombo1.Actions[1].Enabled = false;

                checkSlot1.Actions[0] = new SetFsmActiveState(__instance.Fsm, checkSlot1, checkSlot2, GetPersistentBoolFromDataFunc(persistentBoolDataYellowSlot), GetTrueFunc()); // yellow slot
                checkSlot1.Actions[1].Enabled = false;
                checkSlot1.Actions[2].Enabled = false;

                checkSlot2.Actions[0] = new SetFsmActiveState(__instance.Fsm, checkSlot2, checkHunterv3, GetPersistentBoolFromDataFunc(persistentBoolDataBlueSlot), GetTrueFunc()); // blue slot
                checkSlot2.Actions[1].Enabled = false;
                checkSlot2.Actions[2].Enabled = false;

                checkHunterv3.Actions[0] = new SetFsmActiveState(__instance.Fsm, checkHunterv3, checkFinalUpgrade, GetPersistentBoolFromDataFunc(persistentBoolDataHunter_v3), GetTrueFunc()); // hunter 3

                checkFinalUpgrade.Actions[0].Enabled = false;
                checkFinalUpgrade.Actions[1] = new SetFsmActiveState(__instance.Fsm, checkFinalUpgrade, showedPrompt, GetPersistentBoolFromDataFunc(persistentBoolDataSylphsong), GetTrueFunc()); // bound eva

                // The following handle replacing getting the upgrades

                bool CheckCurrentCrestPoints()
                {
                    if (__instance.Fsm.GetFsmInt("Current Crest Points").Value >= 12)
                    {
                        __instance.Fsm.GetFsmBool("Stay In Sequence").Value = true;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                GameObject hunter_v2GameObject = new GameObject(hunter_v2);
                unlockCrestUpg1.Actions[2].Enabled = false;
                unlockCrestUpg1.Actions[3].Enabled = false;
                firstUpgDlg.Actions = ReturnCombinedActions(new FsmStateAction[] { new GetCheck(hunter_v2GameObject, hunter_v2), new SetFsmActiveState(__instance.Fsm, firstUpgDlg, checkSlot1, CheckCurrentCrestPoints, GetTrueFunc()) }, firstUpgDlg.Actions);

                GameObject yellowSlot_GameObject = new GameObject(yellowSlot);
                unlockFirstSlot.Actions[5] = new GetCheck(yellowSlot_GameObject, yellowSlot);

                GameObject blueSlot_GameObject = new GameObject(blueSlot);
                unlockOtherSlot.Actions[2] = new SetIntValue();
                (unlockOtherSlot.Actions[2] as SetIntValue).intVariable = __instance.Fsm.GetFsmInt("Slot Index");
                (unlockOtherSlot.Actions[2] as SetIntValue).intValue = 1;
                unlockOtherSlot.Actions[4] = new GetCheck(blueSlot_GameObject, blueSlot);

                GameObject hunter_v3GameObject = new GameObject(hunter_v3);
                unlockCrestUpg2.Actions[2] = new GetCheck(hunter_v3GameObject, hunter_v3);

                GameObject sylphsong_GameObject = new GameObject(sylphsong);
                setBound.Actions[2] = new GetCheck(sylphsong_GameObject, sylphsong);

                // The following handles removing the incorrect animations for hunter crest upgrades

                unlockCrestUpg1.Actions = ReturnCombinedActions(unlockCrestUpg1.Actions, new FsmStateAction[] { new SetFsmActiveState(__instance.Fsm, crestChangeAntic) });
                unlockCrestUpg2.Actions = ReturnCombinedActions(unlockCrestUpg2.Actions, new FsmStateAction[] { new SetFsmActiveState(__instance.Fsm, crestChangeAntic) });

                crestChangeAntic.Actions[1].Enabled = false; // Play animation
                crestChangeAntic.Actions[7].Enabled = false; // Wait
                crestChangeAntic.Actions[9].Enabled = false; // Wait
                crestChangeAntic.Actions[10].Enabled = false; // Send event
                crestChangeAntic.Actions[11].Enabled = false; // Wait

                crestChange.Actions[1].Enabled = false; // Auto equip crest
                crestChange.Actions[2].Enabled = false; // Send event
                crestChange.Actions[3].Enabled = false; // Wait

                crestChangeEnd.Actions[1].Enabled = false; // Play animation
                crestChangeEnd.Actions[3] = new SetFsmActiveState(__instance.Fsm, firstUpgDlg, GetPersistentBoolFromDataFunc(persistentBoolDataHunter_v2), GetFalseFunc());
                //crestChangeEnd.Actions = ReturnCombinedActions(crestChangeEnd.Actions, new FsmStateAction[] { new SetFsmActiveState(__instance.Fsm, checkFinalUpgrade, GetPersistentBoolFromDataFunc(persistentBoolDataHunter_v3), GetTrueFunc()) });

                // The following handles removing the incorrect animations for slot upgrades

                unlockFirstSlot.Actions[6].Enabled = false;
                unlockFirstSlot.Actions[7].Enabled = false;
                unlockFirstSlot.Actions[8].Enabled = false;
                unlockFirstSlot.Actions = ReturnCombinedActions(unlockFirstSlot.Actions, new FsmStateAction[] { new SetFsmActiveState(__instance.Fsm, checkSlot2) }); // Added to the end instead of replacing to make it clear 6-8 are disabled

                unlockOtherSlot.Actions[5].Enabled = false;
                unlockOtherSlot.Actions[6].Enabled = false;
                unlockOtherSlot.Actions[7].Enabled = false;
                unlockOtherSlot.Actions = ReturnCombinedActions(unlockOtherSlot.Actions, new FsmStateAction[] { new SetFsmActiveState(__instance.Fsm, checkHunterv3) }); // Added to the end instead of replacing to make it clear 5-7 are disabled

                // The following handles removing the incorrect animations and persistence for Sylphsong

                init.Actions[0].Enabled = false;
                init.Actions = ReturnCombinedActions(init.Actions, new FsmStateAction[] { new SetFsmActiveState(__instance.Fsm, broken, GetPersistentBoolFromDataFunc(persistentBoolDataSylphsong), GetTrueFunc()) });
                setBound.Actions[0].Enabled = false;
                endDialogue.Actions[2] = endDialogue.Actions[3];
                endDialogue.Actions[3] = new SetFsmActiveState(__instance.Fsm, broken, GetPersistentBoolFromDataFunc(persistentBoolDataSylphsong), GetTrueFunc());

                // The following handles incorrect dialogue and other misc fixes

                (offerDlg.Actions[2] as RunDialogue).Key = "CREST_UPGRADE_ALL"; // Makes sure the correct dialogue plays
                (wasUpgraded.Actions[2] as GetIsCrestUnlocked).falseEvent = FsmEvent.GetFsmEvent("CONVO_END"); // Ensures skipping dialogue always

                // As a note here, by adding dummy code to a test action for testing, I don't think that its some pc specific async issue based on processing time or some bs like that
                showedPrompt.Actions[7].Enabled = false; // For some reason, the Wait here completely stops working and the state moves on before anything past it happens, so I disable it
                showedPrompt.Actions[8].Enabled = false; // The animation also causes issues, which I suspect relates to the wait?
            }
        }
    }
}
