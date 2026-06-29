using UnityEngine;

namespace BackPackLike
{
    public class DiamondShader : MonoBehaviour
    {
        Material material;

        int fadePropertyID;
        public float OutlineValue = 0.1f;
        public float SquareValue=200;

        void Start()
        {
            //Get material reference
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                material = spriteRenderer.material;

            //Convert property name to id (improves performance).
            //You can see property names by hovering over them in the material inspector.
            fadePropertyID = Shader.PropertyToID("_InnerOutlineWide");
        }

        public void OutLineMode(){
            if (material != null)
                material.SetFloat(fadePropertyID, OutlineValue);
        }


        public void SquareMode()
        {
            if (material != null)
                material.SetFloat(fadePropertyID, SquareValue);
        }

    }
}

