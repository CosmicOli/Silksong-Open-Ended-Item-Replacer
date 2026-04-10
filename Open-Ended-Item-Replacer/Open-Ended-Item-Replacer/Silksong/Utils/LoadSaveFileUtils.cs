using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Silksong.Utils
{
    internal class LoadSaveFileUtils
    {
        public static void DoLoadSaveFileExtras()
        {
            IEnumerable<AssetBundle> preLoadedBundles = AssetBundle.GetAllLoadedAssetBundles();

            HeartPieceInstant = preLoadedBundles.Where(x => x.Contains("Assets/Prefabs/Items/Heart Piece Instant.prefab")).First().LoadAsset<GameObject>("Assets/Prefabs/Items/Heart Piece Instant.prefab");

            /*string address = "Assets/Scenes/Hornet/Bone_East_05.unity";
            LoadSceneParameters loadSceneParameters = new LoadSceneParameters();
            Scene scene = SceneManager.LoadScene(address, loadSceneParameters);
            foreach (GameObject rootGameObject in scene.GetRootGameObjects())
            {
                // pick barrel flea, see if I can unload the scene and keep the object
            }*/

            
            // Should do a check first whether the asset bundle has already been loaded, in the case where you either spawn in a given room or a different mod keeps the scene loaded
            AssetBundle Bone_East_05 = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "aa/StandaloneWindows64/scenes_scenes_scenes/Bone_East_05.bundle"));
            //Bone_East_05.LoadAsset();
            Bone_East_05.Unload(false);
        }
    }
}
