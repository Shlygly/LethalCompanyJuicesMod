using GameNetcodeStuff;
using JuicesMod.Properties;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace JuicesMod.Behaviours
{
    public class ShipJuiceBlenderBehaviour : NetworkBehaviour
    {
        public bool isPowered = false;
        public bool hasBeenMixed = false;
        private List<JuiceProperty> juiceContent = [];

        private float _juiceLevel = 0;

        public Animator bladeAnimator;
        public Animator speedSwitchAnimator;
        public List<Renderer> juiceRenderers = new();
        public InteractTrigger powerButtonTrigger;
        public InteractTrigger addJuiceTrigger;
        public GameObject juiceSpawnFX;

        public AudioSource blenderMixAudioSource;

        public AudioSource sfxAudioSource;
        public List<AudioClip> addJuiceSFX = new();
        public AudioClip powerButtonSFX;

        public void Awake()
        {
            if (ES3.KeyExists("JuicesMod_JuiceBlender_Content", GameNetworkManager.Instance.currentSaveFileName))
            {
                juiceContent = ES3.Load<JuiceProperty[]>("JuicesMod_JuiceBlender_Content", GameNetworkManager.Instance.currentSaveFileName).ToList();
                for (int i = 0; i < juiceContent.Count; i++) {
                    juiceRenderers[i].material.color = juiceContent[i].Fruit.Color;
                }
            }
            if (!blenderMixAudioSource.isPlaying)
            {
                blenderMixAudioSource.Play();
            }
            blenderMixAudioSource.pitch = 0f;
        }

        public void Update()
        {
            // Triggers
            addJuiceTrigger.disabledHoverTip = isPowered ? "Can't add juice while blending" : "Blender is full";
            addJuiceTrigger.interactable = !isPowered && juiceContent.Count < 4;

            powerButtonTrigger.interactable = juiceContent.Count > 0;

            // Audio
            if (isPowered)
            {
                blenderMixAudioSource.pitch += (1 - blenderMixAudioSource.pitch) * 1 / 4;
            }
            else
            {
                blenderMixAudioSource.pitch += (0.5f - blenderMixAudioSource.pitch) * 1 / 16;
            }
            blenderMixAudioSource.volume = Mathf.Pow(2 * blenderMixAudioSource.pitch - 1, 0.25f);

            // Materials
            _juiceLevel += (juiceContent.Count - _juiceLevel) / 2;
            foreach (var renderer in juiceRenderers.Select((value, index) => new { index, value }))
            {
                Color color;
                if ((isPowered || hasBeenMixed) && juiceContent.Count > 0)
                {
                    Color mean = new Color(
                        juiceRenderers.Take(juiceContent.Count).Average(r => r.material.color.r),
                        juiceRenderers.Take(juiceContent.Count).Average(r => r.material.color.g),
                        juiceRenderers.Take(juiceContent.Count).Average(r => r.material.color.b)
                    );
                    color = Color.Lerp(renderer.value.material.color, mean, 0.1f);
                }
                else
                {
                    color = renderer.value.material.color;
                }
                color.a = Mathf.Clamp(_juiceLevel - renderer.index, 0, 1);
                renderer.value.material.color = color;
            }
        }

        public void SaveState()
        {
            ES3.Save("JuicesMod_JuiceBlender_Content", juiceContent.ToArray(), GameNetworkManager.Instance.currentSaveFileName);
        }

        public void DebugSpawnJuices()
        {
            HUDManager.Instance.DisplayTip("Spawning juices !", "16 random juices gonna be spawn !");

            for (int i = 0; i < 16; i++)
            {
                Item item = Plugin.instance.JuicesBuilder.Juices[Random.RandomRangeInt(0, Plugin.instance.JuicesBuilder.Juices.Length)];
                GameObject multifruit = Instantiate(
                    item.spawnPrefab,
                    gameObject.transform.position + Vector3.left * i / 2f,
                    Quaternion.identity,
                    StartOfRound.Instance.propsContainer
                );
                multifruit.GetComponent<PhysicsProp>().SetScrapValue((int)Random.Range(item.minValue * 0.4f, item.maxValue * 0.4f));
                multifruit.GetComponent<GrabbableObject>().fallTime = 0f;
                multifruit.GetComponent<NetworkObject>().Spawn();
            }
        }

        #region Power On/Off
        public void EarlyToggleSwitch()
        {
            if (!isPowered)
            {
                powerButtonTrigger.timeToHold = 3f;

                JuiceTypeProperty type = juiceContent.Find(j => j.Type.Multiplier == juiceContent.Min(j2 => j2.Type.Multiplier)).Type;
                int scrapSumValue = juiceContent.Sum(j => j.ScrapValue);
                float realMultiplier = Mathf.Lerp(1, type.Multiplier, (juiceContent.Count - 1) / 3f);

                HUDManager.Instance.DisplayTip(
                    $"{type.Name} multifruit",
                    $"Mixing these {juiceContent.Count} juices will produce a {type.Name.ToLower()} multifuit with a x{realMultiplier:0.##} bonus worth {scrapSumValue * realMultiplier:0.00}."
                );
            }
            else
            {
                powerButtonTrigger.timeToHold = 0.5f;
            }
        }

        public void ToggleSwitch(PlayerControllerB player)
        {
            SetBlendingStateServerRpc(!isPowered);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetBlendingStateServerRpc(bool state)
        {
            if (!state)
            {
                JuiceTypeProperty type = juiceContent.Find(j => j.Type.Multiplier == juiceContent.Min(j2 => j2.Type.Multiplier)).Type;
                GameObject multifruit = Instantiate(
                    Plugin.instance.JuicesBuilder.getMultifruit(type).spawnPrefab,
                    transform.position + Vector3.up + transform.forward * 0.5f,
                    transform.rotation,
                    RoundManager.Instance.spawnedScrapContainer
                );
                int scrapValue = (int)Mathf.Ceil(juiceContent.Sum(j => j.ScrapValue) * type.Multiplier);
                multifruit.GetComponent<GrabbableObject>().fallTime = 0f;
                multifruit.GetComponent<NetworkObject>().Spawn();

                StopBlendingClientRpc(multifruit, scrapValue);
            }
            else
            {
                StartBlendingClientRpc();
            }
        }

        [ClientRpc]
        public void StartBlendingClientRpc()
        {
            ApplyBlendingState(true);
        }

        [ClientRpc]
        public void StopBlendingClientRpc(NetworkObjectReference multifruit, int scrapValue)
        {
            ((GameObject)multifruit).GetComponent<PhysicsProp>().SetScrapValue(scrapValue);
            juiceContent.Clear();

            GameObject SpawnFx = Instantiate(
                juiceSpawnFX,
                ((GameObject)multifruit).transform.position,
                Quaternion.identity,
                ((GameObject)multifruit).transform
            );
            AudioSource SpawnSound = SpawnFx.GetComponent<AudioSource>();
            if (SpawnSound != null && !SpawnSound.isPlaying)
            {
                SpawnSound.Play();
            }

            ApplyBlendingState(false);
            SaveState();
        }

        private void ApplyBlendingState(bool state)
        {
            isPowered = state;
            hasBeenMixed = true;

            speedSwitchAnimator.SetBool("power", isPowered);
            bladeAnimator.SetBool("power", isPowered);
            sfxAudioSource.PlayOneShot(powerButtonSFX);
        }
        #endregion

        #region Add Juice
        public void AddJuice(PlayerControllerB player)
        {
            if (juiceContent.Count < 4)
            {
                GrabbableObject heldItem = player.currentlyHeldObjectServer;
                if (heldItem != null)
                {
                    if (Plugin.instance.JuicesBuilder.hasJuiceProperty(heldItem))
                    {
                        AddingJuiceServerRpc(Plugin.instance.JuicesBuilder.getJuiceProperty(heldItem));
                        player.DespawnHeldObject();
                    }
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddingJuiceServerRpc(JuiceProperty juice)
        {
            AddingJuiceClientRpc(juice);
        }

        [ClientRpc]
        public void AddingJuiceClientRpc(JuiceProperty juice)
        {
            ApplyAddingJuice(juice);
        }

        public void ApplyAddingJuice(JuiceProperty juice)
        {
            hasBeenMixed = false;
            juiceContent.Add(juice);

            juiceRenderers[juiceContent.Count - 1].material.color = juice.Fruit.Color;

            sfxAudioSource.PlayOneShot(addJuiceSFX[Random.RandomRangeInt(0, addJuiceSFX.Count)]);

            SaveState();
        }
        #endregion
    }
}
