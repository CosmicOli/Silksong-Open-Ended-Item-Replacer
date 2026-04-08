using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Utils
{
    internal class LoadSaveFileUtils
    {
        public static void DoLoadSaveFileExtras()
        {
            HeartPieceInstant = AssetBundle.GetAllLoadedAssetBundles().Where(x => x.Contains("Assets/Prefabs/Items/Heart Piece Instant.prefab")).First().LoadAsset<GameObject>("Assets/Prefabs/Items/Heart Piece Instant.prefab");

            // Make asset bundle pointing to scene
            // Grab the scene data and pull a mask shard
        }
    }
}
