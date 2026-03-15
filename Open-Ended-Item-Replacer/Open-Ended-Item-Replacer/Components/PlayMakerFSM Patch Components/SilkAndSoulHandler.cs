using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Open_Ended_Item_Replacer.FsmStateActions;
using static Open_Ended_Item_Replacer.Utils.FsmStateActionUtils;
using static Open_Ended_Item_Replacer.Utils.PersistenceUtils;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components
{
    internal class SilkAndSoulHandler
    {
        public static void TEST(PlayMakerFSM __instance)
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
        }

        // This is another instance of getting an item specifically while in a quest; I should probably also make this accessible at any point
        public static void HandleBellHermit(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Dialogue" && __instance.gameObject?.name == "Bell Hermit")
            {
                FsmState snareSoulDlg = __instance.Fsm.GetState("Snare Soul Dlg");
                if (snareSoulDlg == null) { return; }

                logSource.LogWarning("WORKING");

                snareSoulDlg.Actions[1] = new GetPersistentBoolUsingPersistentItemBool(GeneratePersistentBoolData(__instance.gameObject, "Soul Bell Hermit"), __instance.Fsm.GetFsmBool("Has Any"));
            }
        }
    }
}
