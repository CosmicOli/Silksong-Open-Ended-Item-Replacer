using HutongGames.PlayMaker;
using Open_Ended_Item_Replacer.FsmStateActions;
using UnityEngine;
using static Open_Ended_Item_Replacer.Utils.GetBoolFuncs;
using static Open_Ended_Item_Replacer.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components
{
    internal class CreigeHandler
    {
        public static void HandleNectar(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Dialogue" && __instance.gameObject?.name == "HH Bartender")
            {
                FsmState nectar = __instance.Fsm.GetState("Nectar?");
                FsmState questState = __instance.Fsm.GetState("Quest State");
                if (nectar == null || questState == null) { return; }

                GameObject dummyGameObject = new GameObject("Collectable Item Pickup");
                PersistentItemData<bool> persistentData = GeneratePersistentBoolData(dummyGameObject, "Vintage Nectar");
                persistentData.SceneName = "Ant_08";

                nectar.Actions[2].Enabled = false;
                nectar.Actions[3] = new SetFsmActiveState(__instance.Fsm, questState, GetPersistentBoolFromDataFunc(persistentData), GetTrueFunc());// This usually is what checks for the item itself, so I am replacing it in particular with the persistence check
                nectar.Actions[4].Enabled = false;
            }

            if (__instance.Fsm.Name == "Trapdoor" && __instance.gameObject?.name == "cellar trapdoor set")
            {
                FsmState hasNectar = __instance.Fsm.GetState("Has Nectar");
                FsmState opened = __instance.Fsm.GetState("Opened");
                FsmState close = __instance.Fsm.GetState("Close");
                if (hasNectar == null || close == null) { return; }

                GameObject dummyGameObject = new GameObject("Collectable Item Pickup");
                PersistentItemData<bool> persistentData = GeneratePersistentBoolData(dummyGameObject, "Vintage Nectar");
                persistentData.SceneName = "Ant_08";

                hasNectar.Actions = new FsmStateAction[2];
                hasNectar.Actions[0] = new SetFsmActiveState(__instance.Fsm, hasNectar, opened, GetPersistentBoolFromDataFunc(persistentData), GetFalseFunc());
                hasNectar.Actions[1] = new SetFsmActiveState(__instance.Fsm, hasNectar, close, GetPersistentBoolFromDataFunc(persistentData), GetTrueFunc());
            }

            if (__instance.gameObject == null) { return; }
            if (__instance.Fsm.Name == "Control" && __instance.gameObject.name.Contains("BattleStart Inspect Region"))
            {
                FsmState wave1Start = __instance.Fsm.GetState("Wave 1 Start");
                if (wave1Start == null || __instance.gameObject.scene.name != "Ant_08") { return; }
                wave1Start.Actions[1].Enabled = false;
            }
        }
    }
}
