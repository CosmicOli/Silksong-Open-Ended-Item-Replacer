using HutongGames.PlayMaker;
using UnityEngine;
using Open_Ended_Item_Replacer.Silksong.FsmStateActions;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;
using static Open_Ended_Item_Replacer.Silksong.Utils.GetBoolFuncs;

namespace Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components
{
    internal class WeaverStatueHandler
    {
        public static void Handle_WeaverStatue(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Inspection" && __instance.gameObject?.name == "Shrine Weaver Ability")
            {
                FsmState collectedCheckState = __instance.Fsm.GetState("Collected Check");
                FsmState abilityCollected = __instance.Fsm.GetState("Ability Collected");
                FsmState autoEquip = __instance.Fsm.GetState("Auto Equip");
                FsmState heal = __instance.Fsm.GetState("Heal");
                FsmState end = __instance.Fsm.GetState("End");
                if (collectedCheckState == null || abilityCollected == null || autoEquip == null || heal == null || end == null) { return; }

                string abilityName = __instance.Fsm.GetFsmEnum("Ability").Value.ToString();

                collectedCheckState.Actions[0] = new SetFsmActiveState(__instance.Fsm, abilityCollected, GetPersistentBoolFromDataFunc(GeneratePersistentBoolData_SameScene(abilityName, abilityName)), GetTrueFunc()); // Replaces original persistence checking

                autoEquip.Actions = new FsmStateAction[] { new SetFsmActiveState(__instance.Fsm, heal) }; // Disables auto equipping tool

                GameObject dummyGameObject = new GameObject(abilityName);
                end.Actions[2] = new GetCheck(dummyGameObject, abilityName);
                end.Actions[3].Enabled = false;
                end.Actions[4].Enabled = false;
            }
        }
    }
}
