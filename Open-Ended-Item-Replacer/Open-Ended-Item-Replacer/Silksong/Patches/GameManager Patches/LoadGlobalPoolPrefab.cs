using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Open_Ended_Item_Replacer.Silksong.Utils.LoadSaveFileUtils;

namespace Open_Ended_Item_Replacer.Silksong.Patches.GameManager_Patches
{
    [HarmonyPatch(typeof(GameManager), "LoadGlobalPoolPrefab")]
    internal class LoadGlobalPoolPrefab
    {
        public static bool LoadGlobalPoolPrefabPatched = true;

        /*public static AsyncOperationHandle<GameObject> Postfix(AsyncOperationHandle<GameObject> __result)
        {
            /*if (GameManager.instance.IsLoadingSceneTransition)
            {
                AsyncOperationHandle<GameObject> replacement = new AsyncOperationHandle<GameObject>();
                replacement.

                return DoLoadSaveFileExtras();
            }
            else
            {
                return __result;
            }*/
        //}
    }
}
