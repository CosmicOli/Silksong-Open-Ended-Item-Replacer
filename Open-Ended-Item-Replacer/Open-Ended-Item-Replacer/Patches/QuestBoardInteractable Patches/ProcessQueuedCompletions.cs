using HarmonyLib;
using Open_Ended_Item_Replacer.Components;
using System.Collections.Generic;
using UnityEngine;
using static Open_Ended_Item_Replacer.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Patches.QuestBoardInteractable_Patches
{
    [HarmonyPatch(typeof(QuestBoardInteractable), "ProcessQueuedCompletions")]
    internal class ProcessQueuedCompletions
    {
        static bool handingIn = false;

        public static void Prefix(QuestBoardInteractable __instance, Queue<FullQuestBase> ___queuedCompletions)
        {
            // The point of the following is to replace the reward item while the quests are being turned in

            if (!handingIn)
            {
                handingIn = true;

                foreach (FullQuestBase quest in ___queuedCompletions)
                {
                    if (quest.RewardItem == null) { continue; } 

                    GenericSavedItem dummyPersistentItem = ScriptableObject.CreateInstance<GenericSavedItem>();
                    GeneratePersistentBoolSetToItem_SameScene("Quest_Board", quest.RewardItem.name, dummyPersistentItem);

                    Traverse.Create(quest).Field("rewardItem").SetValue(dummyPersistentItem);
                }
            }

            if (___queuedCompletions.Count == 0)
            {
                handingIn = false;
            }
        }
    }
}
