using HutongGames.PlayMaker;
using UnityEngine;
using Open_Ended_Item_Replacer.Components;
using static Open_Ended_Item_Replacer.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.FsmStateActions
{
    // Progressive start and end are inclusive
    public class GetReplacedProgressiveLevel : FsmStateAction
    {
        int progressiveStart;
        int progressiveEnd;
        string[] gameObjectNames;
        string progressiveItemName;
        FsmInt storeResult;

        public GetReplacedProgressiveLevel(int progressiveStart, int progressiveEnd, string gameObjectName, string progressiveItemName, FsmInt storeResult)
        {
            gameObjectNames = new string[progressiveEnd - progressiveStart + 1];
            for (int i = 0; i < gameObjectNames.Length; i++) { gameObjectNames[i] = gameObjectName; }

            this.progressiveStart = progressiveStart;
            this.progressiveEnd = progressiveEnd;
            this.progressiveItemName = progressiveItemName;
            this.storeResult = storeResult;
        }

        public GetReplacedProgressiveLevel(int progressiveStart, int progressiveEnd, string[] gameObjectNames, string progressiveItemName, FsmInt storeResult)
        {
            this.progressiveStart = progressiveStart;
            this.progressiveEnd = progressiveEnd;
            this.gameObjectNames = gameObjectNames;
            this.progressiveItemName = progressiveItemName;
            this.storeResult = storeResult;
        }

        public override void OnEnter()
        {
            GenericSavedItem needleUpgradeItem = ScriptableObject.CreateInstance<GenericSavedItem>();
            PersistentItemData<bool> needleUpgradePersistentBoolData;

            for (int i = progressiveStart; i <= progressiveEnd; i++)
            {
                needleUpgradeItem.name = progressiveItemName + " " + i.ToString();
                needleUpgradePersistentBoolData = GeneratePersistentBoolData_SameScene(gameObjectNames[i - progressiveStart], needleUpgradeItem.name);

                if (!GetPersistentBoolFromData(needleUpgradePersistentBoolData))
                {
                    storeResult.Value = i - 1; // Minus 1 as the previous i will be the last "true" bool
                    break;
                }
            }
        }
    }
}
