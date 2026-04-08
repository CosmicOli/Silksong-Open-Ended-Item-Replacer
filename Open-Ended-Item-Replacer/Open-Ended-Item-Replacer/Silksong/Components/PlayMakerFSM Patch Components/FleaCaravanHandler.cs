using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Open_Ended_Item_Replacer.Silksong.FsmStateActions;
using UnityEngine;
using static Open_Ended_Item_Replacer.Silksong.Utils.FsmStateActionUtils;
using static Open_Ended_Item_Replacer.Silksong.Utils.GetBoolFuncs;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components
{
    internal class FleaCaravanHandler
    {
        private static string fleaBrew = "Flea Brew";
        private static bool CheckAllCaravanScenesForFleaBrew()
        {
            PersistentItemData<bool> data = GeneratePersistentBoolData_SameScene(fleaBrew, fleaBrew);

            data.SceneName = "Greymoor_08";
            bool greymoor = GetPersistentBoolFromData(data);

            data.SceneName = "Coral_Judge_Arena";
            bool ljArena = GetPersistentBoolFromData(data);

            data.SceneName = "Aqueduct_05";
            bool fleatopia = GetPersistentBoolFromData(data);

            return (greymoor || ljArena || fleatopia);
        }

        public static void Handle_Grishkin(PlayMakerFSM __instance)
        {
            if (__instance.gameObject == null) { return; }

            if (__instance.Fsm.Name == "Behaviour" && __instance.gameObject.name.Contains("Caravan Troup Member Short")) // Yes troup is right
            {
                FsmState brew = __instance.Fsm.GetState("Brew?");
                FsmState met2 = __instance.Fsm.GetState("Met? 2");
                FsmState brewFull = __instance.Fsm.GetState("Brew Full?");
                FsmState meetBrew = __instance.Fsm.GetState("Meet Brew");
                FsmState giveBrew = __instance.Fsm.GetState("Give Brew");
                FsmState repeatDlg = __instance.Fsm.GetState("Repeat Dlg");
                if (brew == null || met2 == null || brewFull == null || meetBrew == null || giveBrew == null || repeatDlg == null) { return; }

                GameObject fleaBrewGameObject = new GameObject(fleaBrew); // Standardises the object name across all locations
                giveBrew.Actions[2] = new GetCheck(fleaBrewGameObject, fleaBrew);

                brew.Actions[1] = new SetFsmActiveState(__instance.Fsm, brew, meetBrew, CheckAllCaravanScenesForFleaBrew, GetFalseFunc());

                (met2.Actions[2] as BoolTest).isFalse = FsmEvent.GetFsmEvent("");
                met2.Actions = ReturnCombinedActions(new FsmStateAction[] { new SetFsmActiveState(__instance.Fsm, met2, meetBrew, CheckAllCaravanScenesForFleaBrew, GetFalseFunc()) }, met2.Actions);
                met2.Actions = ReturnCombinedActions(met2.Actions, new FsmStateAction[] { new SetFsmActiveState(__instance.Fsm, brew) });
            }

            if (__instance.Fsm.Name == "FSM" && __instance.gameObject.name.Contains("Refill Inspect")) // For fleatopia
            {
                FsmState brew = __instance.Fsm.GetState("Brew?");
                FsmState brewInspect = __instance.Fsm.GetState("Brew Inspect");
                FsmState giveBrew = __instance.Fsm.GetState("Give Brew");
                if (brew == null || brewInspect == null || giveBrew == null) { return; }

                GameObject fleaBrewGameObject = new GameObject(fleaBrew); // Standardises the object name across all locations
                giveBrew.Actions[0] = new GetCheck(fleaBrewGameObject, fleaBrew);

                bool HasPersistentAndNotBrew()
                {
                    return CheckAllCaravanScenesForFleaBrew() && !__instance.Fsm.GetFsmBool("Brew unlocked").Value;
                }

                brew.Actions = ReturnCombinedActions(new FsmStateAction[2] { new SetFsmActiveState(__instance.Fsm, brew, giveBrew, CheckAllCaravanScenesForFleaBrew, GetFalseFunc()), brew.Actions[0] }, brew.Actions);
                brew.Actions[2] = new SetFsmActiveState(__instance.Fsm, brew, brewInspect, HasPersistentAndNotBrew, GetTrueFunc());
            }
        }

        // This only handles the persistence
        public static void Handle_FleaCharm(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Dialogue" && __instance.gameObject?.name == "Caravan Troupe Leader Fleatopia NPC") // Yes troupe is right
            {
                FsmState shouldWave = __instance.Fsm.GetState("Should Wave?");
                FsmState met = __instance.Fsm.GetState("Met?");
                if (shouldWave == null || met == null) { return; }

                UniqueID uniqueID = new UniqueID(__instance.gameObject, "Flea Charm");

                GetPersistentBoolFromSaveData fleaCharmGetPersistentBool = new GetPersistentBoolFromSaveData();

                fleaCharmGetPersistentBool.Target = new FsmOwnerDefault();
                fleaCharmGetPersistentBool.Target.OwnerOption = OwnerDefaultOption.SpecifyGameObject;
                fleaCharmGetPersistentBool.Target.GameObject = new FsmGameObject();
                fleaCharmGetPersistentBool.Target.GameObject.Value = null;

                fleaCharmGetPersistentBool.ID = uniqueID.PickupName;
                fleaCharmGetPersistentBool.SceneName = uniqueID.SceneName;
                fleaCharmGetPersistentBool.StoreValue = __instance.Fsm.GetFsmBool("Has Flea Charm");

                shouldWave.Actions[4] = fleaCharmGetPersistentBool;
                met.Actions[3] = fleaCharmGetPersistentBool;
            }
        }

        // This only handles the persistence
        public static void Handle_SethMemento(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Dialogue" && __instance.gameObject?.name == "Seth Sit NPC Fleatopia")
            {
                FsmState convoChoice = __instance.Fsm.GetState("Convo Choice");
                FsmState awardMemento = __instance.Fsm.GetState("Award Memento?");
                if (convoChoice == null || awardMemento == null) { return; }

                UniqueID uniqueID = new UniqueID(__instance.gameObject, "Memento Seth");

                GetPersistentBoolFromSaveData mementoSethGetPersistentBool = new GetPersistentBoolFromSaveData();

                mementoSethGetPersistentBool.Target = new FsmOwnerDefault();
                mementoSethGetPersistentBool.Target.OwnerOption = OwnerDefaultOption.SpecifyGameObject;
                mementoSethGetPersistentBool.Target.GameObject = new FsmGameObject();
                mementoSethGetPersistentBool.Target.GameObject.Value = null;

                mementoSethGetPersistentBool.ID = uniqueID.PickupName;
                mementoSethGetPersistentBool.SceneName = uniqueID.SceneName;
                mementoSethGetPersistentBool.StoreValue = __instance.Fsm.GetFsmBool("Memento Collected");

                convoChoice.Actions[2] = mementoSethGetPersistentBool;
                convoChoice.Actions[3].Enabled = false; // Disabled flipping the bool

                awardMemento.Actions[1] = new GetCheck(__instance.gameObject, "Memento Seth");
            }
        }
    }
}
