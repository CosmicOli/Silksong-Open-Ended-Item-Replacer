using HarmonyLib;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using static Open_Ended_Item_Replacer.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Patches.GameManager_Patches
{
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.TimePasses))]
    internal class TimePasses
    {
        public static void Postfix(GameManager __instance)
        {
            PlayerData playerData = __instance.playerData;
            string sceneNameString = __instance.GetSceneNameString();

            logSource.LogInfo("Time Passes");

            if (sceneNameString != "Room_Pinstress")
            {
                if (playerData.blackThreadWorld)
                {
                    if (GetPersistentBoolFromData("Room_Pinstress", GeneratePersistentBoolData_SameScene("Needle Strike", "Needle Strike").ID))
                    {
                        playerData.pinstressQuestReady = true;
                    }
                    else
                    {
                        playerData.pinstressQuestReady = false;
                    }
                }
            }
        }
    }
}
