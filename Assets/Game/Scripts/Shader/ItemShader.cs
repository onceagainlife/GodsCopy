using UnityEngine;

namespace BackPackLike
{
    public class ItemShader : MonoBehaviour
    {
        Material material;

        int fadePropertyID;
        public float OppressedValue=0.2f;
        public float fadeValue;

        void Start()
        {
            //Get material reference
            material = GetComponent<SpriteRenderer>().material;

            //Convert property name to id (improves performance).
            //You can see property names by hovering over them in the material inspector.
            fadePropertyID = Shader.PropertyToID("_StrongTintFade");

        }

     

        /// <summary>
        /// 被压迫
        /// </summary>
        public void Oppressed()
        {
            //Update value in material.
            material.SetFloat(fadePropertyID, OppressedValue);
        }
       
        /// <summary>
        /// 被释放
        /// </summary>
        public void Released()
        {
            material.SetFloat(fadePropertyID, fadeValue);
        }
    }
}


