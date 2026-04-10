using HutongGames.PlayMaker;
using Open_Ended_Item_Replacer.Silksong.FsmStateActions;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;
using static Open_Ended_Item_Replacer.Silksong.Utils.GetBoolFuncs;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components
{
    internal class CrullAndBenjinHandler
    {
        public static void Handle_SteelSpines(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Dialogue" && __instance.gameObject?.name == "Dust Traders")
            {
                FsmState state = __instance.Fsm.GetState("State?");
                FsmState pinsQuestActive = __instance.Fsm.GetState("Pins Quest Active?");
                FsmState pinsState = __instance.Fsm.GetState("Pins State?");
                FsmState hasPins = __instance.Fsm.GetState("Has Pins");
                if (state == null || pinsQuestActive == null || pinsState == null || hasPins == null) { return; }

                PersistentItemData<bool> persistent = GeneratePersistentBoolData(__instance.gameObject, "Extractor Machine Pins");

                state.Actions[1] = new SetFsmActiveState(__instance.Fsm, state, pinsState, GetPersistentBoolFromDataFunc(persistent), GetFalseFunc());
                pinsQuestActive.Actions[0].Enabled = false;
                pinsQuestActive.Actions[1] = new SetFsmActiveState(__instance.Fsm, pinsQuestActive, pinsState, GetPersistentBoolFromDataFunc(persistent), GetFalseFunc());

                pinsState.Actions[0] = new SetFsmActiveState(__instance.Fsm, pinsState, hasPins, GetPersistentBoolFromDataFunc(persistent), GetTrueFunc());
            }
        }
    }
}

