using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace JuicesMod.Behaviours
{
    public class ShipNeonAnimation : NetworkBehaviour
    {
        public List<Material> materials = [];

        public float minIntensity = 0;
        public float maxIntensity = 1;

        public NeonAnimation animation = NeonAnimation.Blink;
        public float animationSpeed = 1;

        private Color[] originalColors;

        public void Awake()
        {
            originalColors = materials.Select(c => c.GetColor("_EmissiveColor")).ToArray();
        }

        public void Update()
        {
            int animationDuration =
                animation == NeonAnimation.Blink ? 2 :
                animation == NeonAnimation.AppearBlinkDisapear ? (materials.Count + 5) * 2 :
                animation == NeonAnimation.OneByOne ? materials.Count :
                0;

            float[] animationIntensities = materials.Select(m => 1f).ToArray();
            float animationFrame = (Time.time / animationSpeed) % animationDuration;

            if (animation == NeonAnimation.Blink)
            {
                animationIntensities = animationIntensities.Select(l => animationFrame % 2 < 1 ? 1f : 0f).ToArray();
            }
            else if (animation == NeonAnimation.AppearBlinkDisapear)
            {
                if (animationFrame < materials.Count)
                {
                    animationIntensities = animationIntensities.Select((l, i) => animationFrame - 1 > i ? 1f : 0f).ToArray();
                }
                else if (animationFrame < materials.Count + 5 * 2)
                {
                    animationIntensities = animationIntensities.Select(l => animationFrame % 1 < 0.5f ? 1f : 0f).ToArray();
                }
                else
                {
                    animationIntensities = animationIntensities.Select((l, i) => (materials.Count + 5) * 2 - animationFrame > i ? 1f : 0f).ToArray();
                }
            }
            else if (animation == NeonAnimation.OneByOne)
            {
                animationIntensities = animationIntensities.Select((l, i) => Mathf.FloorToInt(animationFrame) == i ? 1f : 0f).ToArray();
            }

            float intensity = Random.Range(minIntensity, maxIntensity);
            for (int i = 0; i < materials.Count; i++)
            {
                materials[i].SetColor("_EmissiveColor", originalColors[i] * animationIntensities[i] * intensity);
            }
        }
    }

    public enum NeonAnimation
    {
        Blink,
        AppearBlinkDisapear,
        OneByOne
    }
}
