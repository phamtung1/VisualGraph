using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Y2VisualGraph
{
    class MovingBall
    {
        private const int SPEED = 5;
        private System.Windows.Forms.Timer timer;
        int Index;
        public PointF StartPos;
        public PointF EndPos;
        float _dx;
        float _dy;
        public List<Point> NodeLocations;
        Control _parent;
        bool blink;
        public bool IsRunning
        {
            get { return timer != null && timer.Enabled ; }
        }
        public MovingBall(Control parent)
        {
            NodeLocations = new List<Point>();
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 10;
            timer.Tick += new EventHandler(timer_Tick);
            _parent = parent;
        }
        public void Reset()
        {
            NodeLocations.Clear();
        }
        public void Start()
        {
            this.Index = 0;

            StartPos = this.NodeLocations[Index];
            StartPos.X += GraphUI.NODE_RADIUS;
            StartPos.Y += GraphUI.NODE_RADIUS;

            Prepare();
            timer.Start();
        }
        public void Stop()
        {
            timer.Stop();
        }
        void Prepare()
        {
            if (Index <NodeLocations.Count)
            {
                EndPos = this.NodeLocations[Index];
                EndPos.X += GraphUI.NODE_RADIUS;
                EndPos.Y += GraphUI.NODE_RADIUS;
                _dx = EndPos.X - StartPos.X;
                _dy = EndPos.Y - StartPos.Y;

                var length = (float)Math.Sqrt(_dx * _dx + _dy * _dy);
                _dx = _dx / length * SPEED;
                _dy = _dy / length * SPEED;
            }
            else
            {
                timer.Stop();
                StartPos = Point.Empty;
            }
        }
        void timer_Tick(object sender, EventArgs e)
        {
            if ((_dx < 0 && StartPos.X > EndPos.X) || (_dx > 0 && StartPos.X < EndPos.X))
            {
                StartPos.X+=_dx;
                StartPos.Y+=_dy;
            }
            else
            {
                StartPos = EndPos;
                Index++;
                Prepare();
            }
            _parent.Invalidate();
        }
        public void Draw(Graphics g)
        {
            if (StartPos != Point.Empty)
            {
                if (blink)
                    g.FillEllipse(Brushes.Red, StartPos.X, StartPos.Y, 10, 10);
                else
                    g.FillEllipse(Brushes.Blue, StartPos.X, StartPos.Y, 10, 10);

                blink = !blink;
            }
        }

    }
}
