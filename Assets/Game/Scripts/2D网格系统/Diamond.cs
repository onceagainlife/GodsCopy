using UnityEngine;


namespace BackPackLike
{
    /// <summary>
    /// 缃戞牸鏄剧ず鐗╀綋锛岃礋璐ｈ褰曡嚜韬潗鏍囧苟鏄剧ず/杩樺師棰勮棰滆壊銆?
    /// </summary>
    public class Diamond : MonoBehaviour
    {
        /// <summary>
        /// 璇ユ樉绀烘牸鍦ㄨ儗鍖呭瓧鍏镐腑鐨勭浉瀵瑰潗鏍囥€?
        /// </summary>
        public CellStruct cellStruct;
        public PlaceMode placeMode;
        //public DiamondShader diamondShader;

        // 鍚屾椂鍏煎 SpriteRenderer 鍜?UGUI Image 涓ょ鏄剧ず鏂瑰紡銆?
        private SpriteRenderer spriteRenderer;
        private Color defaultColor = Color.white;

        private void Awake()
        {
            CacheDefaultColor();
        }

        /// <summary>
        /// 缂撳瓨褰撳墠鏄剧ず缁勪欢鍜岄粯璁ら鑹诧紝鐢ㄤ簬棰勮缁撴潫鍚庤繕鍘熴€?
        /// </summary>
        public void CacheDefaultColor()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void CheckPlaceMode(bool canPlace)
        {
            //if (diamondShader == null) return;
            if(spriteRenderer==null) return;

            switch (placeMode)
            {
                case PlaceMode.Valid:
                    //鏂瑰潡妯″紡
                    //diamondShader.SquareMode();
                    if (canPlace)
                    {
                        //鍏ㄧ豢鑹?
                        spriteRenderer.color = GlobalStatic.blueColor;
                    }
                    else
                    {
                        spriteRenderer.color = GlobalStatic.yellowColor;
                    }
                    break;
                case PlaceMode.Occupied:
                    //绾㈣壊鏂瑰潡
                    //diamondShader.SquareMode();
                    spriteRenderer.color = GlobalStatic.redColor;
                    break;
                case PlaceMode.Empty:
                    //diamondShader.OutLineMode();
                    spriteRenderer.color = GlobalStatic.yellowColor;
                    break;
            }
        }
    }
}
