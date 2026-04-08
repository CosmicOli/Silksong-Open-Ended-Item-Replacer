using HarmonyLib;
using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;
using Open_Ended_Item_Replacer.Core.Containers;
using UnityEngine;

namespace Open_Ended_Item_Replacer.Silksong.Containers.CollectableItemPickup_Containers
{
    public abstract class CollectableItemPickup_Abstract_Container : MonoBehaviour, IContainer
    {
        protected CollectableItemPickup collectableItemPickupInstance;
        public CollectableItemPickup CollectableItemPickupInstance
        {
            get { return collectableItemPickupInstance; }
            protected set { collectableItemPickupInstance = value; }
        }

        // In this class, there are two items that represent basically the same thing
        // You can desync them to make the collectableItemPickup function on a different/dummy item, e.g. a fake collectable when you need granting to be handled seperately
        private GenericSavedItem item;
        public GenericSavedItem Item
        {
            get
            {
                return item;
            }
            set
            {
                collectableItemPickupInstance.SetItem(value, true);
                item = value;
            }
        }

        // This allows for desyncing Item and the collectableItemPickup.Item
        public void SetCollectableItemPickupItem(SavedItem item, bool keepPersistence = false)
        {
            collectableItemPickupInstance.SetItem(item, keepPersistence);
        }

        public void Setup()
        {
            // Ignores areas that disallow replacement pickups such that it can exist anyways
            Traverse.Create(collectableItemPickupInstance).Field("ignoreCanExist").SetValue(true);

            // Ensures new replacements made post-pickup don't reset the persistent data
            Traverse.Create(collectableItemPickupInstance).Field("activatedSave").SetValue(true);
        }

        public PersistentBoolItem ContainerPersistentBoolItem
        {
            get
            {
                return Traverse.Create(collectableItemPickupInstance).Field("persistent").GetValue<PersistentBoolItem>();
            }
        }
    }
}
