using GlobalSettings;
using HarmonyLib;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Open_Ended_Item_Replacer.FsmStateActions
{
    public class AllowPickup : FsmStateAction
    {
        CollectableItemPickup pickup;

        public AllowPickup(CollectableItemPickup pickup)
        {
            this.pickup = pickup;
        }

        public override void OnEnter()
        {
            pickup.transform.GetComponent<Collider2D>().enabled = true;
            Traverse.Create(pickup).Field("canPickupTime").SetValue((double)0);
            Traverse.Create(pickup).Field("canPickupDelay").SetValue(Traverse.Create(Gameplay.CollectableItemPickupPrefab).Field("canPickupDelay").GetValue<float>());

            Active = false;
            Finished = true;
            Finish();
        }
    }
}
