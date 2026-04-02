using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Patches.GameManager_Patches
{
    [HarmonyPatch(typeof(GameManager), "RunContinueGame")]
    internal class RunContinueGame
    {
        public static void Postfix(GameManager __instance)
        {
            HeartPieceInstant = AssetBundle.GetAllLoadedAssetBundles().Where(x => x.Contains("Assets/Prefabs/Items/Heart Piece Instant.prefab")).First().LoadAsset<GameObject>("Assets/Prefabs/Items/Heart Piece Instant.prefab");
        }
    }
}
