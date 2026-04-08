using HarmonyLib;
using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;
using UnityEngine;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Silksong.Patches.FullQuestBase_Patches
{
    [HarmonyPatch(typeof(FullQuestBase), "RewardIcon", MethodType.Getter)]
    internal class RewardIcon_Get
    {
        public static void Postfix(FullQuestBase __instance, ref Sprite __result)
        {
            // Currently assuming that the quest icons only are fetched in the same room they are given (true for all quests currently in the game as far as I know)
            // -> Some quests are available at multiple boards, but have no rewards
            // -> Basically though, if a reward can be gotten or the icon requested at multiple quest boards then make sure an association for all persistences exists in the config

            if (__instance.RewardItem == null) { return; }

            // Set up a dummy item that will send has relevant associated data to request a popup icon
            GenericSavedItem dummyPersistentItem = ScriptableObject.CreateInstance<GenericSavedItem>();
            GeneratePersistentBoolSetToItem_SameScene("Quest_Board", __instance.RewardItem.name, dummyPersistentItem);

            __result = dummyPersistentItem.GetPopupIcon();
        }
    }
}
