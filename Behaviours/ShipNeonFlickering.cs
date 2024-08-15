using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace JuicesMod.Behaviours
{
    public class ShipNeonFlickering : NetworkBehaviour
    {
        public List<Material> materials = [];

        public float minIntensity = 0;
        public float maxIntensity = 1;

        private Color[] originalColors;

        public void Awake()
        {
            originalColors = materials.Select(c => c.GetColor("_EmissiveColor")).ToArray();
        }

        public void Update()
        {
            float intensity = Random.Range(minIntensity, maxIntensity);
            for (int i = 0; i < materials.Count; i++)
            {
                materials[i].SetColor("_EmissiveColor", originalColors[i] * intensity);
            }
        }
    }
}
