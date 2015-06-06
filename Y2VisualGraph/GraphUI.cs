using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

namespace Y2VisualGraph
{
    partial class GraphUI : Control
    {

        public event EventHandler ContentChanged;
        public event EventHandler SelectedNodeChanged;
        public const int NODE_RADIUS = 12;
        public const int NODE_DIAMETER = NODE_RADIUS * 2;
        Matrix _matrix;
        Pen _penEdge;
        Point _startPoint; 
        int _startIndex;

        /// <summary>
        /// index of selected child control
        /// </summary>
        int _selectedIndex;

        Point _p;
        EdgeCollection _edges;
        List<Point> _path = new List<Point>();
        // using to trace path
        int _preNodeIndex;

        public bool _isUndirectedGraph;

        public bool IsUndirectedGraph
        {
            get { return _isUndirectedGraph; }
            set
            {
                _isUndirectedGraph = value;
                if (value)
                    _penEdge.EndCap = LineCap.NoAnchor;
                else
                    _penEdge.EndCap = LineCap.ArrowAnchor;
                RefreshMatrix();
                Invalidate();
            }
        }
        public DrawingTools Tool;
        private MovingBall _ball;

        public IEnumerable<char> NodeNames
        {
            get
            {
                List<char> list = new List<char>();
                for (int i = 0; i < this.Controls.Count - 1; i++)
                {
                    list.Add((char)('A' + i));
                }
                return list;
            }
        }
        public GraphUI()
        {

            _edges = new EdgeCollection();

            this.DoubleBuffered = true;
            Control.CheckForIllegalCrossThreadCalls = false;

            _penEdge = new Pen(Color.MediumPurple, 4);
            _penEdge.EndCap = LineCap.ArrowAnchor;

            _matrix = new Matrix();
            _matrix.SizeChanged += new EventHandler(_matrix_SizeChanged);
            Reset();
        }


        void _matrix_SizeChanged(object sender, EventArgs e)
        {
            Refresh();
        }
        public void Reset()
        {
            _edges.Clear();
            this.Controls.Clear();
            if (_ball != null)
                _ball.Stop();
            _matrix.Reset();
            _matrix.Location = new Point(1, 1);
            Invalidate();

            this.Controls.Add(_matrix);

        }

        public Node SelectedNode
        {
            get
            {
                if (_selectedIndex < 1)
                    return null;
                return this.Controls[_selectedIndex] as Node;
            }
        }
        private void RefreshMatrix()
        {
            if (this.Controls.Count > 1)
                _matrix.Data = CreateMatrix();
        }

