using BepInEx.Logging;
using System;
using UnityEngine;
using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;

namespace Open_Ended_Item_Replacer.Silksong.Components.Replacement_Components
{
    // This object defines the item that replaces intended items
    // TODO: 
    // -> Show popup
    // -> Make functions make open ended requests
    public class GenericSavedItem : SavedItem, IGenericItem
    {
        private UniqueID uniqueID;
        public UniqueID UniqueID
        {
            get
            {
                return uniqueID;
            }

            set
            {
                uniqueID = value;
                name = "Generic_Item-" + uniqueID.PickupName + "-" + uniqueID.SceneName;
            }
        }

        private PersistentBoolItem persistentBoolItem;
        public PersistentBoolItem PersistentBoolItem
        {
            get { return persistentBoolItem; }
            set { persistentBoolItem = value; }
        }

        public override void Get(bool showPopup = true)
        {
            ManualLogSource logSource = Open_Ended_Item_Replacer.logSource;

            // Show popup (if showPopup)
            // Send get request
            PersistentBoolItem.ItemData.Value = true;
            SceneData.instance.PersistentBools.SetValue(PersistentBoolItem.ItemData);
            logSource.LogInfo("Item get:  " + PersistentBoolItem.ItemData.ID + "  In Scene: " + PersistentBoolItem.ItemData.SceneName);
        }

        public override bool CanGetMore()
        {
            return true;
        }

        public override Sprite GetPopupIcon()
        {
            if (Application.isPlaying)
            {
                UnityEngine.Debug.LogException(new NotImplementedException());
            }

            return null;
        }

        public override string GetPopupName()
        {
            if (Application.isPlaying)
            {
                //UnityEngine.Debug.LogException(new NotImplementedException());
                return PersistentBoolItem.ToString();
            }

            return null;
        }
    }
}
