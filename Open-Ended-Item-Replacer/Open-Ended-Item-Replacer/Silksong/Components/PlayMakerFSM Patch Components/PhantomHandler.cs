using HutongGames.PlayMaker;
using Open_Ended_Item_Replacer.Silksong.FsmStateActions;
using UnityEngine;
using static Open_Ended_Item_Replacer.Silksong.Utils.FsmStateActionUtils;

namespace Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components
{
    internal class PhantomHandler
    {
        public static void Handle_Phantom(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Control" && __instance.gameObject?.name == "Phantom")
            {
                FsmState UIMsg = __instance.Fsm.GetState("UI Msg");
                FsmState setData = __instance.Fsm.GetState("Set Data");
                if (UIMsg == null || setData == null) { return; }

                string parry = "Parry";

                UIMsg.Actions[1].Enabled = false; // disables giving parry
                UIMsg.Actions[5].Enabled = false; // disables auto equipping parry
                UIMsg.Actions[6].Enabled = false; // disables displaying parry

                setData.Actions[0].Enabled = false; // disables giving parry

                FsmStateAction[] newActionsPre = new FsmStateAction[1];

                GameObject dummyGameObject = new GameObject(parry);
                newActionsPre[0] = new GetCheck(dummyGameObject, parry); // Replace

                /*FsmOwnerDefault ownerDefault = new FsmOwnerDefault();
                ownerDefault.GameObject = __instance.gameObject;
                ownerDefault.OwnerOption = OwnerDefaultOption.SpecifyGameObject;

                FsmEventTarget eventTarget = new FsmEventTarget();
                eventTarget.target = EventTarget.BroadcastAll;
                eventTarget.excludeSelf = false;
                eventTarget.gameObject = ownerDefault;
                eventTarget.fsmName = __instance.Fsm.Name;
                eventTarget.sendToChildren = true;
                eventTarget.fsmComponent = __instance;

                SendEventByName msgFadeOut = new SendEventByName();
                msgFadeOut.eventTarget = eventTarget;
                msgFadeOut.sendEvent = "SKILL GET MSG FADED OUT";
                msgFadeOut.delay = 0;

                SendEventByName msgEnd = new SendEventByName();
                msgEnd.eventTarget = eventTarget;
                msgEnd.sendEvent = "SKILL GET MSG ENDED";
                msgFadeOut.delay = 0;*/

                //Array.Copy(UIMsg.Actions, 0, newActions, numberOfNewActions - 1, UIMsg.Actions.Length);

                UIMsg.Actions = ReturnCombinedActions(newActionsPre, UIMsg.Actions);

                FsmStateAction[] newActionsPost = new FsmStateAction[1];
                //newActionsPost[0] = new SetFsmActiveState(__instance.Fsm, UIMsg, __instance.Fsm.GetState("End Pause"), false);
                newActionsPost[0] = new SetFsmActiveState(__instance.Fsm, __instance.Fsm.GetState("End Pause")); // Replaces original persistence check with custom

                UIMsg.Actions = ReturnCombinedActions(UIMsg.Actions, newActionsPost);

                //UIMsg.Actions = newActions;
            }
        }
    }
}