        public void FindShortestPath(int x, int y)
        {
            try
            {
                if (_matrix.FloydData == null)
                    return;
                int value = _matrix.FloydData[x, y].Value;
                if (value < 0)
                {
                    MessageBox.Show(String.Format("Can not find any path from '{0}' to '{1}'", (char)('A' + x), (char)('A' + y)));
                    return;
                }
                if (_ball == null)
                    _ball = new MovingBall(this);
                _ball.Reset();
                _path.Clear();
                _preNodeIndex = x;
                _ball.NodeLocations.Add(this.Controls[x + 1].Location);

                GetPath(x, y);

                _ball.NodeLocations.Add(this.Controls[y + 1].Location);
                _ball.Start();
                Invalidate();
            }
            catch (IndexOutOfRangeException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void GetPath(int x, int y)
        {
            if (_matrix.FloydData[x, y].Value == -1)
                return;
            int i = _matrix.FloydData[x, y].Previous;
            if (i == -1)
            {
                _path.Add(new Point(x + 1, y + 1));
                Point pos = this.Controls[y + 1].Location;
                _ball.NodeLocations.Add(pos);
                return;
            }
            else
            {
                GetPath(x, i);
                _path.Add(new Point(_preNodeIndex + 1, i + 1));
                _preNodeIndex = i;
                _ball.NodeLocations.Add(this.Controls[i + 1].Location);
                GetPath(i, y);
            }
        }

        #region Save/Load

        public void SaveGraph(string fileName, int fromNode, int toNode)
        {

            GraphData data = new GraphData(fromNode, toNode);
            data.IsUndirectedGraph = IsUndirectedGraph;

            for (int i = 1; i < this.Controls.Count; i++)
            {
                Point p = this.Controls[i].Location;
                p.Offset(NODE_RADIUS, NODE_RADIUS);

                data.NodeLocations.Add(p);
            }
            data.Edges = _edges;
            try
            {
                Stream stream = File.Open(fileName, FileMode.Create);
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, data);
                stream.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
        public GraphData LoadGraph(string fileName)
        {
            try
            {
                GraphData data = null;

                //Open the file written above and read values from it.

                Stream stream = File.Open(fileName, FileMode.Open);
                BinaryFormatter bformatter = new BinaryFormatter();

                data = (GraphData)bformatter.Deserialize(stream);
                stream.Close();

                Reset();

                foreach (var item in data.NodeLocations)
                {
                    AddNewNode(item);
                }
                _edges = data.Edges;


                OnContentChanged(null, null);
                return data;

            }
            catch { }
            return null;
        }

        #endregion

        int[,] CreateMatrix()
        {
            int[,] array = new int[this.Controls.Count - 1, this.Controls.Count - 1];

            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(0); j++)
                {
                    if (_edges.Contains(new Edge(i + 1, j + 1), this.IsUndirectedGraph))
                    {
                        array[i, j] = GetDistance(this.Controls[i + 1].Location, this.Controls[j + 1].Location);
                    }
                    else if (i != j)
                        array[i, j] = -1;
                }
            }
            return array;
        }
        void RefreshSubControls()
        {
            this._selectedIndex = -1;
            for (int i = 1; i < this.Controls.Count; i++)
            {
                Node node = this.Controls[i] as Node;
                node.Index = i;
                node.DisplayName = (char)('A' + i - 1);
                node.Invalidate();
            }
            OnSeletedNodeChanged(null, null);
        }
        int GetDistance(Point p1, Point p2)
        {
            return (int)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2)) / 10;
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            //foreach(Control ctl in this.Controls)

            if (e.Button == MouseButtons.Left)
            {
                if (this.Tool == DrawingTools.Node)
                {
                    int m = 'Z' - 'A' + 2;
                    if (this.Controls.Count == m)
                    {
                        MessageBox.Show("You can only add " + m + " nodes to graph");
                        return;
                    }
                    AddNewNode(e.Location);

                }
                else if (this.Tool == DrawingTools.Edge || this.Tool == DrawingTools.Eraser)
                {
                    int count = 0;
                    foreach (var edge in _edges)
                    {
                        var start = this.Controls[edge.Start].Location;
                        start.X += NODE_RADIUS;
                        start.Y += NODE_RADIUS;
                        var end = this.Controls[edge.End].Location;
                        end.X += NODE_RADIUS;
                        end.Y += NODE_RADIUS;
                        if (Edge.Contains(start, end, e.Location))
                        {
                            if (this.Tool == DrawingTools.Edge)
                            {
                                _edges.SelectedIndex = count;
                            }
                            else if (this.Tool == DrawingTools.Eraser)
                            {
                                _edges.RemoveAt(count);
                                _edges.SelectedIndex = -1;
                            }
                            this.Invalidate();
                            
                            break;
                        }
                        count++;
                    }
                }
                else if (this.Tool == DrawingTools.Eraser) // delete edge
                {

                }
            }
            OnContentChanged(null, null);
            base.OnMouseDown(e);
        }

