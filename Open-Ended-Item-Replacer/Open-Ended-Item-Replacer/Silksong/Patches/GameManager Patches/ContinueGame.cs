using GlobalEnums;
using GlobalSettings;
using HarmonyLib;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using static Open_Ended_Item_Replacer.Silksong.Utils.LoadSaveFileUtils;

namespace Open_Ended_Item_Replacer.Silksong.Patches.GameManager_Patches
{
    [HarmonyPatch(typeof(GameManager), "ContinueGame")]
    internal class ContinueGame
    {
        public static bool Prefix()
        {
            if (LoadGameRunPatched)
            {
                LoadGameRunPatched = false;

                // Starts the async function without awaiting as you cannot await a prefix to halt the continuation of the function from the looks of it
                DoLoadSaveFileExtras();

                if (GameManager.instance.IsMenuScene())
                {
                    GameManager.instance.StartCoroutine(MoveToLoading());
                }

                return false;
            }
            else
            {
                return true;
            }
        }

        private static IEnumerator MoveToLoading()
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
