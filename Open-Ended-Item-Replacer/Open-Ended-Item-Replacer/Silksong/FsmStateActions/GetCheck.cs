using HutongGames.PlayMaker;
using UnityEngine;
using Open_Ended_Item_Replacer.Silksong.Components.Replacement_Components;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;

namespace Open_Ended_Item_Replacer.Silksong.FsmStateActions
{
    public class GetCheck : FsmStateAction
    {
        IGenericItem genericSavedItem;

        public GetCheck(GameObject gameObject, string itemName)
        {
            genericSavedItem = ScriptableObject.CreateInstance<GenericSavedItem>();

            GeneratePersistentBoolSetToItem(gameObject, itemName, genericSavedItem);
        }

        public GetCheck(string gameObjectName, string itemName)
        {
            genericSavedItem = ScriptableObject.CreateInstance<GenericSavedItem>();

            GeneratePersistentBoolSetToItem_SameScene(gameObjectName, itemName, genericSavedItem);
        }

        public GetCheck(GameObject gameObject, string itemName, string sceneName)
        {
            genericSavedItem = ScriptableObject.CreateInstance<GenericSavedItem>();

            GeneratePersistentBoolSetToItem(gameObject, itemName, genericSavedItem);
            genericSavedItem.PersistentBoolItem.ItemData.SceneName = sceneName;
        }

        public GetCheck(string gameObjectName, string itemName, string sceneName)
        {
            genericSavedItem = ScriptableObject.CreateInstance<GenericSavedItem>();

            GeneratePersistentBoolSetToItem_SameScene(gameObjectName, itemName, genericSavedItem);
            genericSavedItem.PersistentBoolItem.ItemData.SceneName = sceneName;
        }

        public GetCheck(IGenericItem genericSavedItem)
        {
            this.genericSavedItem = genericSavedItem;
        }

        public override void OnEnter()
        {
            // Handles persistence set by new item
            if (!GetPersistentBoolFromData(genericSavedItem.PersistentBoolItem.ItemData))
            {
                genericSavedItem.Get();
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