        private void AddNewNode(Point location)
        {
            Node n = new Node();
            n.Index = this.Controls.Count;
            n.DisplayName = (char)(n.Index + 'A' - 1);
            n.Location = location;
            this.Controls.Add(n);
            n.DoCreatingAnimation();
            n.Width = NODE_DIAMETER;
            n.Height = NODE_DIAMETER;

            n.MouseDown += new MouseEventHandler(Node_MouseDown);
            n.MouseMove += new MouseEventHandler(Node_MouseMove);
            n.MouseUp += new MouseEventHandler(Node_MouseUp);
        }
        public void ClearEdges()
        {
            _edges.Clear();
            if (_ball != null)
                _ball.Stop();
            Invalidate();
        }
        private void DeleteEdgeAt(int index)
        {
            _edges[index].IsRemoving = true;
            Refresh();
            _edges.RemoveAt(index);
            RefreshMatrix();
            Invalidate();
        }

        public void DeleteLastestEdge()
        {
            if (_edges.Count > 0)
            {
                DeleteEdgeAt(_edges.Count - 1);
            }
        }
        public void DeleteSelectedNode()
        {
            if (_selectedIndex < 1 || (_ball != null && _ball.IsRunning))
                return;
            int i = 0;
            while (i < _edges.Count)
            {
                Edge edge = _edges[i];
                if (edge.End == _selectedIndex || edge.Start == _selectedIndex)
                {
                    DeleteEdgeAt(i);
                }
                else
                {
                    if (edge.Start >= _selectedIndex)
                        edge.Start--;
                    if (edge.End >= _selectedIndex)
                        edge.End--;
                    i++;
                }
            }

            Node n = this.Controls[_selectedIndex] as Node;
            n.DoRemovingAnimation();
            this.Controls.RemoveAt(_selectedIndex);

            RefreshSubControls();
            Invalidate();

            OnContentChanged(null, null);
        }

        protected virtual void OnContentChanged(object sender, EventArgs e)
        {
            if (ContentChanged != null)
                ContentChanged(sender, null);
            _path.Clear();

            RefreshMatrix();
        }
        protected virtual void OnSeletedNodeChanged(object sender, EventArgs e)
        {
            if (SelectedNodeChanged != null)
                SelectedNodeChanged(sender, null);
        }

        #region Node Events

        void Node_MouseDown(object sender, MouseEventArgs e)
        {
            if (_ball != null && _ball.IsRunning)
                return;
            Node ctl = (Node)sender;
            if (e.Button == MouseButtons.Left)
            {
                if (_selectedIndex > 0)
                {
                    Node node = this.Controls[_selectedIndex] as Node;
                    node.Selected = false;
                    node.Invalidate();
                }

                this._selectedIndex = ctl.Index;
                OnSeletedNodeChanged(sender, null);

                ctl.Selected = true;
                ctl.Invalidate();

                if (this.Tool == DrawingTools.Select || this.Tool == DrawingTools.Node)
                    _p = e.Location;
                else if (this.Tool == DrawingTools.Edge)
                {
                    _p = this.PointToClient((ctl.PointToScreen(e.Location)));
                    _startIndex = ctl.Index;
                    _startPoint = ctl.Location;
                }
                else if (this.Tool == DrawingTools.Eraser)
                {
                    DeleteSelectedNode();
                }
            }
        }

        void Node_MouseMove(object sender, MouseEventArgs e)
        {
            if (_ball != null && _ball.IsRunning)
                return;
            Control ctl = (Control)sender;

            if (e.Button == MouseButtons.Left)
            {

                Point p = this.PointToClient(ctl.PointToScreen(e.Location));
                if (this.Tool == DrawingTools.Select || this.Tool == DrawingTools.Node)
                {
                    if (p.X > 0 && p.Y > 0 && p.X < this.Width && p.Y < this.Height)
                    {
                        p.X -= _p.X;
                        p.Y -= _p.Y;

                        ctl.Location = p;

                        Invalidate();
                    }
                }

                else if (this.Tool == DrawingTools.Edge)
                {
                    Point p2 = this.PointToClient(ctl.PointToScreen(e.Location));
                    using (Graphics g = this.CreateGraphics())
                    {
                        g.DrawLine(Pens.Red, _p, p2);
                        Invalidate();
                    }
                }
            }
        }

