using GameNetcodeStuff;
using JuicesMod.Enums;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace JuicesMod.Behaviours
{
    public class PlayerControllerBBehaviour : NetworkBehaviour
    {
        const float TRANSITION_DURATION = 1;

        private JuiceEffect currentJuiceEffect;
        private JuiceEffect lastJuiceEffect;
        private float transitionDelta;

        public JuiceEffect CurrentJuiceEffect
        {
            get => currentJuiceEffect;
            private set
            {
                lastJuiceEffect = currentJuiceEffect;
                currentJuiceEffect = value;
                transitionDelta = TRANSITION_DURATION;
            }
        }

        public object[] Parameters { get; set; }

        public PlayerControllerBBehaviour()
        {
            CurrentJuiceEffect = JuiceEffect.None;
            lastJuiceEffect = JuiceEffect.None;
            transitionDelta = 0;
            Parameters = [];
        }

        public void ApplyJuiceEffect(JuiceEffect effect, int duration, object[] parameters)
        {
            StartCoroutine(applyJuiceEffectCoroutine(effect, duration, parameters));
        }

        private IEnumerator applyJuiceEffectCoroutine(JuiceEffect effect, int duration, object[] parameters)
        {
            Parameters = parameters;
            CurrentJuiceEffect = effect;
            yield return new WaitForSecondsRealtime(duration);
            CurrentJuiceEffect = JuiceEffect.None;
        }

        public void Update()
        {
            PlayerControllerB player = GetComponent<PlayerControllerB>();

            if (currentJuiceEffect != lastJuiceEffect)
            {
                // Healing effect
                if (currentJuiceEffect == JuiceEffect.Healing)
                {
                    player.health = Mathf.CeilToInt(Mathf.Lerp(player.health, 100, 0.5f));
                }
                // Damages effect
                if (currentJuiceEffect == JuiceEffect.Damages)
                {
                    player.health = Mathf.FloorToInt(Mathf.Lerp(player.health, (int)Parameters[0], 0.5f));
                }
                // Marathoner effect
                if ((currentJuiceEffect | lastJuiceEffect).HasFlag(JuiceEffect.Marathoner))
                {
                    float sprintTimeTarget = (float)Parameters[0] * (currentJuiceEffect == JuiceEffect.Marathoner ? 10 : 1);
                    player.sprintTime = Mathf.Lerp(player.sprintTime, sprintTimeTarget, 0.5f);
                }
                // Asthmathic effect
                if ((currentJuiceEffect | lastJuiceEffect).HasFlag(JuiceEffect.Asmathic))
                {
                    float sprintTimeTarget = (float)Parameters[0] * (currentJuiceEffect == JuiceEffect.Asmathic ? 0.1f : 1);
                    player.sprintTime = Mathf.Lerp(player.sprintTime, sprintTimeTarget, 0.5f);
                }
                // Night vision effect
                if ((currentJuiceEffect | lastJuiceEffect).HasFlag(JuiceEffect.NightVision))
                {
                    bool activating = currentJuiceEffect == JuiceEffect.NightVision;

                    Light newlNightVision = player.nightVision;

                    newlNightVision.color = Color.Lerp(newlNightVision.color, activating ? new Color(0, 1, 0, 1) : (Color)Parameters[0], 0.5f);
                    newlNightVision.intensity = Mathf.Lerp(newlNightVision.intensity, activating ? 10000 : (float)Parameters[1], 0.5f);
                    newlNightVision.range = Mathf.Lerp(newlNightVision.range, activating ? 100000 : (float)Parameters[2], 0.5f);
                    newlNightVision.shadowStrength = Mathf.Lerp(newlNightVision.shadowStrength, activating ? 0 : (float)Parameters[3], 0.5f);
                    newlNightVision.shadows = activating ? LightShadows.None : (LightShadows)Parameters[4];
                    newlNightVision.shape = activating ? LightShape.Box : (LightShape)Parameters[5];

                    player.nightVision = newlNightVision;
                }
                // Upside down effect
                if ((currentJuiceEffect | lastJuiceEffect).HasFlag(JuiceEffect.UpsideDown))
                {
                    int zTarget = currentJuiceEffect == JuiceEffect.UpsideDown ? 180 : 0;
                    Vector3 cameraRotation = player.gameplayCamera.transform.rotation.eulerAngles;
                    cameraRotation.z = Mathf.Lerp(((cameraRotation.z + (180 - zTarget)) % 360) - (180 - zTarget), zTarget, 0.5f);
                    player.gameplayCamera.transform.rotation = Quaternion.Euler(cameraRotation);
                }
                // EMP effect
                if ((currentJuiceEffect | lastJuiceEffect).HasFlag(JuiceEffect.EMP))
                {
                    foreach (Turret turret in FindObjectsByType<Turret>(FindObjectsSortMode.None))
                    {
                        turret.ToggleTurretEnabled(currentJuiceEffect != JuiceEffect.EMP);
                    }
                }
            }
            else
            {
                // Night vision effect
                if (currentJuiceEffect == JuiceEffect.NightVision)
                {
                    Light newlNightVision = player.nightVision;
                    newlNightVision.color = Color.Lerp(new Color(0, 1, 0.25f, 1), new Color(0, 1, 0, 1), Mathf.Sin(Mathf.PI * 2 * Time.time / 4));
                    newlNightVision.intensity = 10000 + Mathf.Sin(Mathf.PI * 2 * Time.time / 2) * 1000;
                    player.nightVision = newlNightVision;
                }
                // Signal jamming effect
                else if (currentJuiceEffect == JuiceEffect.SignalJamming)
                {
                    foreach (Turret turret in FindObjectsByType<Turret>(FindObjectsSortMode.None))
                    {
                        if (!turret.enteringBerserkMode)
                        {
                            turret.EnterBerserkModeServerRpc(player.GetInstanceID());
                        }
                    }
                }
            }

            if (transitionDelta > 0)
            {
                transitionDelta = Mathf.Max(0, transitionDelta - Time.deltaTime);
                if (transitionDelta == 0)
                {
                    lastJuiceEffect = currentJuiceEffect;
                }
            }
        }
    }
}
