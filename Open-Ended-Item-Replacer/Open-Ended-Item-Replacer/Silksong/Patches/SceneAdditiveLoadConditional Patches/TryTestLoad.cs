using HarmonyLib;
using static PlayerDataTest;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Silksong.Patches.SceneAdditiveLoadConditional_Patches
{
    [HarmonyPatch(typeof(SceneAdditiveLoadConditional), "TryTestLoad")]
    internal class TryTestLoad
    {
        private static bool Prefix(SceneAdditiveLoadConditional __instance, PlayerDataTest ___tests, QuestTest[] ___questTests, ref bool __result)
        {
            /*foreach (TestGroup testGroup in ___tests.TestGroups)
            {
                for (int i = 0; i < testGroup.Tests.Length; i++)
                {
                    logSource.LogWarning(testGroup.Tests[i].FieldName);
                    logSource.LogWarning(testGroup.Tests[i].BoolValue);
                    logSource.LogWarning(PlayerData.instance.GetBool(testGroup.Tests[i].FieldName));
                }
            }*/

            if (!Traverse.Create(__instance).Field("sceneNameToLoad").GetValue<string>().ToLowerInvariant().Contains("bone_east_08")) { return true; }

            foreach (TestGroup testGroup in ___tests.TestGroups)
            {
                for (int i = 0; i < testGroup.Tests.Length; i++)
                {
                    if (testGroup.Tests[i].FieldName == "hasBrolly")
                    {

                        if (QuestManager.GetQuest("Brolly Get").IsCompleted)
                        {
                            if (PlayerData.instance.defeatedSongGolem)
                            {
                                __result = false;
                            }
                            else
                            {
                                __result = true;
                            }
                        }

                        return false;
                    }
                }
            }

            return true;
        }
    }
}
