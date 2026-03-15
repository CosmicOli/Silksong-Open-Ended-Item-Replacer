using HarmonyLib;
using static PlayerDataTest;

namespace Open_Ended_Item_Replacer.Patches.SceneAdditiveLoadConditional_Patches
{
    [HarmonyPatch(typeof(SceneAdditiveLoadConditional), "TryTestLoad")]
    internal class TryTestLoad
    {
        private static bool Prefix(SceneAdditiveLoadConditional __instance, PlayerDataTest ___tests, QuestTest[] ___questTests, ref bool __result)
        {
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
