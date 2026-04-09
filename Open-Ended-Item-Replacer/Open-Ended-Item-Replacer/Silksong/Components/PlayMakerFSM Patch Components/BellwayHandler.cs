using HutongGames.PlayMaker;
using UnityEngine;
using static Open_Ended_Item_Replacer.Silksong.Utils.Replace_Utils.SpawnUtils;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;
using Open_Ended_Item_Replacer.Silksong.Containers.CollectableItemPickup_Containers;

namespace Open_Ended_Item_Replacer.Silksong.Components.PlayMakerFSM_Patch_Components
{
    internal class BellwayHandler
    {
        public static void Handle_Bellway(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Unlock Behaviour" && __instance.gameObject?.name == "Bellway Toll Machine")
            {
                FsmString playerDataBool = __instance.Fsm.GetFsmString("Pickup PlayerData Bool");

                // If the bellway hasn't been granted, the option to buy it should not be available as the replacement for the purchase is handled independantly
                __instance.gameObject.SetActive(PlayerData.instance.GetBool(playerDataBool.Value));

                UniqueID uniqueID = new UniqueID(__instance.gameObject, playerDataBool.Value);

                Vector2 offset = new Vector2(2.5f, 0); // 2.5 to make sure they mostly don't interfere with the station itself
                int cost;

                CostReference costReference = __instance.Fsm.GetFsmObject("Cost Reference").Value as CostReference;
                if (costReference != null)
                {
                    cost = costReference.Value;
                }
                else
                {
                    logSource.LogError("Cost reference not assigned!");
                    return;
                }

                SpawnGenericCostedPickup(DefaultCostedContainer, uniqueID, __instance.transform, offset, CurrencyType.Money, cost);
            }
        }
    }
}
