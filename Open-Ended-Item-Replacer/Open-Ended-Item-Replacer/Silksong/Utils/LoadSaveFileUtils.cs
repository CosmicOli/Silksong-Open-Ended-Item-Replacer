using HutongGames.PlayMaker.Actions;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Silksong.Utils
{
    internal class LoadSaveFileUtils : MonoBehaviour
    {
        private static LoadSaveFileUtils dummyMonoBehaviour;
        private static IEnumerator coroutineIEnumerator;

        public static async Task DoLoadSaveFileExtras()
        {
            IEnumerable<AssetBundle> preLoadedBundles = AssetBundle.GetAllLoadedAssetBundles();

            HeartPieceInstant = preLoadedBundles.Where(x => x.Contains("Assets/Prefabs/Items/Heart Piece Instant.prefab")).First().LoadAsset<GameObject>("Assets/Prefabs/Items/Heart Piece Instant.prefab");


            GameObject dummyGameObject = new GameObject();
            dummyMonoBehaviour = dummyGameObject.AddComponent<LoadSaveFileUtils>();

            //coroutineIEnumerator = LoadScene("Bone_East_05");
            //dummyMonoBehaviour.StartCoroutine(coroutineIEnumerator);

            await LoadScene("Bone_East_05");

            /*PlayerData playerData = PlayerData.instance;
            string savedRespawnScene;
            if (!string.IsNullOrEmpty(playerData.tempRespawnScene))
            {
                savedRespawnScene = playerData.tempRespawnScene;
            }
            else
            {
                savedRespawnScene = playerData.respawnScene;
            }
            logSource.LogMessage(savedRespawnScene);
            coroutine = LoadScene(savedRespawnScene);
            dummyMonoBehaviour.StartCoroutine(coroutine);*/

            //logSource.LogMessage(coroutine.Current);

            /*AssetBundle Bone_East_05 = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "aa/StandaloneWindows64/scenes_scenes_scenes/Bone_East_05.bundle"));
            Scene scene = Bone_East_05.LoadAssetWithSubAssets(Bone_East_05.GetAllScenePaths()[0]);
            
            logSource.LogInfo(scene.name);
            logSource.LogInfo(scene.GetRootGameObjects().Length);
            logSource.LogInfo(scene.isLoaded);
            foreach (GameObject rootGameObject in scene.GetRootGameObjects())
            {
                // pick barrel flea, see if I can unload the scene and keep the object
                logSource.LogWarning(rootGameObject.name);
            }
            //SceneManager.LoadScene(Bone_East_05.);
            Bone_East_05.Unload(false);*/
        }

        public static async Task LoadScene(string sceneName)
        {
            // Ensure all of these are successfully loaded by other async/co before continueing
            while (HeroController.instance == null || GameManager.instance == null || GameCameras.instance == null || PlayerData.instance == null)
            {
                await Task.Delay(100);
            }

            // Should do a check first whether the asset bundle has already been loaded, in the case where you either spawn in a given room or a different mod keeps the scene loaded
            AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "aa/StandaloneWindows64/scenes_scenes_scenes/" + sceneName + ".bundle"));

            LoadSceneParameters loadSceneParameters = new LoadSceneParameters(LoadSceneMode.Additive);
            string path = assetBundle.GetAllScenePaths()[0];
            AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(path, loadSceneParameters);

            await sceneLoad;

            Scene scene = SceneManager.GetSceneByName(sceneName);
            logSource.LogInfo(scene.name);
            logSource.LogInfo(scene.GetRootGameObjects().Length);
            logSource.LogInfo(scene.isLoaded);
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

        /*public static IEnumerator LoadScene(string sceneName)
        {
            while (HeroController.instance == null)
            {
                yield return null;
            }

            // Should do a check first whether the asset bundle has already been loaded, in the case where you either spawn in a given room or a different mod keeps the scene loaded
            AssetBundle assetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "aa/StandaloneWindows64/scenes_scenes_scenes/" + sceneName + ".bundle"));

            LoadSceneParameters loadSceneParameters = new LoadSceneParameters(LoadSceneMode.Additive);
            string path = assetBundle.GetAllScenePaths()[0];
            AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(path, loadSceneParameters);

            while (!sceneLoad.isDone)
            {
                yield return null;
            }

            Scene scene = SceneManager.GetSceneByName(sceneName);
            logSource.LogInfo(scene.name);
            logSource.LogInfo(scene.GetRootGameObjects().Length);
            logSource.LogInfo(scene.isLoaded);
            foreach (GameObject rootGameObject in scene.GetRootGameObjects())
            {
                // pick barrel flea, see if I can unload the scene and keep the object
                //logSource.LogWarning(rootGameObject.name);
            }

            AsyncOperation sceneUnload = SceneManager.UnloadSceneAsync(scene);
            while (!sceneUnload.isDone)
            {
                yield return null;
            }

            assetBundle.Unload(false);
        }*/
    }
}
