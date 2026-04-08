using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Open_Ended_Item_Replacer.FsmStateActions;
using static Open_Ended_Item_Replacer.Silksong.Utils.FsmStateActionUtils;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Silksong.Utils.Replace_Utils
{
    internal class UiReplaceUtils
    {
        public static void HandleUiMsgGetItem(PlayMakerFSM playMakerFsm)
        {
            // As these fsms are spawned from a template, I am unsure whether the names will exactly match
            if (playMakerFsm.Fsm.Name.Contains("Msg Control") && playMakerFsm.gameObject.name.Contains("UI Msg Get Item"))
            {
                if (playMakerFsm.gameObject.name.Contains("Melody")) { return; }

                logSource.LogMessage("Ui Msg Get Item found");

                FsmState init = playMakerFsm.Fsm.GetState("Init");
                FsmState done = playMakerFsm.Fsm.GetState("Done");

                int numberOfNewActions = 1;
                FsmStateAction[] newActions = new FsmStateAction[numberOfNewActions];

                newActions[0] = new SetFsmActiveState(playMakerFsm.Fsm, init, done);

                init.Actions = ReturnCombinedActions(newActions, init.Actions);
            }
        }

        public static void HandleUiMsgGetItemMelody(PlayMakerFSM playMakerFsm, SpawnObjectFromGlobalPool __instance)
        {
            // As these fsms are spawned from a template, I am unsure whether the names will exactly match
            if (playMakerFsm.Fsm.Name.Contains("Msg Control") && playMakerFsm.gameObject.name.Contains("UI Msg Get Item Melody"))
            {
                logSource.LogMessage("Ui Msg Get Item Melody found");

                FsmState init = playMakerFsm.Fsm.GetState("Init");
                FsmState stopPlaying = playMakerFsm.Fsm.GetState("Stop Playing");
                FsmState wait = playMakerFsm.Fsm.GetState("Wait");
                FsmState stopUp = playMakerFsm.Fsm.GetState("Stop Up");
                FsmState done = playMakerFsm.Fsm.GetState("Done");

                FsmStateAction[] newActionsForInit = new FsmStateAction[1];
                newActionsForInit[0] = new SetFsmActiveState(playMakerFsm.Fsm, init, stopPlaying);
                //Array.Copy(init.Actions, 0, newActionsForInit, numberOfNewActionsForInit, init.Actions.Length);
                //init.Actions = newActionsForInit;
                init.Actions = ReturnCombinedActions(newActionsForInit, init.Actions);

                (wait.Actions[0] as Wait).time = 0;

                FsmStateAction[] newActionsForStopUp = new FsmStateAction[1];
                newActionsForStopUp[newActionsForStopUp.Length - 1] = new SetFsmActiveState(playMakerFsm.Fsm, stopUp, done);
                //Array.Copy(init.Actions, 0, newActionsForStopUp, 0, init.Actions.Length);
                //stopUp.Actions = newActionsForStopUp;
                stopUp.Actions = ReturnCombinedActions(newActionsForStopUp, stopUp.Actions);

                foreach (FsmStateAction action in __instance.State.Actions)
                {
                    SendEventByName sendEventByName = action as SendEventByName;

                    if (sendEventByName != null)
                    {
                        sendEventByName.Enabled = false;
                    }
                }

                playMakerFsm.Fsm.Start(); // This would usually start in some other way that also shows the message, so this needs to be started independantly
            }
        }
    }
}
