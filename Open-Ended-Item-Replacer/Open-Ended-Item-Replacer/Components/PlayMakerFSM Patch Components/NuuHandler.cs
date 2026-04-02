using HutongGames.PlayMaker;
using Open_Ended_Item_Replacer.FsmStateActions;
using UnityEngine;
using static Open_Ended_Item_Replacer.Utils.GetBoolFuncs;
using static Open_Ended_Item_Replacer.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components
{
    internal class NuuHandler
    {
        public static void Handle_Nuu(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Control" && __instance.gameObject?.name == "Hunter Fan Control")
            {
                FsmState check = __instance.Fsm.GetState("Check");
                FsmState left = __instance.Fsm.GetState("Left");
                if (check == null) { return; }

                check.Actions[0] = new SetFsmActiveState(__instance.Fsm, check, left, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene("Nuu", "Hunter Memento")), GetTrueFunc());
            }

            if (__instance.Fsm.Name == "Dialogue" && __instance.gameObject?.name == "Nuu")
            {
                FsmState hasJournal = __instance.Fsm.GetState("Has Journal?");
                FsmState journal = __instance.Fsm.GetState("Journal");
                FsmState journalRepeat = __instance.Fsm.GetState("Journal Repeat");
                FsmState journalHint = __instance.Fsm.GetState("Journal Hint");
                FsmState convoChoice = __instance.Fsm.GetState("Convo Choice");
                FsmState completionEvaluate = __instance.Fsm.GetState("Completion Evaluate");
                FsmState giveMemento = __instance.Fsm.GetState("Give Memento");
                if (hasJournal == null || journal == null || journalRepeat == null || convoChoice == null || completionEvaluate == null) { return; }

                string huntersJournal = "Hunter's Journal";

                hasJournal.Actions = new FsmStateAction[3];
                hasJournal.Actions[0] = new SetFsmActiveState(__instance.Fsm, hasJournal, journalRepeat, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene(huntersJournal, huntersJournal)), GetFalseFunc());
                hasJournal.Actions[1] = new SetFsmActiveState(__instance.Fsm, hasJournal, journalHint, GetPlayerDataBoolFunc("hasJournal"), GetFalseFunc());
                hasJournal.Actions[2] = new SetFsmActiveState(__instance.Fsm, hasJournal, convoChoice, GetPlayerDataBoolFunc("hasJournal"), GetTrueFunc());

                GameObject dummyGameObject = new GameObject(huntersJournal);
                journal.Actions[1] = new GetCheck(dummyGameObject, huntersJournal);

                completionEvaluate.Actions[0].Enabled = false;
                giveMemento.Actions[2].Enabled = false;
            }
        }
    }
}
