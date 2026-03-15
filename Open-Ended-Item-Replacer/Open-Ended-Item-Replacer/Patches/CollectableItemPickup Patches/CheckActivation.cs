using HarmonyLib;
using UnityEngine;

namespace Open_Ended_Item_Replacer.Patches.CollectableItemPickup_Patches
{
    [HarmonyPatch(typeof(CollectableItemPickup), "CheckActivation")]
    internal class CheckActivation
    {
        // As some items check persistence using whether an item can be gotten anymore, this needs to be intercepted
        // A transpiler could be used instead to change the one line that is modified, but the relative difficulty compared to this method means this is what I will be doing for now
        private static bool Prefix(CollectableItemPickup __instance, bool ___activatedRead, string ___playerDataBool, PersistentBoolItem ___persistent, SavedItem ___item)
        {
            if (___activatedRead) // This first part resets items that can be continuously gotten that have already been picked up; this doesn't need changing for now
            {
                if (string.IsNullOrEmpty(___playerDataBool) && ___persistent == null && (___item == null || (!___item.IsUnique && ___item.CanGetMore())))
                {
                    ___activatedRead = false;
                    return false; // This stops the original code running
                }
            }
            else // This second part usually looks like "activatedRead = (bool)item && !item.CanGetMore();", but has been changed to ignore CanGetMore()
            {
                bool flag = false;

                for (int i = 0; i < __instance.transform.childCount; i++)
                {
                    GameObject replacementObject = __instance.transform.GetChild(i).gameObject;
                    if (replacementObject == null) { continue; } // Not sure if this is necessary

                    if (replacementObject.name.Contains(__instance.gameObject.name) && replacementObject.GetComponent<CollectableItemPickup>() != null)
                    {
                        flag = true;
                    }
                }

                if (flag)
                {
                    ___activatedRead = false;
                }
                else // If the item was never replaced (from either config or other reasons) the original code should run
                {
                    ___activatedRead = (bool)___item && !___item.CanGetMore();
                }
            }

            // The rest of this is unchanged
            if (___activatedRead)
            {
                if (__instance.OnPickedUp != null)
                {
                    __instance.OnPickedUp.Invoke();
                }

                if (__instance.OnPreviouslyPickedUp != null)
                {
                    __instance.OnPreviouslyPickedUp.Invoke();
                }

                __instance.gameObject.SetActive(value: false);
            }

            // This stops the original code running
            return false;
        }
    }
}
