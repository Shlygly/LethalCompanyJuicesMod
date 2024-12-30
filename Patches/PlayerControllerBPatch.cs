using GameNetcodeStuff;
using HarmonyLib;
using JuicesMod.Behaviours;

namespace JuicesMod.Patches
{
    public class PlayerControllerBPatch
    {
        [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Start))]
        [HarmonyPostfix]
        private static void PlayerControllerB_Start(PlayerControllerB __instance)
        {
            if (__instance.GetComponent<PlayerControllerBBehaviour>() == null)
            {
                __instance.gameObject.AddComponent<PlayerControllerBBehaviour>();
            }
        }
    }
}
