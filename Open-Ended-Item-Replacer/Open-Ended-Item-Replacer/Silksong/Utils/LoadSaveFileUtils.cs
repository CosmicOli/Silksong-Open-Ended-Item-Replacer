using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using static Open_Ended_Item_Replacer.Silksong.Patches.GameManager_Patches.ContinueGame;


namespace Open_Ended_Item_Replacer.Silksong.Utils
{
    internal class LoadSaveFileUtils : MonoBehaviour
    {
        public static async Task DoLoadSaveFileExtras()
        {
            //logSource.LogWarning("STARTED");

            // Wait to fade to black before loading to avoid visual stuttering and potentially assets popping into view
            await Task.Delay(2600);
            //logSource.LogWarning("WAITED FOR FADE TO BLACK");

            IEnumerable<AssetBundle> preLoadedBundles = AssetBundle.GetAllLoadedAssetBundles();

            AssetBundleRequest heartPieceInstantRequest = preLoadedBundles.Where(x => x.Contains("Assets/Prefabs/Items/Heart Piece Instant.prefab")).First().LoadAssetAsync<GameObject>("Assets/Prefabs/Items/Heart Piece Instant.prefab");

            await heartPieceInstantRequest;
            HeartPieceInstant = heartPieceInstantRequest.asset as GameObject;

            await LoadScene("Bone_East_05");

            //logSource.LogWarning("FINISHED");

            LoadGameRunPatched = true;
            GameManager gameManager = GameManager.instance;
            gameManager.StartCoroutine(gameManager.RunContinueGame(false));
        }

        // Issues: Need to pause the game loading halfway through the continue game function to allow for the correct scene to load first to establish hero controller and also for the loading screen to show

        public static async Task LoadScene(string sceneName)
        {
            // Ensure hero controller and others are correctly loaded
            AsyncOperationHandle<GameObject> handle2 = GameManager.instance.LoadGlobalPoolPrefab();
            await handle2.Task;
            Instantiate(handle2.Result);
            ObjectPool.CreateStartupPools();
            handle2 = GameManager.instance.LoadHeroPrefab();
            await handle2.Task;
            Instantiate(handle2.Result);

            //await Task.Delay(5000);

            // Should do a check first whether the asset bundle has already been loaded, in the case where you either spawn in a given room or a different mod keeps the scene loaded
            AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "aa/StandaloneWindows64/scenes_scenes_scenes/" + sceneName + ".bundle"));

            await assetBundleCreateRequest;
            AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;

            LoadSceneParameters loadSceneParameters = new LoadSceneParameters(LoadSceneMode.Additive);
            string path = assetBundle.GetAllScenePaths()[0];
            AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(path, loadSceneParameters);

            await sceneLoad;

            Scene scene = SceneManager.GetSceneByName(sceneName);

            foreach (GameObject rootGameObject in scene.GetRootGameObjects())
            {
                // pick barrel flea, see if I can unload the scene and keep the object
                //logSource.LogWarning(rootGameObject.name);
            }

            AsyncOperation sceneUnload = SceneManager.UnloadSceneAsync(scene);
            await sceneUnload;

            AsyncOperation assetBundleUnload = assetBundle.UnloadAsync(false);
            await assetBundleUnload;
        }
    }
}
