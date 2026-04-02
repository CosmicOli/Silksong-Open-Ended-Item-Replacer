using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Open_Ended_Item_Replacer.FsmStateActions;
using static Open_Ended_Item_Replacer.Utils.FsmStateActionUtils;
using static Open_Ended_Item_Replacer.Utils.PersistenceUtils;
using static Open_Ended_Item_Replacer.Utils.GetBoolFuncs;

namespace Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components
{
    internal class SilkAndSoulHandler
    {
        // Interesting fact from testing: Snare Setter is taken in some other way than SetToolLocked, which honestly is a good thing because it means I don't have to fix it not being taken
        /*public static void TEST(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Dialogue" && __instance.gameObject?.name == "Enclave Caretaker")
            {
                FsmState willOfferSnare = __instance.Fsm.GetState("Will Offer Snare?");
                FsmState dlgChoice = __instance.Fsm.GetState("Dlg Choice");
                FsmState snare = __instance.Fsm.GetState("Snare?");


                (willOfferSnare.Actions[0] as SetBoolValue).boolValue = true;
                willOfferSnare.Actions = ReturnCombinedActions(new FsmStateAction[] { new TestAction() }, willOfferSnare.Actions);

                dlgChoice.Actions = ReturnCombinedActions(new FsmStateAction[] { new TestAction() }, dlgChoice.Actions);
                snare.Actions = ReturnCombinedActions(new FsmStateAction[] { new TestAction() }, snare.Actions);

                FsmState repeat = __instance.Fsm.GetState("Repeat");
                FsmState swampSoul = __instance.Fsm.GetState("Swamp Soul");
                FsmState offer = __instance.Fsm.GetState("Offered?");
                FsmState wishProgress = __instance.Fsm.GetState("Wish Progress");
                FsmState canComplete = __instance.Fsm.GetState("Can Complete?");

                repeat.Actions = ReturnCombinedActions(new FsmStateAction[] { new TestAction() }, repeat.Actions); // This is ran, I need to make offer ran 
                swampSoul.Actions = ReturnCombinedActions(new FsmStateAction[] { new TestAction() }, swampSoul.Actions);
                offer.Actions = ReturnCombinedActions(new FsmStateAction[] { new TestAction() }, offer.Actions);
                wishProgress.Actions = ReturnCombinedActions(new FsmStateAction[] { new TestAction() }, wishProgress.Actions);
                canComplete.Actions = ReturnCombinedActions(new FsmStateAction[] { new TestAction() }, canComplete.Actions);

                repeat.Actions = ReturnCombinedActions(new FsmStateAction[] { new SetFsmActiveState(__instance.Fsm, repeat, offer) }, repeat.Actions);
            }
        }*/

        // This is an instance of getting an item specifically while in a quest; hence, it is made accessible at any point
        public static void Handle_BellHermit(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Dialogue" && __instance.gameObject?.name == "Bell Hermit")
            {
                FsmState doSnareConvo = __instance.Fsm.GetState("Do Snare Convo?");
                FsmState snareSoulDlg = __instance.Fsm.GetState("Snare Soul Dlg");
                FsmState branchCheck = __instance.Fsm.GetState("Branch Check");
                if (doSnareConvo == null || snareSoulDlg == null || branchCheck == null) { return; }

                doSnareConvo.Actions[3] = new SetFsmActiveState(__instance.Fsm, doSnareConvo, snareSoulDlg, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData(__instance.gameObject, "Snare Soul Bell Hermit")), GetFalseFunc());
                doSnareConvo.Actions = ReturnCombinedActions(doSnareConvo.Actions, new FsmStateAction[] { new SetFsmActiveState(__instance.Fsm, doSnareConvo, branchCheck, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData(__instance.gameObject, "Snare Soul Bell Hermit")), GetTrueFunc()) });

                snareSoulDlg.Actions[1].Enabled = false;
                snareSoulDlg.Actions[2].Enabled = false;
                snareSoulDlg.Actions[3].Enabled = false;
                snareSoulDlg.Actions[4].Enabled = false;
                (snareSoulDlg.Actions[5] as ConvertBoolToString).trueString = (snareSoulDlg.Actions[5] as ConvertBoolToString).falseString;
            }
        }

        // This is an instance of getting an item specifically while in a quest; hence, it is made accessible at any point
        public static void Handle_Churchkeeper(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Conversation" && __instance.gameObject?.name == "Churchkeeper")
            {
                FsmState snare = __instance.Fsm.GetState("Snare?");
                FsmState snareSoulDlg = __instance.Fsm.GetState("Snare Soul Dlg");
                FsmState talk = __instance.Fsm.GetState("Talk?");
                if (snare == null || snareSoulDlg == null || talk == null) { return; }

                snare.Actions[0] = new SetFsmActiveState(__instance.Fsm, snare, snareSoulDlg, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData(__instance.gameObject, "Snare Soul Churchkeeper")), GetFalseFunc());
                snare.Actions = ReturnCombinedActions(snare.Actions, new FsmStateAction[] { new SetFsmActiveState(__instance.Fsm, snare, talk, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData(__instance.gameObject, "Snare Soul Churchkeeper")), GetTrueFunc()) });

                snareSoulDlg.Actions[0].Enabled = false;
                snareSoulDlg.Actions[1].Enabled = false;
                snareSoulDlg.Actions[2].Enabled = false;
                (snareSoulDlg.Actions[3] as ConvertBoolToString).trueString = (snareSoulDlg.Actions[3] as ConvertBoolToString).falseString;
            }
        }
    }
}
