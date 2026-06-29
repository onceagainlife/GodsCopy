using HuaHaiLiKanHua;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BackPackLike
{
    public class DiamondShaderPool : ObjectPoolShow
    {
        //public AssetReferenceGameObject spherePrefabRef;
        public List<Diamond> diamondList=new();
        //private AsyncOperationHandle<GameObject> loadHandle;

        public override void Init()
        {
            base.Init();
           
            //if (!spherePrefabRef.RuntimeKeyIsValid())
            //{
            //    Debug.LogError("Prefab Reference 未赋值");
            //    return;
            //}

            //loadHandle = spherePrefabRef.LoadAssetAsync<GameObject>();
            //loadHandle.Completed += OnPrefabLoaded;
        }

        //private void OnPrefabLoaded(AsyncOperationHandle<GameObject> handle)
        //{
        //    if (handle.Status != AsyncOperationStatus.Succeeded)
        //    {
        //        Debug.LogError("DiamondShaderPool prefab 加载失败");
        //        return;
        //    }

        //    prefab = handle.Result;
        //    base.OnInit(); //
        //}

        public Diamond OnShowDiamond(int count,Vector2 pos, Backpack backpack)
        {
            //Vector2 pos = cellClass.truePos;
            GameObject obj = objPool.Get();
            Diamond diamond =obj.GetComponent<Diamond>();
            SetDiamondPosition(diamond, pos, backpack);
            diamondList.Add(diamond);
            return diamond;
        }
        //public Diamond OnShowDiamond(int count, Vector2 pos)
        //{
        //    //Vector2 pos = cellClass.truePos;
        //    GameObject obj = objPool.Get();
        //    obj.transform.position = pos;
        //    Diamond diamond = obj.GetComponent<Diamond>();
        //    diamondList.Add(diamond);
        //    return diamond;
        //}
        public override void OnHide(GameObject obj)
        {
            objPool.Release(obj);

        }

        /// <summary>
        /// 更新列表,目前是将不需要的setActive(false)
        /// </summary>
        /// <param name="count"></param>
        public void UpdateList(int count)
        {
            if (diamondList.Count > count)
            {
                for (int i = count-1; i < diamondList.Count; i++)
                {
                    diamondList[i].gameObject.SetActive(false);
                }
            }
            else if (diamondList.Count < count)
            {
                int diffent = count - diamondList.Count;
                for(int i = 0; i < diffent; i++)
                {
                    OnShow(i, Vector2.one);
                }
            }

        }
        ///// <summary>
        ///// 更新数据
        ///// </summary>
        //public void UpdateDiamond(int index,Vector2 pos)
        //{
        //    if (index >= diamondList.Count)
        //    {
        //        OnShow(index, cellClass.truePos);
        //        return;
        //    }
        //    diamondList[index].transform.gameObject.SetActive(true);
        //    diamondList[index].transform.posiInDic = pos;
        //}

        //public void UpdateDiamond(int index, CellClass cellClass,PlaceMode placeMode)
        //{
        //    if (index >= diamondList.Count)
        //    {
        //        OnShow(index, cellClass.truePos);
        //        //diamond.transform.gameObject.SetActive(true);
        //        //diamond.transform.posiInDic = cellClass.truePos;
        //        //return;
        //    }
        //    diamondList[index].placeMode= placeMode;
        //    //diamondList[index].CellClass = cellClass;
        //    diamondList[index].transform.gameObject.SetActive(true);
        //    diamondList[index].transform.position = cellClass.truePos;
        //}

        //public void UpdateDiamond(List<CellStruct> cellStructList)
        //{
        //    int index= 0;
        //    foreach (Diamond diamond in diamondList) 
        //    {
        //        UpdateDiamond(index, cellStructList[index]);
        //        index++;
        //    }
            
        //}
        public void UpdateDiamond(int index, CellStruct cellStruct,Backpack backpack)
        {
            PlaceMode placeMode = cellStruct.placeMode;

            if (index >= diamondList.Count)
            {
                Diamond diamond = OnShowDiamond(index, cellStruct.truePos, backpack);
                diamond.placeMode = placeMode;
                diamond.cellStruct = cellStruct;
                diamond.transform.gameObject.SetActive(cellStruct.show);
                SetDiamondPosition(diamond, cellStruct.truePos, backpack);
                //diamond.transform.position = cellStruct.truePos;
                return;
            }
            
            diamondList[index].placeMode = placeMode;
            diamondList[index].cellStruct = cellStruct;
            diamondList[index].transform.gameObject.SetActive(cellStruct.show);
            SetDiamondPosition(diamondList[index], cellStruct.truePos, backpack);
            //diamond.transform.position = cellStruct.truePos;
        }

        private void SetDiamondPosition(Diamond diamond, Vector2 position, Backpack backpack)
        {
            if (diamond == null)
            {
                return;
            }

            if (diamond.transform is RectTransform rectTransform)
            {

                Vector2 newPos = CoordinateConverter.WorldToUI_LocalPosition(backpack.gridUISystem.parentRect, position);
                Vector2 oldPivot=backpack.GetGridRectTransform().pivot;
                Vector2 delta = rectTransform.pivot-oldPivot;
                rectTransform.localPosition = new Vector2(
                    newPos.x + rectTransform.sizeDelta.x * delta.x,
                    newPos.y + rectTransform.sizeDelta.y * delta.y
                );
                Debug.Log($"SetDiamondPosition: WorldPos={position}, LocalPos={newPos} 补偿后={rectTransform.localPosition}");
                //rectTransform.localPosition = position;
                return;
            }

            diamond.transform.position = position;
        }

        public void OnDestroy()
        {
            //Addressables.Release(loadHandle);
            //loadHandle.Completed -= OnPrefabLoaded;
        }
    }
}