        private void Node_MouseUp(object sender, MouseEventArgs e)
        {
            if (_ball != null && _ball.IsRunning)
                return;
            Node ctl = (Node)sender;
            if (e.Button == MouseButtons.Left)
            {
                if (this.Tool == DrawingTools.Edge)
                {
                    Point p2 = this.PointToClient(ctl.PointToScreen(e.Location));
                    Node node = this.GetChildAtPoint(p2) as Node;
                    if (node != null)
                    {                 
                        _edges.Add(new Edge(_startIndex, node.Index));
                    }
                }
                Invalidate();
                OnContentChanged(null, null);
            }
        }

        #endregion

        protected override void OnResize(EventArgs e)
        {
            Invalidate();
            base.OnResize(e);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int count = 0;
            foreach (var item in _edges)
            {
                Control ctl1 = this.Controls[item.Start];
                Control ctl2 = this.Controls[item.End];
                PointF p1 = ctl1.Location;
                PointF p2 = ctl2.Location;

                DrawEdge(g, item, p1, p2, count == _edges.SelectedIndex);
                count++;
            }
            if (_ball != null)
                _ball.Draw(e.Graphics);

            g.DrawRectangle(Pens.Black, 0, 0, this.Width - 1, this.Height - 1);

            base.OnPaint(e);
        }


        void DrawEdge(Graphics g, Edge item, PointF p1, PointF p2, bool selected)
        {
            string distance = GetDistance(Point.Round(p1), Point.Round(p2)).ToString();

            p1.X += NODE_RADIUS;
            p1.Y += NODE_RADIUS;
            p2.X += NODE_RADIUS;
            p2.Y += NODE_RADIUS;

            Vector2D v1 = new Vector2D(p1.X - p2.X, p1.Y - p2.Y);
            if (v1.Length - NODE_RADIUS > 0)
            {
                v1.Contract(NODE_RADIUS);
                p1.X = p2.X + v1.X;
                p1.Y = p2.Y + v1.Y;
            }
            Vector2D v = new Vector2D(p2.X - p1.X, p2.Y - p1.Y);
            if (v.Length - NODE_RADIUS > 0)
            {
                v.Contract(NODE_RADIUS);
                p2.X = p1.X + v.X;
                p2.Y = p1.Y + v.Y;
            }
            if (!IsUndirectedGraph && item.IsUndirected)
            {
                _penEdge.StartCap = LineCap.ArrowAnchor;
            }
            else
                _penEdge.StartCap = LineCap.NoAnchor;

            if (item.IsRemoving)
            {
                Pen p = new Pen(Color.Red, 4);
                g.DrawLine(p, p1, p2);

            }
            else
            {

                if (_path != null && (_path.Contains(new Point(item.Start, item.End)) || _path.Contains(new Point(item.End, item.Start))))
                {
                    Pen p = (Pen)_penEdge.Clone();
                    p.Color = Color.Green;
                    p.DashStyle = DashStyle.Dash;
                    g.DrawLine(p, p1, p2);
                }
                else if (selected)
                {
                    var hPen = (Pen)_penEdge.Clone();
                    hPen.Color = Color.Red;
                    g.DrawLine(hPen, p1, p2);
                }
                else
                    g.DrawLine(_penEdge, p1, p2);
            }


            // draw distance
            SizeF size = g.MeasureString(distance, this.Font);
            PointF pp = p1;
            pp.X += p2.X;
            pp.Y += p2.Y;
            pp.X /= 2;
            pp.Y /= 2;
            pp.X -= size.Width / 2;
            pp.Y -= size.Height / 2;
            g.FillEllipse(Brushes.Yellow, pp.X - 5, pp.Y - 5, size.Width + 10, size.Height + 5);
            g.DrawString(distance.ToString(), this.Font, Brushes.Blue, pp);
        }

    }
}
