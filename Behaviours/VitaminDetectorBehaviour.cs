using System.Linq;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace JuicesMod.Behaviours
{
    public class VitaminDetectorBehaviour : PhysicsProp
    {
        public float RadarMinDistance = 0f;
        public float RadarMaxDistance = 50f;

        public ParticleSystem RadarParticle;
        public AudioSource RadarAudio;
        public AudioSource ItemAudio;
        public AudioClip PowerOnClip;
        public AudioClip PowerOffClip;
        public AudioClip OutOfBatteriesClip;

        private bool activated = false;

        public override void Start()
        {
            base.Start();
            insertedBattery.charge = 1f;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            ToggleState(!activated);

            if (activated)
            {
                RadarParticle.Play();
            }
            else
            {
                RadarParticle.Stop();
            }
        }

        public override void UseUpBatteries()
        {
            base.UseUpBatteries();
            ToggleState(false);
            ItemAudio.PlayOneShot(OutOfBatteriesClip);
        }

        public void ToggleState(bool state)
        {
            activated = state;
            ItemAudio.PlayOneShot(activated ? PowerOnClip : PowerOffClip);
        }

        public override void Update()
        {
            base.Update();
            GrabbableObject[] juices = FindObjectsOfType<PhysicsProp>()
                    .Where(juice => Plugin.instance.JuicesBuilder.hasJuiceProperty(juice) || Plugin.instance.JuicesBuilder.isMultifruit(juice))
                    .ToArray();

            if (!RadarParticle.isPlaying && activated && (playerHeldBy == null || playerHeldBy.currentlyHeldObjectServer == this))
            {
                RadarParticle.Play();
            }
            else if (RadarParticle.isPlaying && (!activated || (playerHeldBy != null && playerHeldBy.currentlyHeldObjectServer != this)))
            {
                RadarParticle.Stop();
            }

            if (!RadarAudio.isPlaying && activated)
            {
                RadarAudio.Play();
            }
            else if (RadarAudio.isPlaying && !activated)
            {
                RadarAudio.Stop();
            }

            if (activated)
            {
                float distValue = (Mathf.Clamp(
                    juices
                        .Where(juice => !juice.isPocketed)
                        .Select(juice => Vector3.Distance(transform.position, juice.transform.position))
                        .OrderBy(dist => dist)
                        .DefaultIfEmpty(RadarMaxDistance)
                        .First(),
                    RadarMinDistance, RadarMaxDistance
                ) - RadarMinDistance) / (RadarMaxDistance - RadarMinDistance);
                EmissionModule emission = RadarParticle.emission;
                emission.rateOverTime = Mathf.Lerp(4, 0.25f, distValue);
                RadarAudio.pitch = Mathf.Lerp(4, 0.25f, distValue);
                Material particleMaterial = RadarParticle.GetComponent<ParticleSystemRenderer>().material;
                particleMaterial.SetColor("_EmissionColor", Color.Lerp(new Color(0, 1.97667456f, 0, 1), new Color(1.97667456f, 0, 0, 1), distValue));
            }
        }
    }
}
