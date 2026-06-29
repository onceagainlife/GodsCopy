using UnityEngine;

namespace HuaHaiLiKanHua
{
    public  class ObjectPoolShow: MonoBehaviour
    {
        public GameObject prefab;
        public ObjectPool objPool;
        [Tooltip("ÊýÁ¿")]
        public int initialSize = 10;

        public virtual void Init()
        {
            objPool = new ObjectPool(prefab, initialSize,transform);
        }

        public virtual void OnShow(int count, Vector2 pos)
        {
            GameObject obj = objPool.Get();
            obj.transform.position = pos;
        }

        public virtual void OnHide(GameObject obj)
        {
            objPool.Release(obj);

        }
    }
}

