using GlobalEnums;
using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using static CutsceneHelper;
using static GameManager;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;


namespace Open_Ended_Item_Replacer.Silksong.Utils
{
    public class LoadSaveFileUtils : MonoBehaviour
    {
        public static bool HandleLoadSave(bool startNewSave)
        {
            if (LoadGameRunPatched)
            {
                LoadGameRunPatched = false;

                // Starts the async function without awaiting as you cannot await a prefix to halt the continuation of the function from the looks of it
                #pragma warning disable CS4014
                DoLoadSaveFileExtras(startNewSave);
                #pragma warning restore CS4014

                if (GameManager.instance.IsMenuScene())
                {
                    // NEED THIS TO WAIT A SEC
                    GameManager.instance.StartCoroutine(MoveToLoading());
                }

                return false;
            }
            else
            {
                return true;
            }
        }

        public static async Task DoLoadSaveFileExtras(bool startNewSave)
        {
            //logSource.LogWarning("STARTED");

            // Wait to fade to black before loading to avoid visual stuttering and potentially assets popping into view
            await Task.Delay(2600);
            //logSource.LogWarning("WAITED FOR FADE TO BLACK");

            IEnumerable<AssetBundle> preLoadedBundles = AssetBundle.GetAllLoadedAssetBundles();

            AssetBundleRequest heartPieceInstantRequest = preLoadedBundles.Where(x => x.Contains("Assets/Prefabs/Items/Heart Piece Instant.prefab")).First().LoadAssetAsync<GameObject>("Assets/Prefabs/Items/Heart Piece Instant.prefab");

            await heartPieceInstantRequest;
            HeartPieceInstant = heartPieceInstantRequest.asset as GameObject;

            Flea_Barrel = await LoadSceneGameObject("Bone_East_05", "Flea Rescue Barrel");

            //logSource.LogWarning("FINISHED");

            LoadGameRunPatched = true; // Set up the next time the game loads to run patched; set before running the following coroutine as it doesn't effect it but it may cause issues with patching CustomSceneManager and GradeMarker if this is not set prior
            GameManager gameManager = GameManager.instance;

            if (startNewSave)
            {
                gameManager.StartCoroutine(gameManager.RunStartNewGame());

            }
            else
            {
                gameManager.StartCoroutine(gameManager.RunContinueGame(false));
            }
        }

        public static async Task<GameObject> LoadSceneGameObject(string sceneName, string gameObjectName)
        {
            // Ensure hero controller is loaded correctly temporarily to avoid errors
            AsyncOperationHandle<GameObject> handle2 = GameManager.instance.LoadHeroPrefab();
            await handle2.Task;
            GameObject dummyHero = Instantiate(handle2.Result);

            // Should do a check first whether the asset bundle has already been loaded, in the case where you either spawn in a given room or a different mod keeps the scene loaded
            /*AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "aa/StandaloneWindows64/scenes_scenes_scenes/" + sceneName + ".bundle"));

            await assetBundleCreateRequest;
            AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;

            //GameManager.instance.LoadScene(sceneName);

            LoadSceneParameters loadSceneParameters = new LoadSceneParameters(LoadSceneMode.Additive);
            string path = assetBundle.GetAllScenePaths()[0];
            AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(path, loadSceneParameters);

            await sceneLoad;*/

            AsyncOperationHandle<SceneInstance> addressablesLoadScene = Addressables.LoadSceneAsync("Scenes/" + sceneName, LoadSceneMode.Additive);
            //addressablesLoadScene.ReleaseHandleOnCompletion();

            SceneLoad lastSceneLoad = new SceneLoad(addressablesLoadScene, new SceneLoadInfo
            {
                SceneName = sceneName
            });

            //SceneLoad LastSceneLoad = GameManager.instance.LastSceneLoad;

            //GameManager.instance.LoadScene(sceneName);

            //logSource.LogInfo(path);

            // It seems to be the case that this is *either* loading to early such that the Library isn't available, or the Library is only available on a non additive load which I have been so far unable to test independantly as it seems to hang every time
            // The one other situation (although unlikely imo) is that the surrounding seemingly unrelated code in LoadScene(string destScene) is somehow loading the Library
            // Not sure how to test this, because attempting to load assets later would mean needing to allow the correct scene to load first but the game not "start" until the additive load happens
            // -> Perhaps worth it for testing, as if this is the case then whatever is done to make the Library load correctly can be handled prior

            // Running both single and additive in post does not fix the issue in either case :(
            // That being said, loading it in as single has some problems as it doesn't unload the current scene

            // Using GameManager.instance.LoadScene(); DOES fix it

            SceneInstance sceneInstance = await addressablesLoadScene.Task;
            Scene scene = sceneInstance.Scene;

            //Scene scene = SceneManager.GetSceneByName(sceneName);

            //Scene scene = (await a.Task).Scene;

            logSource.LogInfo(scene.name);
            logSource.LogInfo(scene.GetRootGameObjects().Length);

            GameObject targetObject = null;
            tk2dSpriteAnimator[] animators = null;
            foreach (GameObject rootGameObject in scene.GetRootGameObjects())
            {
                //logSource.LogInfo(rootGameObject.name);

                if (rootGameObject.name.Equals(gameObjectName))
                {
                    targetObject = Instantiate(rootGameObject);
                    targetObject.SetActive(false);
                    DontDestroyOnLoad(targetObject);

                    animators = targetObject.GetComponentsInChildren<tk2dSpriteAnimator>();
                    foreach (tk2dSpriteAnimator animator in animators)
                    {
                        GameObject replacementLibrary = Instantiate(animator.Library.gameObject);
                        //logSource.LogWarning(replacementLibrary);
                        DontDestroyOnLoad(replacementLibrary);
                        animator.Library = replacementLibrary.GetComponent<tk2dSpriteAnimation>();

                        foreach (var clip in animator.Library.clips)
                        {
                            //logSource.LogInfo(clip.name);
                        }
                    }
                    //logSource.LogWarning(targetObject.GetComponent<tk2dSpriteAnimator>().Library);
                }
            }

            //AsyncOperation sceneUnload = SceneManager.UnloadSceneAsync(scene);
            //await sceneUnload;

            addressablesLoadScene.Release();

            foreach (tk2dSpriteAnimator animator in animators)
            {
                foreach (var clip in animator.Library.clips)
                {
                    //logSource.LogInfo(clip.name);
                }
            }

            //AsyncOperation assetBundleUnload = assetBundle.UnloadAsync(true);
            //await assetBundleUnload;

            //GameManager.instance.BeginSceneTransition(LastSceneLoad.SceneLoadInfo);

            Destroy(dummyHero);
            GameManager.instance.UnloadHeroPrefab();

            return targetObject;
        }

        public static IEnumerator MoveToLoading()
        {
            GameManager gameManager = GameManager.instance;

            Platform.Current.SetSceneLoadState(isInProgress: true, isHighPriority: true);
            Traverse.Create(gameManager).Field("isLoading").SetValue(true);
            gameManager.SetState(GameState.LOADING);

            gameManager.ui.FadeScreenOut();
            gameManager.noMusicSnapshot.TransitionToSafe(2f);
            gameManager.noAtmosSnapshot.TransitionToSafe(2f);
            yield return new WaitForSeconds(1f);
            gameManager.ui.FadeOutBlackThreadLoop();
            yield return new WaitForSeconds(1.6f);
            gameManager.AudioManager.ApplyMusicCue(gameManager.noMusicCue, 0f, 0f, applySnapshot: false);
            gameManager.ui.MakeMenuLean();
        }
    }
}
