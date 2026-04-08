using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using Open_Ended_Item_Replacer.Components;
using UnityEngine;
using static Open_Ended_Item_Replacer.Silksong.Utils.PersistenceUtils;

namespace Open_Ended_Item_Replacer.Silksong.Patches.CreateObject_Patches
{
    [HarmonyPatch(typeof(CreateObject), "OnEnter")]
    internal class OnEnter
    {
        private static bool Prefix(CreateObject __instance)
        {
            if (__instance.gameObject.Value == null) { return true; }

            string loweredName = __instance.gameObject.Value.name.ToLowerInvariant();

            if (loweredName.Contains("silk spool") || loweredName.Contains("heart piece"))
            {
                string itemName;

                if (loweredName.Contains("silk spool"))
                {
                    itemName = "Silk Spool";
                }
                else
                {
                    itemName = "Heart Piece";
                }

                GenericSavedItem genericItem = ScriptableObject.CreateInstance<GenericSavedItem>();

                GeneratePersistentBoolSetToItem(__instance.Fsm.GameObject, itemName, genericItem);

                // Handles persistence set by new item
                if (!GetPersistentBoolFromData(genericItem.PersistentBoolItem.ItemData))
                {
                    genericItem.Get();
                }

                foreach (var transition in __instance.State.Transitions)
                {
                    if (transition.EventName.Contains("SILK SPOOL UI END") || transition.EventName.Contains("HEART PIECE UI END"))
                    {
                        __instance.Fsm.SetState(transition.ToState);
                    }
                }

                // I believe this should be intended
                HeroController.instance.RegainControl(true);
                HeroController.instance.StartAnimationControl();

                return false;
            }

            return true;

            /*if (loweredName.Contains("ui msg crest evolve"))
            {
                PlayMakerFSM playMakerFsm = __instance.gameObject?.Value?.transform?.GetComponent<PlayMakerFSM>();
                if (playMakerFsm == null) { return; }

                HandleUiMsgCrestEvolve(playMakerFsm);
            }*/
        }
    }
}
