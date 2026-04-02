using HutongGames.PlayMaker;
using Open_Ended_Item_Replacer.FsmStateActions;
using static Open_Ended_Item_Replacer.Utils.PersistenceUtils;
using static Open_Ended_Item_Replacer.Utils.GetBoolFuncs;
using static Open_Ended_Item_Replacer.Utils.FsmStateActionUtils;
using HutongGames.PlayMaker.Actions;

namespace Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components
{
    internal class CraftPickupHandler
    {
        public static void Handle_CraftPickup(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "FSM" && __instance.gameObject?.name == "Craft Pickup")
            {
                FsmState init = __instance.Fsm.GetState("Init");
                FsmState gone = __instance.Fsm.GetState("Gone");
                FsmState idle = __instance.Fsm.GetState("Idle");
                FsmState prompt = __instance.Fsm.GetState("Prompt");

                if (init == null || gone == null || idle == null || prompt == null) { return; }

                init.Actions[3] = new SetFsmActiveState(__instance.Fsm, init, gone, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData(__instance.gameObject, __instance.Fsm.GetFsmObject("Item").Value.name)), GetTrueFunc());
                init.Actions = ReturnCombinedActions(init.Actions, new FsmStateAction[] { new SetFsmActiveState(__instance.Fsm, init, idle, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData(__instance.gameObject, __instance.Fsm.GetFsmObject("Item").Value.name)), GetFalseFunc()) });

                (prompt.Actions[0] as DialogueYesNoItemV3).WillGetItem = new FsmObject();
            }
        }

        // This doesn't actually need changing; to check for persistence, it litterally just checks whether you have the broken tool
        /*public static void HandleMtFaySilkshot(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Repair webshot" && __instance.gameObject?.name == "Webshot Scene")
            {
                
            }
        }*/
    }
}
