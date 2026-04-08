using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Open_Ended_Item_Replacer.Silksong.FsmStateActions;
using UnityEngine;
using static Open_Ended_Item_Replacer.Silksong.Utils.FsmStateActionUtils;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components
{
    internal class FirstSinnerHandler
    {
        private static string runeRage = "Rune Rage";
        public static void Handle_FirstSinnerPersistenceAndPickup(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Inspection" && __instance.gameObject?.name == "Shrine First Weaver")
            {
                FsmState init = __instance.Fsm.GetState("Init");
                if (init == null) { return; }

                (init.Actions[0] as PlayerDataBoolTest).isTrue = new FsmEvent(""); // disables checking for rune bomb

                // Handles persistence set by new item
                GameObject dummyGameObject = new GameObject(runeRage);
                UniqueID uniqueID = new UniqueID(dummyGameObject, runeRage);
                if (GetPersistentBoolFromData("Memory_First_Sinner", uniqueID.PickupName))
                {
                    __instance.gameObject.SetActive(false);
                }
            }
        }

        public static void Handle_FirstSinnerInMemory(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Memory Control" && __instance.gameObject?.name == "Memory Control")
            {
                FsmState getRuneBomb = __instance.Fsm.GetState("Get Rune Bomb");
                if (getRuneBomb == null) { return; }

                FsmState needolinPrompt = __instance.Fsm.GetState("Needolin Prompt");
                needolinPrompt.Actions[1].Enabled = false; // disables giving needolin
                needolinPrompt.Actions[2].Enabled = false; // disables setting having beaten widow to true

                getRuneBomb.Actions[0].Enabled = false; // disables owning rune bomb
                getRuneBomb.Actions[1].Enabled = false; // disables auto equipping rune bomb
                getRuneBomb.Actions[2].Enabled = false; // disables displaying rune bomb

                int numberOfNewActions = 1;

                FsmStateAction[] newActions = new FsmStateAction[getRuneBomb.Actions.Length + numberOfNewActions];

                GameObject dummyGameObject = new GameObject(runeRage);
                newActions[0] = new GetCheck(dummyGameObject, runeRage); // Replace

                //Array.Copy(getRuneBomb.Actions, 0, newActions, numberOfNewActions, getRuneBomb.Actions.Length);
                getRuneBomb.Actions = ReturnCombinedActions(newActions, getRuneBomb.Actions);

                //getRuneBomb.Actions = newActions;
            }
        }
    }
}
