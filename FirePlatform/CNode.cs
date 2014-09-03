using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace WindowsFormsFirePlatform
{
    public class CNode<T>
    {
        //结点值
        private T m_KeyValue;
        //起点
        private Point m_StartPoint;
        //终点
        private Point m_EndPoint;
        //后一结点
        private CNode<T> m_Next;

        public CNode()
        {
            m_Next = null;
        }

        public CNode(T keyvalue, Point p1, Point p2)
        {
            m_KeyValue = keyvalue;
            m_StartPoint = p1;
            m_EndPoint = p2;
            m_Next = null;
        }

        public T KeyValue
        {
            get { return m_KeyValue; }
            set { m_KeyValue = value; }
        }

        public Point StartPoint
        {
            get { return m_StartPoint; }
            set { m_StartPoint = value; }
        }

        public Point EndPoint
        {
            get { return m_EndPoint; }
            set { m_EndPoint = value; }
        }

        public CNode<T> Next
        {
            get { return m_Next; }
            set { m_Next = value; }
        }
    }
}
