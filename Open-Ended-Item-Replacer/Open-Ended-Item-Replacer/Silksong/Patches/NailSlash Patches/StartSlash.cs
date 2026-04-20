using GlobalSettings;
using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using Open_Ended_Item_Replacer.Silksong.Components.Grant_Components;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Open_Ended_Item_Replacer.Silksong.Utils.LoadSaveFileUtils;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;

namespace Open_Ended_Item_Replacer.Silksong.Patches.NailSlash_Patches
{
    [HarmonyPatch(typeof(NailSlash), "StartSlash")]
    internal class StartSlash
    {
        private static bool logging = true;
        public static Transform testTransform;
        static bool test = true;
        private static async void Postfix(NailSlash __instance)
        {
            if (logging)
            {
                try
                {
                    logSource.LogWarning(testTransform.name);
                    logSource.LogMessage(testTransform.GetComponent<Rigidbody2D>().gravityScale);
                    logSource.LogMessage(testTransform.position);
                    logSource.LogMessage(testTransform.parent.name);
                    logSource.LogMessage(testTransform.parent.GetComponent<Rigidbody2D>().gravityScale);
                    logSource.LogMessage(testTransform.parent.position);
                }
                catch
                {
                    if (!testTransform)
                    {
                        logSource.LogWarning("No test transform found");   
                    }
                }
            }

            //logSource.LogInfo(Flea_Barrel);

            if (test)
            {
                test = false;
                //GameManager.instance.StartCoroutine(GameManager.instance.LoadSceneAdditive("Bone_East_05"));
                //Flea_Barrel = await LoadSceneGameObject("Bone_East_05", "Flea Rescue Barrel");
                //testTransform = SpawnGenericPickup(BarrelFleaContainer, new Core.Components.Replacement_Components.UniqueID("test", "test"), HeroController.instance.transform, new Vector3(0, 10, 0));
            }

            //foreach (Component component in testTransform.GetComponents<Component>())
            {
                //logSource.LogInfo(component);
            }
            //logSource.LogInfo(testTransform.GetComponent<PlayMakerFSM>().Fsm.ActiveStateName);

            // COMPARE THIS TO THE REAL BARREL
            //logSource.LogInfo(Traverse.Create(testTransform.GetComponent<tk2dSpriteInitialiser>()).Field("sprites").GetValue<List<tk2dSprite>>().First().spriteId);
            //logSource.LogInfo(Traverse.Create(testTransform.GetComponent<tk2dSpriteInitialiser>()).Field("sprites").GetValue<List<tk2dSprite>>().First().CurrentSprite.name);
            //logSource.LogInfo(testTransform.GetComponent<tk2dSprite>().CurrentSprite);
            //logSource.LogInfo(Traverse.Create(testTransform.GetComponent<tk2dSpriteInitialiser>()).Field("sprites").GetValue<List<tk2dSprite>>().First().CurrentSprite.Valid);
            //logSource.LogInfo(testTransform.GetComponent<tk2dSpriteAnimator>().Playing);

            // Caravan Troupe Flea Anim (tk2dSpriteAnimation)

            //SceneManager.LoadScene("Bone_East_05");

            tk2dSpriteAnimation animation = SceneManager.GetActiveScene().GetRootGameObjects().Where(x => x.name.ToLowerInvariant().Contains("flea rescue barrel")).First().GetComponent<tk2dSpriteAnimator>().Library;
            logSource.LogWarning(animation);
            if (animation != null)
            {
                logSource.LogWarning(animation.gameObject.name);
                //logSource.LogWarning(animation.gameObject.scene.name);
                //logSource.LogInfo(GameObject.Find(animation.gameObject.name));
            }

            /*foreach (var v in AssetBundle.GetAllLoadedAssetBundles())
            {
                foreach (var v2 in v.GetAllAssetNames())
                {
                    logSource.LogMessage(v2);
                }
            }*/

            //logSource.LogInfo(SceneManager.GetActiveScene().path);

            //MaskShardGranter.Grant_MaskShard();

            //logSource.LogMessage(PlayerData.instance.HasMelodyArchitect);
            //logSource.LogMessage(PlayerData.instance.HasMelodyConductor);
            //logSource.LogMessage(PlayerData.instance.HasMelodyLibrarian);

            //logSource.LogMessage(testTransform.position);
            //logSource.LogMessage(testTransform.gameObject.activeInHierarchy);

            /*var quests = QuestManager.GetAllQuests();

            foreach (var quest in quests)
            {
                logSource.LogMessage(quest.DisplayName);
            }*/

            //logSource.LogMessage("Slash Postfix");

            //HeroController heroControl = GameManager.instance.hero_ctrl;

            //logSource.LogInfo(test.GetComponent<Rigidbody2D>().gravityScale);
            //logSource.LogInfo(testAction.gravityScale);

            /*UniqueID uniqueID = new UniqueID("pickupName", "sceneName");
            spawningReplacementCollectableItemPickup = true;
            SpawnGenericInteractablePickup(uniqueID, null, heroControl.transform, new Vector3(0, 0, 0));
            spawningReplacementCollectableItemPickup = false;*/
        }
    }
}
