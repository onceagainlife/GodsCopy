namespace HuaHaiLiKanHua
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 简易优先队列（基于最小堆实现）
    /// 用于时间轴调度，保证每次出队（Dequeue）的元素具有最小的优先级（即最早的时间）
    /// </summary>
    /// <typeparam name="TElement">队列中存储的实际元素类型（如：技能指令 Struct）</typeparam>
    /// <typeparam name="TPriority">优先级类型（如：float 时间戳），必须支持比较</typeparam>
    public class SimplePriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
    {
        // 使用 List 存储元组（元素, 优先级）。
        // 堆在逻辑上是一棵完全二叉树，但在物理存储上就是一个一维数组（List）。
        private readonly List<(TElement Element, TPriority Priority)> _heap = new();

        /// <summary>
        /// 获取当前队列中的元素数量
        /// </summary>
        public int Count => _heap.Count;

        /// <summary>
        /// 入队：将一个新元素及其优先级加入队列
        /// 时间复杂度：O(log N)
        /// </summary>
        public void Enqueue(TElement element, TPriority priority)
        {
            // 1. 先将新元素添加到 List 的末尾（即完全二叉树的最后一个位置）
            _heap.Add((element, priority));
            // 2. 执行“上浮”操作，将新元素与父节点比较，如果比父节点小，就交换位置，直到满足堆的性质
            HeapifyUp(_heap.Count - 1);
        }

        /// <summary>
        /// 查看队首元素（优先级最高的元素），但不将其移除
        /// 时间复杂度：O(1)
        /// </summary>
        public TElement Peek()
        {
            if (_heap.Count == 0) throw new InvalidOperationException("Queue is empty.");
            // 堆顶元素（List的第一个元素）永远是优先级最小的
            return _heap[0].Element;
        }

        /// <summary>
        /// 出队：取出并移除优先级最高的元素
        /// 时间复杂度：O(log N)
        /// </summary>
        public TElement Dequeue()
        {
            if (_heap.Count == 0) throw new InvalidOperationException("Queue is empty.");

            // 1. 记录堆顶元素（即将要返回的元素）
            TElement element = _heap[0].Element;

            // 2. 将 List 的最后一个元素移动到堆顶（填补空缺）
            int lastIndex = _heap.Count - 1;
            _heap[0] = _heap[lastIndex];
            _heap.RemoveAt(lastIndex); // 移除末尾元素

            // 3. 执行“下沉”操作，将新的堆顶元素与子节点比较，如果比子节点大，就交换位置，直到满足堆的性质
            if (_heap.Count > 0) HeapifyDown(0);

            return element;
        }

        /// <summary>
        /// 上浮操作（Heapify Up）：用于入队后恢复堆的性质
        /// 新加入的元素在末尾，如果它比父节点小，就不断向上交换
        /// </summary>
        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                // 计算父节点的索引：(当前索引 - 1) / 2
                int parentIndex = (index - 1) / 2;

                // 如果当前节点的优先级 小于 父节点的优先级
                if (_heap[index].Priority.CompareTo(_heap[parentIndex].Priority) < 0)
                {
                    // 交换当前节点与父节点
                    var temp = _heap[index];
                    _heap[index] = _heap[parentIndex];
                    _heap[parentIndex] = temp;

                    // 更新索引，继续向上比较
                    index = parentIndex;
                }
                else
                {
                    // 已经比父节点大了，满足最小堆性质，停止上浮
                    break;
                }
            }
        }

        /// <summary>
        /// 下沉操作（Heapify Down）：用于出队后恢复堆的性质
        /// 堆顶被替换后，如果它比子节点大，就不断向下与最小的子节点交换
        /// </summary>
        private void HeapifyDown(int index)
        {
            int lastIndex = _heap.Count - 1;
            while (true)
            {
                // 计算左右子节点的索引：左 = 2*i+1, 右 = 2*i+2
                int leftChild = 2 * index + 1;
                int rightChild = 2 * index + 2;

                // 假设当前节点就是最小的
                int smallest = index;

                // 如果左子节点存在，且左子节点比当前最小节点还要小，更新 smallest
                if (leftChild <= lastIndex && _heap[leftChild].Priority.CompareTo(_heap[smallest].Priority) < 0)
                    smallest = leftChild;

                // 如果右子节点存在，且右子节点比当前最小节点还要小，更新 smallest
                if (rightChild <= lastIndex && _heap[rightChild].Priority.CompareTo(_heap[smallest].Priority) < 0)
                    smallest = rightChild;

                // 如果最小的不是当前节点，说明需要交换
                if (smallest != index)
                {
                    var temp = _heap[index];
                    _heap[index] = _heap[smallest];
                    _heap[smallest] = temp;

                    // 更新索引，继续向下比较
                    index = smallest;
                }
                else
                {
                    // 已经比所有子节点都小了，满足最小堆性质，停止下沉
                    break;
                }
            }
        }
    }
}