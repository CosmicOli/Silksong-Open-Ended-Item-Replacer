using HarmonyLib;
using Open_Ended_Item_Replacer.Core.Containers;
using Open_Ended_Item_Replacer.Core.Utils.Replace_Utils;
using System;
using System.Reflection;
using TeamCherry.NestedFadeGroup;
using UnityEngine;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using static Open_Ended_Item_Replacer.Core.Utils.Replace_Utils.SpawnUtils;
using Open_Ended_Item_Replacer.Silksong.Containers.CollectableItemPickup_Containers;

namespace Open_Ended_Item_Replacer.Silksong.Containers
{
    public class Costed_CollectableItemPickup_Container : CollectableItemPickup_Abstract_Container, ICosted
    {
        HeroController HCinstance = HeroController.instance;

        public void OnCancel()
        {
            SpawnUtils.OnCancel(this);

            CollectableItemPickup.IsPickupPaused = false;
            BlockNextFsmEventTransmition = false;
        }

        public void SpawnSetup()
        {
            SavedItem dummyItem = ScriptableObject.CreateInstance<FakeCollectable>();
            SetCollectableItemPickupItem(dummyItem, true);
        }

        public void InteractSetup()
        {
            SetCollectableItemPickupItem(null, true);
            logSource.LogWarning("Item set to null to force DoPickupAction to return false");

            CollectableItemPickup.IsPickupPaused = true;
            BlockNextFsmEventTransmition = true;
        }

        public Action Yes
        {
            get
            {
                return delegate
                {
                    SetCollectableItemPickupItem(ScriptableObject.CreateInstance<FakeCollectable>(), true);
                    logSource.LogWarning("Item reset to dummyItem");

                    CollectableItemPickup.IsPickupPaused = false;
                    CollectableItemPickupInstance.SetActivation(true);

                    Purchase(CollectableItemPickupInstance, Item);

                    HCinstance.OnTakenDamage -= OnCancel;
                    purchased = true;
                };
            }
        }

        public Action No
        {
            get
            {
                return delegate
                {
                    CollectableItemPickup.IsPickupPaused = false;
                    CollectableItemPickupInstance.SetActivation(false);

                    HCinstance.OnTakenDamage -= OnCancel;
                    purchased = false;
                };
            }
        }

        private static void Purchase(CollectableItemPickup collectableItemPickup, SavedItem item)
        {
            if (collectableItemPickup.OnPickup != null)
            {
                collectableItemPickup.OnPickup.Invoke();
            }

            if (collectableItemPickup.OnPickedUp != null)
            {
                collectableItemPickup.OnPickedUp.Invoke();
            }

            SpriteRenderer spriteRenderer = Traverse.Create(collectableItemPickup).Field("spriteRenderer").GetValue<SpriteRenderer>();
            GameObject extraEffects = Traverse.Create(collectableItemPickup).Field("extraEffects").GetValue<GameObject>();
            GameObject pickupEffect = Traverse.Create(collectableItemPickup).Field("pickupEffect").GetValue<GameObject>();
            bool smallGetEffect = Traverse.Create(collectableItemPickup).Field("smallGetEffect").GetValue<bool>();
            bool showPopup = Traverse.Create(collectableItemPickup).Field("showPopup").GetValue<bool>();

            if ((bool)spriteRenderer)
            {
                NestedFadeGroup component = spriteRenderer.GetComponent<NestedFadeGroup>();
                if ((bool)component)
                {
                    component.AlphaSelf = 0f;
                }
                else
                {
                    spriteRenderer.enabled = false;
                }
            }

            if ((bool)extraEffects)
            {
                extraEffects.SetActive(value: false);
            }

            if ((bool)pickupEffect)
            {
                pickupEffect.SetActive(value: true);
            }

            if (!showPopup)
            {
                CollectableItemHeroReaction.DoReaction(new Vector2(0f, -0.76f), smallGetEffect);
            }

            MethodInfo DoPickup = typeof(CollectableItemPickup).GetMethod("StopRBMovement", BindingFlags.Instance | BindingFlags.NonPublic);
            DoPickup.Invoke(collectableItemPickup, new object[] { });

            item.Get();
        }
    }
}
