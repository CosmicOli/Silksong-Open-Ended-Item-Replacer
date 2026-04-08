using GlobalSettings;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Open_Ended_Item_Replacer.Core;
using Open_Ended_Item_Replacer.Core.Components;
using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;
using Open_Ended_Item_Replacer.Core.Containers;
using Open_Ended_Item_Replacer.Silksong.Components.Grant_Components;
using Open_Ended_Item_Replacer.Silksong.Containers.CollectableItemPickup_Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Open_Ended_Item_Replacer.Core.Utils.Replace_Utils.ReplaceUtils;
using static Open_Ended_Item_Replacer.Core.Utils.Replace_Utils.SpawnUtils;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using static Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components.FleaHandler;
using static Open_Ended_Item_Replacer.Silksong.Patches.CollectableItemPickup_Patches.Awake;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Silksong.Utils.Replace_Utils
{
    internal class ReplaceUtils
    {
        public static string replacementFlag = "-(Replacement)";

        // Moves and replaces a given object
        public static Transform Replace(GameObject replacedObject, string replacedItemName, bool interactable, Vector3 offset = new Vector3())
        {
            return Replace(replacedObject, replacedObject, replacedItemName, interactable, offset);
        }

        // Moves and replaces a given object
        public static Transform Replace(GameObject replacedObject, GameObject activeParent, string replacedItemName, bool interactable, Vector3 offset = new Vector3())
        {
            return Core_Replace<CollectableItemPickup_Abstract_Container, CollectableItemPickup_Container, CollectableItemPickupInstant_Container>(replacedObject, replacedObject, replacedItemName, interactable, offset);
        }

        // Moves and replaces a given object
        public static Transform ReplaceWithCostedPickup(GameObject replacedObject, string replacedItemName, CurrencyType currencyType, int currencyAmount, IReadOnlyList<SavedItem> requiredItems, IReadOnlyList<int> itemAmounts, Vector3 offset = new Vector3())
        {
            return Core_ReplaceWithCostedPickup<Costed_CollectableItemPickup_Container>(replacedObject, replacedItemName, currencyType, currencyAmount, requiredItems, itemAmounts, offset);
        }

        public static void ReplaceGiantFleaPickup(Transform giantFlea, PlayMakerFSM giantFleaFSM, PlayMakerFSM __instance, GameObject fleaObject)
        {
            // Generates a generic item using the uniqueID
            GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();

            GeneratePersistentBoolSetToItem(fleaObject, GenericFleaItemName, genericItem);

            // Handle actions on "Stun" state
            FsmStateAction[] stunActions = giantFleaFSM.Fsm.GetState("Stun").Actions;

            FsmBool giantFleaBool = new FsmBool();
            giantFleaBool.Value = false;

            // Stops the giant flea being marked as saved in the player bools
            HutongGames.PlayMaker.Actions.SetPlayerDataBool setGiantFleaSaved = stunActions.OfType<HutongGames.PlayMaker.Actions.SetPlayerDataBool>().ToList()[0];
            Traverse.Create(setGiantFleaSaved).Field("value").SetValue(giantFleaBool);


            // Handle actions on "Deactivate" state
            FsmStateAction[] deactivateActions = giantFleaFSM.Fsm.GetState("Deactivate").Actions;

            SavedItemGetV2 getFleaItem = deactivateActions.OfType<SavedItemGetV2>().ToList()[0];
            getFleaItem.Item = genericItem;
            getFleaItem.ShowPopup = true;
            getFleaItem.Amount = 1;


            // Handles persistence set by new item
            if (GetPersistentBoolFromData(genericItem.PersistentBoolItem.ItemData))
            {
                giantFlea.gameObject.SetActive(false);
                __instance.gameObject.SetActive(false);
            }
        }

        public static void ReplaceFsmItemGet(FsmStateAction __instance, SavedItem item)
        {
            GameObject gameObject = new GameObject();
            gameObject.name = __instance.Owner?.gameObject?.name;
            gameObject.transform.position = HeroController.instance.transform.position;

            if (gameObject.name == null)
            {
                gameObject.name = "dummyName";
            }

            GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();
            GeneratePersistentBoolSetToItem(gameObject, item.name, genericItem);

            // Handles persistence set by new item
            if (GetPersistentBoolFromData(genericItem.PersistentBoolItem.ItemData))
            {
                logSource.LogInfo("Replacement set inactive: " + genericItem.PersistentBoolItem.ItemData.SceneName + "   " + genericItem.PersistentBoolItem.ItemData.ID);
            }
            else
            {
                genericItem.Get();
            }
        }

        public static void ReplaceFsmToolGet(SetToolUnlocked __instance)
        {
            GameObject gameObject = new GameObject();
            gameObject.name = __instance.Owner?.gameObject?.name;
            gameObject.transform.position = HeroController.instance.transform.position;

            if (gameObject.name == null)
            {
                gameObject.name = "dummyName";
            }

            GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();
            GeneratePersistentBoolSetToItem(gameObject, (__instance.Tool.Value as ToolItem).name, genericItem);

            // Handles persistence set by new item
            if (GetPersistentBoolFromData(genericItem.PersistentBoolItem.ItemData))
            {
                logSource.LogInfo("Replacement set inactive");
            }
            else
            {
                genericItem.Get();
            }
        }
    }
}
