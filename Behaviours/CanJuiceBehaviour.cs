using GameNetcodeStuff;
using JuicesMod.Enums;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace JuicesMod.Behaviours
{
    public class CanJuiceBehaviour : PhysicsProp
    {
        private (MethodInfo, JuiceEffectInfo)[] juiceEffects = [];

        public override void Start()
        {
            juiceEffects = GetType().GetMethods()
                .Select(method => (
                    method,
                    method.GetCustomAttribute<JuiceEffectInfo>()
                ))
                .Where((data) => data.Item2 != null)
                .ToArray();
            base.Start();
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            PlayerControllerBBehaviour player = playerHeldBy.GetComponent<PlayerControllerBBehaviour>();
            if (player != null)
            {
                if (player.CurrentJuiceEffect == JuiceEffect.None)
                {
                    StartCoroutine(Drink(player, buttonDown));
                }
                else if (playerHeldBy.IsOwner)
                {
                    HUDManager.Instance.DisplayTip("Don't drink !", "Cumulating juice effects is dangerous...");
                }
            }
        }

        public IEnumerator Drink(PlayerControllerBBehaviour player, bool buttonDown = true)
        {
            playerHeldBy.activatingItem = buttonDown;
            playerHeldBy.playerBodyAnimator.SetBool("useTZPItem", buttonDown);

            yield return new WaitForSeconds(2);

            (MethodInfo method, JuiceEffectInfo effectInfo) = juiceEffects[UnityEngine.Random.RandomRangeInt(0, juiceEffects.Length)];
            effectInfo.DisplayEffectTip(playerHeldBy);
            method.Invoke(this, [player]);

            playerHeldBy.activatingItem = false;
            playerHeldBy.playerBodyAnimator.SetBool("useTZPItem", false);
            playerHeldBy.DespawnHeldObject();
        }

        private int DebugEffectIndex = 0;

        public void DebugApplyEffect(PlayerControllerBBehaviour player)
        {
            (MethodInfo method, JuiceEffectInfo effectInfo) = juiceEffects[DebugEffectIndex];
            effectInfo.DisplayEffectTip(playerHeldBy);
            method.Invoke(this, [player]);
        }

        [JuiceEffectInfo("Healing", "Restoring full health", true)]
        public void HealingEffect(PlayerControllerBBehaviour player)
        {
            player.ApplyJuiceEffect(
                JuiceEffect.Healing,
                1,
                []
            );
        }

        [JuiceEffectInfo("Damages", "Loosing half health", false)]
        public void DamagesEffect(PlayerControllerBBehaviour player)
        {
            player.ApplyJuiceEffect(
                JuiceEffect.Damages,
                1,
                [
                    playerHeldBy.health / 2
                ]
            );
        }

        [JuiceEffectInfo("Marathoner", "More stamina for 10 minutes", true)]
        public void MarathonerEffect(PlayerControllerBBehaviour player)
        {
            playerHeldBy.sprintMeter = 1;
            player.ApplyJuiceEffect(
                JuiceEffect.Marathoner,
                10 * 60,
                [
                    playerHeldBy.sprintTime
                ]
            );
        }

        [JuiceEffectInfo("Asthmatic", "Less stamina for 10 minutes", false)]
        public void AsthmaticEffect(PlayerControllerBBehaviour player)
        {
            player.ApplyJuiceEffect(
                JuiceEffect.Asmathic,
                10 * 60,
                [
                    playerHeldBy.sprintTime
                ]
            );
        }

        [JuiceEffectInfo("Night vision", "Better vision inside factories for 5 minutes", true)]
        public void NightVisionEffect(PlayerControllerBBehaviour player)
        {
            player.ApplyJuiceEffect(
                JuiceEffect.NightVision,
                5 * 60,
                [
                    playerHeldBy.nightVision.color,
                    playerHeldBy.nightVision.intensity,
                    playerHeldBy.nightVision.range,
                    playerHeldBy.nightVision.shadowStrength,
                    playerHeldBy.nightVision.shadows,
                    playerHeldBy.nightVision.shape,
                ]
            );
        }

        [JuiceEffectInfo("Upside down", "Reverse vision for 5 minutes", false)]
        public void UpsideDownEffect(PlayerControllerBBehaviour player)
        {
            player.ApplyJuiceEffect(
                JuiceEffect.UpsideDown,
                5 * 60,
                []
            );
        }

        [JuiceEffectInfo("Electromagnetic pulse", "Disable all turrets for 2 minutes", true)]
        public void EmpEffect(PlayerControllerBBehaviour player)
        {
            player.ApplyJuiceEffect(
                JuiceEffect.EMP,
                2 * 60,
                []
            );
        }

        [JuiceEffectInfo("Signal jamming", "All turrets go crazy for 2 minutes", false)]
        public void SignalJammingEffect(PlayerControllerBBehaviour player)
        {
            player.ApplyJuiceEffect(
                JuiceEffect.SignalJamming,
                2 * 60,
                []
            );
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class JuiceEffectInfo(string name, string description, bool isPositive) : Attribute
    {
        public string Name => name;
        public string Description => description;
        public bool IsPositive => isPositive;

        public void DisplayEffectTip(PlayerControllerB player)
        {
            if (player.IsOwner)
            {
                HUDManager.Instance.DisplayTip(Name, Description, !isPositive);
            }
        }
    }
}
