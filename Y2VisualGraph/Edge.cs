using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Y2VisualGraph
{
    [Serializable]
    class EdgeCollection : IEnumerable<Edge>
    {
        List<Edge> _list;

        public int SelectedIndex
        {
            get;
            set;
        }

        public Edge SelectedItem
        {
            get
            {
                return _list[this.SelectedIndex];
            }
        }

        public EdgeCollection()
        {
            _list = new List<Edge>();
        }
        public bool Add(Edge edge)
        {
            if (!_list.Contains(edge))
            {
                Edge newEdge = new Edge(edge.End, edge.Start);
                if (_list.Contains(newEdge))
                {
                    edge = _list[_list.IndexOf(newEdge)];
                    edge.IsUndirected = true;
                }
                else
                {
                    _list.Add(edge);
                }
                return true;
            }
            return false;
        }
        public void Clear()
        {
            _list.Clear();
        }
        public bool Contains(Edge edge)
        {
            return _list.Contains(edge);
        }
        public Edge this[int index]
        {
            get { return _list[index]; }
        }
        public bool Contains(Edge edge, bool checkInverted)
        {
            if (_list.Contains(edge))
                return true;
            if (checkInverted)
                return _list.Contains(new Edge(edge.End, edge.Start));
            return false;
        }
        public void RemoveAt(int index)
        {
            System.Threading.Thread.Sleep(100);
            _list.RemoveAt(index);
        }
        public int Count
        {
            get { return _list.Count; }
        }
        public IEnumerator<Edge> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }
    [Serializable]
    class Edge
    {
        private const float EPSILON = 2f;

        public int Start;
        public int End;
        public bool IsUndirected;
        public bool IsRemoving;

        public Edge(int start, int end)
        {
            this.Start = start;
            this.End = end;
            this.IsUndirected = false;

        }


        public static bool Contains(PointF start, PointF end, PointF p)
        {
            if (p.X < Math.Min(start.X, end.X) ||
                    p.X > Math.Max(start.X, end.X) ||
                    p.Y < Math.Min(start.Y, end.Y) ||
                    p.Y > Math.Max(start.Y, end.Y))
                return false;
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;
            var v1 = new Vector2D(dx, dy).Length;


            float cx = p.X - start.X;
            float cy = p.Y - start.Y;
            var v2 = new Vector2D(cx, cy).Length;

            var v3 = new Vector2D(p.X - end.X, p.Y - end.Y).Length;
            var pp = (v1 + v2 + v3) / 2;

            var s = Math.Sqrt(pp * (pp - v1) * (pp - v2) * (pp - v3));
            var h = s * 2 / v1;
            Console.WriteLine(h);
            return h < EPSILON;
        }

        public override bool Equals(Object obj)
        {
            if (!(obj is Edge))
                return false;
            Edge con = (Edge)obj;
            if (this.Start == con.Start && this.End == con.End)
                return true;
            if (this.IsUndirected && this.Start == con.End && this.End == con.Start)
                return true;
            return false;
        }
        public override int GetHashCode()
        {
            return Start ^ End;
        }
    }
}
