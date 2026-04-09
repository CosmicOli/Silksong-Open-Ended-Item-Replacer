using Open_Ended_Item_Replacer.Silksong.Containers.CollectableItemPickup_Containers.Bases;
using Open_Ended_Item_Replacer.Silksong.Containers.General_Bases;
using UnityEngine;

namespace Open_Ended_Item_Replacer.Silksong.Containers.CollectableItemPickup_Containers
{
    public class CollectableItemPickupInstant_Container : CollectableItemPickup_Abstract_Container, ICollisionable
    {
        public override void Setup()
        {
            base.Setup();

            gameObject.GetComponent<Rigidbody2D>().gravityScale = 0;
        }
    }
}
