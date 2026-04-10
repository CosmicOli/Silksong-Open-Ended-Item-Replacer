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
            HeartPieceInstant = AssetBundle.GetAllLoadedAssetBundles().Where(x => x.Contains("Assets/Prefabs/Items/Heart Piece Instant.prefab")).First().LoadAsset<GameObject>("Assets/Prefabs/Items/Heart Piece Instant.prefab");

            string address = "Assets/Scenes/Hornet/Bone_East_05.unity";
            LoadSceneParameters loadSceneParameters = new LoadSceneParameters();
            Scene scene = SceneManager.LoadScene(address, loadSceneParameters);
            foreach (GameObject rootGameObject in scene.GetRootGameObjects())
            {
                // pick barrel flea, see if I can unload the scene and keep the object
            }
        }
    }
}
