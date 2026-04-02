using Open_Ended_Item_Replacer.Components;
using TeamCherry.SharedUtils;
using UnityEngine;
using static SceneData;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Utils
{
    public class PersistenceUtils
    {
        public static bool GetPersistentBoolFromData(PersistentItemData<bool> persistentBoolData)
        {
            return SceneData.instance.PersistentBools.GetValueOrDefault(persistentBoolData.SceneName, persistentBoolData.ID);
        }

        public static bool GetPersistentBoolFromData(string sceneName, string persistentID)
        {
            return SceneData.instance.PersistentBools.GetValueOrDefault(sceneName, persistentID);
        }

        public static bool GetPlayerDataBool(string playerDataBool)
        {
            if (!VariableExtensions.VariableExists<bool, PlayerData>(playerDataBool))
            {
                return false;
            }

            return GameManager.instance.GetPlayerDataBool(playerDataBool);
        }

        public static PersistentBoolItem GeneratePersistentBoolSetToItem_SameScene(string gameObjectName, string originalItemName, GenericSavedItem replacementItem)
        {
            GameObject gameObject = new GameObject(gameObjectName);
            return GeneratePersistentBoolSetToItem(gameObject, originalItemName, replacementItem);
        }

        public static PersistentBoolItem GeneratePersistentBoolSetToItem(GameObject gameObject, string originalItemName, GenericSavedItem replacementItem)
        {
            UniqueID uniqueID = new UniqueID(gameObject, originalItemName);
            replacementItem.UniqueID = uniqueID;

            PersistentBoolItem persistent = gameObject.AddComponent<PersistentBoolItem>();
            SetGenericPersistentInfo(uniqueID, persistent);

            replacementItem.PersistentBoolItem = persistent;

            return persistent;
        }

        public static PersistentItemData<bool> GeneratePersistentBoolData_SameScene(string gameObjectName, string originalItemName)
        {
            GameObject gameObject = new GameObject(gameObjectName);
            return GeneratePersistentBoolData(gameObject, originalItemName);
        }

        public static PersistentItemData<bool> GeneratePersistentBoolData(GameObject gameObject, string originalItemName)
        {
            UniqueID uniqueID = new UniqueID(gameObject, originalItemName);
            PersistentItemData<bool> persistentBoolData = new PersistentItemData<bool>();
            SetGenericPersistentBoolDataInfo(uniqueID, persistentBoolData);

            return persistentBoolData;
        }

        public static void SetGenericPersistentInfo(UniqueID uniqueID, PersistentBoolItem persistent)
        {
            // Makes sure that persistent has loaded and that hasSetup = true
            persistent.LoadIfNeverStarted();
            persistent.ItemData.ToString();

            // Sets persistent data
            SetGenericPersistentBoolDataInfo(uniqueID, persistent.ItemData);

            // The value needs to be overidden to stop the default handling breaking things
            persistent.SetValueOverride(persistent.ItemData.Value);
        }

        public static void SetGenericPersistentBoolDataInfo(UniqueID uniqueID, PersistentItemData<bool> persistentBoolData)
        {
            // Sets persistent data
            persistentBoolData.ID = uniqueID.PickupName;
            persistentBoolData.SceneName = uniqueID.SceneName;
            persistentBoolData.IsSemiPersistent = false;
            persistentBoolData.Mutator = PersistentMutatorTypes.None;
            persistentBoolData.Value = GetPersistentBoolFromData(persistentBoolData); // By default this returns false, but if it has been picked up before it is true
        }
    }
}
