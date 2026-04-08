using HarmonyLib;
using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;
using System.Collections.Generic;
using UnityEngine;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Silksong.Patches.QuestBoardInteractable_Patches
{
    [HarmonyPatch(typeof(QuestBoardInteractable), "ProcessQueuedCompletions")]
    internal class ProcessQueuedCompletions
    {
        public static void Prefix(QuestBoardInteractable __instance, Queue<FullQuestBase> ___queuedCompletions)
        {
            // The point of the following is to replace the reward item while the quests are being turned in

            if (___queuedCompletions.Count == 0) { return; }

            FullQuestBase quest = ___queuedCompletions.Peek();

            if (quest.RewardItem == null) { return; }

            GenericSavedItem dummyPersistentItem = ScriptableObject.CreateInstance<GenericSavedItem>();
            GeneratePersistentBoolSetToItem_SameScene("Quest_Board", quest.RewardItem.name, dummyPersistentItem);

            Traverse.Create(quest).Field("rewardItem").SetValue(dummyPersistentItem);
        }
    }
}
