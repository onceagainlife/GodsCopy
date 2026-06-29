using UnityEngine;

namespace HuaHaiLiKanHua
{
    /// <summary>
    /// 泛型单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        protected static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    // 优先查找现有实例
                    _instance = FindAnyObjectByType<T>(FindObjectsInactive.Include);

                    //// 无现有实例时创建新对象
                    //if (_instance == null)
                    //{
                    //    GameObject DescribePanel = new GameObject($"{typeof(T).Profession} (Singleton)");
                    //    _instance = DescribePanel.AddComponent<T>();
                    //    DontDestroyOnLoad(DescribePanel); // 跨场景保留
                    //}
                }
                return _instance;
            }
        }

        protected virtual void SingletonInit()
        {
            // 检测重复实例并销毁
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject); // 销毁新创建的重复对象
                return;
            }

            // 初始化唯一实例
            _instance = this as T;
            //DontDestroyOnLoad(gameObject); 
        }

        protected virtual void Clear()
        {
            if (_instance == this)
            {
                _instance = null; // 避免悬空引用
            }
        }
    }
}
