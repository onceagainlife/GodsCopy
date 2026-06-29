using System.Collections.Generic;
using UnityEngine;

namespace HuaHaiLiKanHua
{

    /// Unity的 Object.Instantiate<T>() 要求 T 必须是 ​​UnityEngine.Object 的子类​​（如 GameObject/Component）。
    /// 删除 where T : Component 后，T 可能是任意类型（如 int、string），违反此规则
    /// 因此不能再用泛型，只能直接用GameObject。
    /// 如果一定要用泛型感觉也可以。创建的时候传入组件就行了。
    public class ObjectPool
    {
        //​​Stack​​ 是 后进先出的数据结构
        private Stack<GameObject> pool;
        private GameObject prefab;
        public List<GameObject> poolList;
        public ObjectPool(GameObject prefab, int initialSize,Transform parent=null,bool size=false)
        {
            poolList = new List<GameObject>(initialSize);
            this.prefab = prefab;
            pool = new Stack<GameObject>(initialSize);
            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = Object.Instantiate(prefab);
                obj.SetActive(false);
                if(parent != null)
                obj.transform.SetParent(parent, size);
                pool.Push(obj);
            }
        }

        public GameObject Get()
        {
            //调用栈的 Pop()，取出最近归还的对象
            GameObject obj= pool.Count > 0 ? pool.Pop() : Object.Instantiate(prefab);
            obj.SetActive(true);
            poolList.Add(obj);
            return obj;
        }

        public void Release(GameObject obj)
        {
            //调用栈的 Push()，将对象压入栈顶
            poolList.Remove(obj);
            //DescribePanel.transform.m_parent = null;
            obj.SetActive(false);
            pool.Push(obj);
        }

        public void ReleaseAll(GameObject obj)
        {
            //调用栈的 Push()，将对象压入栈顶
            //DescribePanel.transform.m_parent = null;
            obj.SetActive(false);
            obj.transform.localScale = Vector3.one;
            pool.Push(obj);
        }
        public void AllRelease()
        {
            if(poolList.Count <=0)  return; 
            foreach (var obj in poolList)
            {
                ReleaseAll(obj);
            }
            poolList.Clear();
        }
    }

}

