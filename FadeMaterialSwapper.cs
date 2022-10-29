using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class FadeMaterialSwapper : SerializedMonoBehaviour {
    public float fadeDistance;
    [SerializeField] private List<Material> materials;
    [SerializeField] private Renderer[] renderers = new Renderer[0];
    [ShowInInspector] private static Dictionary<Material, Material> materialMap = new Dictionary<Material, Material>();
    private static Dictionary<Material, Material> reverseMaterialMap = new Dictionary<Material, Material>();
    [ShowInInspector] public static GameObject fadeObject = null;
    [ShowInInspector] private bool currentSwappedState = false;
    
    private void Reset() {
        if (renderers.Length == 0) {
            renderers = gameObject.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) {
                Debug.LogWarning("Unused MaterialSwapper");
                return;
            }
            HashSet<Material> materials = new HashSet<Material>();
            foreach (Renderer renderer in renderers) {
                foreach (Material material in renderer.sharedMaterials) {
                    if (material != null) {
                        materials.Add(material);
                    }
                }
            }
            this.materials = new List<Material>(materials);
        }
    }

    private void Start() {
        foreach (Material material in materials) {
            if (!materialMap.ContainsKey(material)) {
                materialMap.Add(material, GenerateTransparentMaterial(material));
                reverseMaterialMap.Add(materialMap[material], material);
            }
        }
    }

    private void Update() {
        // Debug.LogError(name + " " + fadeDistance + " / " + Vector3.Distance(fadeObject.transform.position, this.transform.position));
        if (fadeObject != null && Vector3.Distance(fadeObject.transform.position, this.transform.position) < fadeDistance) {
            SetState(true);
        } else {
            SetState(false);
        }
    }

    public void SetState(bool swapped) {
        if (currentSwappedState == swapped) {
            return;
        }
        foreach (Renderer renderer in renderers) {
            Material[] oldSharedMaterials = renderer.sharedMaterials;
            Material[] newSharedMaterials = new Material[oldSharedMaterials.Length];
            for (int i = 0; i < oldSharedMaterials.Length; i++) {
                Material oldSharedMaterial = oldSharedMaterials[i];
                Material newSharedMaterial = oldSharedMaterial;
                if (swapped) {
                    if (materialMap.ContainsKey(oldSharedMaterial)) {
                        newSharedMaterial = materialMap[oldSharedMaterial];
                    }
                } else {
                    if (reverseMaterialMap.ContainsKey(oldSharedMaterial)) {
                        newSharedMaterial = reverseMaterialMap[oldSharedMaterial];
                    }
                }
                newSharedMaterials[i] = newSharedMaterial;
            }
            renderer.sharedMaterials = newSharedMaterials;
        }
        currentSwappedState = swapped;
    }

    private Material GenerateTransparentMaterial(Material input) {
        float alphaValue = .2f;
        Material fadeMaterial = new Material(Shader.Find("Standard"));
        fadeMaterial.name = input.name + " Fade Material";
        fadeMaterial.CopyPropertiesFromMaterial(input);
        SetupMaterialWithFadeBlendMode(fadeMaterial);
        if (input.shader.name == "Raygeas/AZURE Surface") {
            Color originalColor = input.GetColor("_SurfaceColor");
            fadeMaterial.color = new Color(originalColor.r, originalColor.g, originalColor.b, alphaValue);
        } else {
            fadeMaterial.color = new Color(fadeMaterial.color.r, fadeMaterial.color.g, fadeMaterial.color.b, alphaValue);
        }
        return fadeMaterial;
    }

    private void SetupMaterialWithFadeBlendMode(Material material) {
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0.0f);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }
}
