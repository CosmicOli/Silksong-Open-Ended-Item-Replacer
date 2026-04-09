using GlobalSettings;
using Open_Ended_Item_Replacer.Silksong.Containers.CollectableItemPickup_Containers;
using System.Linq;
using UnityEngine;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Silksong.Utils
{
    internal class LoadSaveFileUtils
    {
        public static void DoLoadSaveFileExtras()
        {
            HeartPieceInstant = AssetBundle.GetAllLoadedAssetBundles().Where(x => x.Contains("Assets/Prefabs/Items/Heart Piece Instant.prefab")).First().LoadAsset<GameObject>("Assets/Prefabs/Items/Heart Piece Instant.prefab");

            // Make asset bundle pointing to scene
            // Grab the scene data and pull a mask shard

            //GameObject DefaultCollisionContainer_GameObject = Object.Instantiate(Gameplay.CollectableItemPickupPrefab).gameObject;
            //DefaultCollisionContainer_GameObject.SetActive(false);
            //DefaultCollisionContainer = DefaultCollisionContainer_GameObject.AddComponent<CollectableItemPickupInstant_Container>();

            //GameObject DefaultCostedContainer_GameObject = Object.Instantiate(Gameplay.CollectableItemPickupPrefab).gameObject;
            //DefaultCostedContainer_GameObject.SetActive(false);
            //DefaultCostedContainer = DefaultCostedContainer_GameObject.gameObject.AddComponent<Costed_CollectableItemPickup_Container>();
        }
    }
}
