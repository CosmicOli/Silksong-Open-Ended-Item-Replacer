using GlobalSettings;
using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;
using Open_Ended_Item_Replacer.Silksong.Containers.CollectableItemPickup_Containers.Bases;
using Open_Ended_Item_Replacer.Silksong.Containers.General_Bases;
using UnityEngine;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Silksong.Containers.CollectableItemPickup_Containers
{
    public class CollectableItemPickupInstant_Container : CollectableItemPickup_Abstract_Container, ICollisionable
    {
        public static CollectableItemPickupInstant_Container Prefab
        {
            get
            {
                SpawningReplacement = true;
                GameObject CollectableItemPickupInstant_Container_GameObject = Instantiate(Gameplay.CollectableItemPickupInstantPrefab).gameObject;
                SpawningReplacement = false;
                CollectableItemPickupInstant_Container_GameObject.SetActive(false);
                return CollectableItemPickupInstant_Container_GameObject.AddComponent<CollectableItemPickupInstant_Container>();
            }
        }

        public override void Setup(UniqueID uniqueID)
        {
            base.Setup(uniqueID);

            gameObject.GetComponent<Rigidbody2D>().gravityScale = 0;
        }
    }
}
