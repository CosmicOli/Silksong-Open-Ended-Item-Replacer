using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Open_Ended_Item_Replacer.FsmStateActions;
using static Open_Ended_Item_Replacer.Utils.GetBoolFuncs;
using static Open_Ended_Item_Replacer.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components
{
    // See CrullAndBenjinHandler for Steel Spines (seperate as giving independantly to quest)
    internal class CurseHandler
    {
        public static void Handle_WoodWitch(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Dialogue" && __instance.gameObject?.name == "Wood Witch")
            {
                FsmState pollipReward = __instance.Fsm.GetState("Pollip Reward?");
                FsmState getPollipReward = __instance.Fsm.GetState("Get Pollip Reward");
                if (pollipReward == null || getPollipReward == null) { return; }

                // Replaces persistence for end of first quest
                pollipReward.Actions[1] = new SetFsmActiveState(__instance.Fsm, getPollipReward, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData(__instance.gameObject, "Poison Pouch")), GetFalseFunc());
            }
        }

        public static void Handle_DoctorFly(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Dialogue" && __instance.gameObject?.name == "Doctor Fly")
            {
                FsmState comboBarPrompt = __instance.Fsm.GetState("Combo Bar Prompt");
                FsmState crestGetAntic = __instance.Fsm.GetState("Crest Get Antic");
                FsmState crestChange = __instance.Fsm.GetState("Crest Change");
                if (comboBarPrompt == null || crestGetAntic == null || crestChange == null) { return; }

                comboBarPrompt.Actions[1] = new SetFsmActiveState(__instance.Fsm, comboBarPrompt, crestGetAntic);
                crestGetAntic.Actions[0].Enabled = false;
                (crestGetAntic.Actions[2] as Wait).time = 0f;
                (crestGetAntic.Actions[3] as ScreenFader).duration = 0.5f;
                (crestGetAntic.Actions[4] as Wait).time = 0.5f;
                crestGetAntic.Actions[5].Enabled = false;

                crestChange.Actions[0] = new GetCheck(__instance.gameObject, "Witch");
                (crestChange.Actions[1] as AutoEquipCrestV2).Crest.Value = ToolItemManager.GetCrestByName(PlayerData.instance.PreviousCrestID);
            }
        }
    }
}
