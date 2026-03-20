using HutongGames.PlayMaker;
using UnityEngine;
using static Open_Ended_Item_Replacer.Utils.Replace_Utils.SpawnUtils;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Components.PlayMakerFSM_Patch_Components
{
    internal class VentricaHandler
    {
        public static void HandleVentrica(PlayMakerFSM __instance)
        {
            if (__instance.Fsm.Name == "Unlock Behaviour" && __instance.gameObject?.name == "tube_toll_machine")
            {
                FsmString playerDataBool = __instance.Fsm.GetFsmString("Unlocked PD Bool");

                // If the ventrica hasn't been granted, the option to buy it should not be available as the replacement for the purchase is handled independantly
                __instance.gameObject.SetActive(PlayerData.instance.GetBool(playerDataBool.Value));

                UniqueID uniqueID = new UniqueID(__instance.gameObject, playerDataBool.Value);

                Vector2 offset = new Vector2(2.5f, 3f); // For some reason the ventrica objects are under the floor so the +3 fixes that in the replacement
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

                SpawnGenericCostedPickup(uniqueID, __instance.transform, offset, CurrencyType.Money, cost);
            }
        }
    }
}
