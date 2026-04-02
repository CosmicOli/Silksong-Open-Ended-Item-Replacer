using HutongGames.PlayMaker;
using UnityEngine;
using Open_Ended_Item_Replacer.Components;
using static Open_Ended_Item_Replacer.Utils.PersistenceUtils;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.FsmStateActions
{
    public class GetCheck : FsmStateAction
    {
        GenericSavedItem genericItem;

        public GetCheck(GameObject gameObject, string itemName)
        {
            genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();

            GeneratePersistentBoolSetToItem(gameObject, itemName, genericItem);
        }

        public GetCheck(string gameObjectName, string itemName)
        {
            genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();

            GeneratePersistentBoolSetToItem_SameScene(gameObjectName, itemName, genericItem);
        }

        public GetCheck(GameObject gameObject, string itemName, string sceneName)
        {
            genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();

            GeneratePersistentBoolSetToItem(gameObject, itemName, genericItem);
            genericItem.PersistentBoolItem.ItemData.SceneName = sceneName;
        }

        public GetCheck(string gameObjectName, string itemName, string sceneName)
        {
            genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();

            GeneratePersistentBoolSetToItem_SameScene(gameObjectName, itemName, genericItem);
            genericItem.PersistentBoolItem.ItemData.SceneName = sceneName;
        }

        public override void OnEnter()
        {
            // Handles persistence set by new item
            if (!GetPersistentBoolFromData(genericItem.PersistentBoolItem.ItemData))
            {
                genericItem.Get();
            }
            else
            {
                logSource.LogInfo("Replacement GetCheck set inactive");
            }

            Active = false;
            Finished = true;
            Finish();
        }
    }
}
