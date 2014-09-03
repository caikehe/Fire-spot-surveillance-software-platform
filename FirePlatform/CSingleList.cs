using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsFormsFirePlatform
{
    public class CSingleList<T> where T : IComparable
    {
        //头结点
        private CNode<T> m_head;

        //默认构造函数
        public CSingleList() 
        { 
            m_head = new CNode<T>(); 
        }

        //
        public CSingleList(CNode<T> k)
        {
            m_head.Next = k;
        }

        public CNode<T> Head
        {
            get { return m_head; }
            set { m_head = value; }
        }

        public bool Empty
        {
            get
            {
                return m_head.Next == null;
            }
        }

        //返回链表长度
        public int Count
        {
            get{
                int n = 0;
                CNode<T> p = m_head.Next;
                while (p != null)
                {
                    n++;
                    p = p.Next;
                }
                return n;
            }
        }

        //以升序序插入结点
        public void AscendInsert(CNode<T> node)
        {
            CNode<T> p = m_head.Next;
            if (p == null)
                m_head.Next = node;
            else if (p.KeyValue.CompareTo(node.KeyValue) > 0)//传入结点比第一个小，更换头结点
            {
                m_head.Next = node;
                node.Next = p;
            }
            else
            {
                CNode<T> q = p.Next;
                while (q != null)
                {
                    if (q.KeyValue.CompareTo(node.KeyValue) <= 0)
                    {
                        p = q;
                        q = q.Next;
                    }
                    else
                        break;
                }
                p.Next = node;
                node.Next = q;
            }
        }

        //删除链表最后一个结点
        public void DeleteLast()
        {
            if (m_head.Next.Next != null)//保证链表至少有两个结点
            {
                CNode<T> p = m_head;
                CNode<T> q = p.Next;
                while (q.Next != null)
                {
                    p = q;
                    q = q.Next;
                }
                p.Next = null;
            }
        }

        //获取指定位置的数据
        public virtual CNode<T> this[int i]
        {
            get 
            {
                if(i < 0)
                    throw new IndexOutOfRangeException("Index is negatice in " + this.GetType());
                int n = 0;
                CNode<T> node = m_head.Next;
                while (node != null && i != n)
                {
                    node = node.Next;
                    n++;
                }
                if (node == null)
                    throw new IndexOutOfRangeException("Index out of range in " + this.GetType());
                return node;
            }
        }
    }
}
