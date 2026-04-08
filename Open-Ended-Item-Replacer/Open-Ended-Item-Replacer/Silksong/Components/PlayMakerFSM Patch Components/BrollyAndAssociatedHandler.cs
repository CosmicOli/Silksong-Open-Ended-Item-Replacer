using HutongGames.PlayMaker;
using Open_Ended_Item_Replacer.Silksong.FsmStateActions;
using QuestPlaymakerActions;
using UnityEngine;

namespace Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components
{
    internal class BrollyAndAssociatedHandler
    {
        public static void Handle_Seamstress(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Dialogue" && __instance.gameObject?.name == "Seamstress")
            {
                FsmState msg = __instance.Fsm.GetState("Msg");
                if (msg == null) { return; }

                string driftersCloak = "Drifter's Cloak";

                GameObject dummyGameObject = new GameObject(driftersCloak);
                msg.Actions[3] = new GetCheck(dummyGameObject, driftersCloak);
            }
        }

        // Fourth chorus additive scene loading is handled independently to the FSM handler below
        public static void Handle_FourthChorus(PlayMakerFSM __instance)
        {
            // Fourth Chorus; "Control" "Boss Scene" "Init 18 and 19" -> Will need a more identifying part of this fsm to avoid triggering for other bosses

            if (__instance.Fsm.Name == "Control" && __instance.gameObject?.name == "Boss Scene")
            {
                FsmState activateReturnBombRock = __instance.Fsm.GetState("Activate Return Bomb Rock"); // Hopefully a unique enough state name to uniquely identify fourth chorus
                FsmState init = __instance.Fsm.GetState("Init");
                if (activateReturnBombRock == null || init == null) { return; }

                init.Actions[18] = init.Actions[19]; // Moves checking whether encountered Fourth Chorus prior to checking the quest

                CheckQuestState checkBrollyGetQuest = new CheckQuestState();
                checkBrollyGetQuest.Quest = QuestManager.GetQuest("Brolly Get");
                checkBrollyGetQuest.NotTrackedEvent = FsmEvent.GetFsmEvent("");
                checkBrollyGetQuest.TrackedEvent = FsmEvent.GetFsmEvent("");
                checkBrollyGetQuest.CompletedEvent = FsmEvent.GetFsmEvent("MEET READY");

                init.Actions[19] = checkBrollyGetQuest;
            }
        }
    }
}
