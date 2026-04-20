using GlobalSettings;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Open_Ended_Item_Replacer.Core.Components.Replacement_Components;
using Open_Ended_Item_Replacer.Core.Containers;
using Open_Ended_Item_Replacer.Silksong.Containers.CollectableItemPickup_Containers.Bases;
using Open_Ended_Item_Replacer.Silksong.Containers.Flea_Containers.Bases;
using Open_Ended_Item_Replacer.Silksong.Containers.General_Bases;
using Open_Ended_Item_Replacer.Silksong.FsmStateActions;
using Open_Ended_Item_Replacer.Silksong.Utils.Replace_Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TeamCherry.NestedFadeGroup;
using UnityEngine;
using static Open_Ended_Item_Replacer.Open_Ended_Item_Replacer;
using static Open_Ended_Item_Replacer.Silksong.Utils.Replace_Utils.SpawnUtils;
using static UnityEngine.Object;

namespace Open_Ended_Item_Replacer.Silksong.Containers.Flea_Containers
{
    public class Flea_Barrel_Container : Flea_Abstract_Container
    {
        public static Flea_Barrel_Container Prefab
        {
            get
            {
                SpawningReplacement = true;
                GameObject CollectableItemPickup_Container_GameObject = Instantiate(Flea_Barrel);
                SpawningReplacement = false;
                CollectableItemPickup_Container_GameObject.SetActive(false);
                return CollectableItemPickup_Container_GameObject.AddComponent<Flea_Barrel_Container>();
            }
        }

        public override void Setup(UniqueID uniqueID)
        {
            //logSource.LogInfo(Traverse.Create(gameObject.GetComponent<tk2dSpriteInitialiser>()).Field("sprites").GetValue<List<tk2dSprite>>().Count);

            //logSource.LogInfo(Traverse.Create(gameObject.GetComponent<tk2dSpriteInitialiser>()).Field("sprites").GetValue<List<tk2dSprite>>().First().enabled);

            //logSource.LogInfo(Traverse.Create(gameObject.GetComponent<tk2dSpriteInitialiser>()).Field("sprites").GetValue<List<tk2dSprite>>().First().spriteId);

            //logSource.LogInfo(Traverse.Create(gameObject.GetComponent<tk2dSpriteInitialiser>()).Field("sprites").GetValue<List<tk2dSprite>>().First().Collection);

            //logSource.LogInfo(Traverse.Create(gameObject.GetComponent<tk2dSpriteInitialiser>()).Field("sprites").GetValue<List<tk2dSprite>>().First().CurrentSprite);
            //logSource.LogInfo(Traverse.Create(gameObject.GetComponent<tk2dSpriteInitialiser>()).Field("sprites").GetValue<List<tk2dSprite>>().First().CurrentSprite.name);

            //Traverse.Create(gameObject.GetComponent<tk2dSpriteInitialiser>()).Field("sprites").GetValue<List<tk2dSprite>>().Add(gameObject.GetComponent<tk2dSprite>());
            //logSource.LogInfo(Traverse.Create(gameObject.GetComponent<tk2dSpriteInitialiser>()).Field("sprites").GetValue<List<tk2dSprite>>().Count);

            base.Setup(uniqueID);

            gameObject.GetComponents<PlayMakerFSM>().First().Fsm.GetState("Init").Actions[4].Enabled = false;

            FsmOwnerDefault newGameObject = new FsmOwnerDefault();
            newGameObject.OwnerOption = OwnerDefaultOption.SpecifyGameObject;
            newGameObject.GameObject = this.gameObject;

            (GetComponent<PlayMakerFSM>().Fsm.GetState("To Idle").Actions[1] as Tk2dPlayAnimation).gameObject = newGameObject;
            (GetComponent<PlayMakerFSM>().Fsm.GetState("To Alert").Actions[1] as Tk2dPlayAnimation).gameObject = newGameObject;

            tk2dSpriteAnimator animator = GetComponent<tk2dSpriteAnimator>();

            logSource.LogWarning(animator.Library);

            //tk2dSpriteAnimationClip clip = animator.GetClipByName("Barrel");
            //logSource.LogInfo(clip);
            //logSource.LogInfo(clip.name);
            //logSource.LogInfo(clip.fps);
            //logSource.LogInfo(clip.frames);
            //GetComponent<tk2dSpriteAnimator>().Play(clip);

            //GetComponent<PlayMakerFSM>().Fsm.GetState("To Idle").Actions[2] = new TestAction();
        }

        public override PlayMakerFSM FleaRescueActivation
        {
            get
            {
                //logSource.LogMessage((gameObject.GetComponents<PlayMakerFSM>().First().Fsm.GetState("Activate Flea").Actions[0] as ActivateGameObject).gameObject.GameObject.Value.scene.name);
                //logSource.LogMessage((gameObject.GetComponents<PlayMakerFSM>().First().Fsm.GetState("Activate Flea").Actions[0] as ActivateGameObject).gameObject.GameObject.Value.GetComponents<PlayMakerFSM>().First().Fsm.States.Length);

                return (gameObject.GetComponents<PlayMakerFSM>().First().Fsm.GetState("Activate Flea").Actions[0] as ActivateGameObject).gameObject.GameObject.Value.GetComponents<PlayMakerFSM>().First();
            }
        }
    }
}
