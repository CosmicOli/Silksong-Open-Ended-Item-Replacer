using HarmonyLib;
using UnityEngine;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using Open_Ended_Item_Replacer.Components.Grant_Components;

namespace Open_Ended_Item_Replacer.Patches.NailSlash_Patches
{
    [HarmonyPatch(typeof(NailSlash), "StartSlash")]
    internal class StartSlash
    {
        private static bool logging = true;
        public static Transform testTransform;
        private static void Postfix(NailSlash __instance)
        {
            if (logging)
            {
                try
                {
                    logSource.LogWarning(testTransform.name);
                    logSource.LogMessage(testTransform.GetComponent<Rigidbody2D>().gravityScale);
                    logSource.LogMessage(testTransform.position);
                    logSource.LogMessage(testTransform.parent.name);
                    logSource.LogMessage(testTransform.parent.GetComponent<Rigidbody2D>().gravityScale);
                    logSource.LogMessage(testTransform.parent.position);
                }
                catch
                {
                    logSource.LogWarning("No test transform found");
                }
            }

            MaskShardGranter.Grant_MaskShard();

            //logSource.LogMessage(PlayerData.instance.HasMelodyArchitect);
            //logSource.LogMessage(PlayerData.instance.HasMelodyConductor);
            //logSource.LogMessage(PlayerData.instance.HasMelodyLibrarian);

            //logSource.LogMessage(testTransform.position);
            //logSource.LogMessage(testTransform.gameObject.activeInHierarchy);

            /*var quests = QuestManager.GetAllQuests();

            foreach (var quest in quests)
            {
                logSource.LogMessage(quest.DisplayName);
            }*/

            //logSource.LogMessage("Slash Postfix");

            //HeroController heroControl = GameManager.instance.hero_ctrl;

            //logSource.LogInfo(test.GetComponent<Rigidbody2D>().gravityScale);
            //logSource.LogInfo(testAction.gravityScale);

            /*UniqueID uniqueID = new UniqueID("pickupName", "sceneName");
            spawningReplacementCollectableItemPickup = true;
            SpawnGenericInteractablePickup(uniqueID, null, heroControl.transform, new Vector3(0, 0, 0));
            spawningReplacementCollectableItemPickup = false;*/
        }
    }
}
