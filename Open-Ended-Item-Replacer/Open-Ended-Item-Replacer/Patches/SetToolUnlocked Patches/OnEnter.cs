using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Patches.SetToolUnlocked_Patches
{
    [HarmonyPatch(typeof(SetToolUnlocked), "OnEnter")]
    internal class OnEnter
    {
        // Handles when FSMs run SetToolUnlocked
        // Should handle the vast majority of cases of being given an item from an NPC
        private static bool Prefix(SetToolUnlocked __instance)
        {
            bool flag = false;

            ToolItem item = __instance.Tool.Value as ToolItem;

            if (item?.name == "Flea Brew" && item.SavedData.IsUnlocked) // If this item is flea brew and flea brew is owned
            {
                flag = true;

                // Commented out to handle flea caravan side
                /*if (!CheckAllCaravanScenesForFleaBrew()) // If the player hasn't gotten the flea brew check yet, give it now
                {
                    GameObject fleaBrewGameObject = new GameObject(fleaBrew);

                    GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();
                    genericItem.persistentBoolItem = GeneratePersistentBoolSetToItem(fleaBrewGameObject, fleaBrew, genericItem);
                }*/

                return flag;
            }

            ReplaceFsmToolGet(__instance);

            __instance.Finish();
            return flag;
        }
    }
}
