using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;

namespace Y2VisualGraph
{
    class Node : Control
    {
        public char DisplayName;
        public int Index { get; set; }
        public bool Selected;
        int _alpha = 0;
        bool _isRevoming;
        bool _isCreating;

        public Node()
        {
            this.Size = new Size(1, 1);
            
            this.BackColor = Color.Wheat;
        }
        public void DoCreatingAnimation()
        {
            this._isCreating = true;
            while (_alpha < 240)
            {
                this.Inflate(1);
                _alpha += 20;

                this.Refresh();
                Thread.Sleep(10);
            }
            this._isCreating = false;
            Invalidate();
        }
        public void Inflate(int size)
        {
           // if (this.Width + size < GraphUI.NODE_DIAMETER)
           //     return;

            Rectangle rect = this.Bounds;
            rect.Inflate(size, size);
            this.Bounds = rect;

            Invalidate();
        }
        public void DoRemovingAnimation()
        {
            this._isRevoming = true;
            while (_alpha > 0)
            {
                this.Inflate(1);
                _alpha -= 10;
                
                this.Refresh();
                Thread.Sleep(10);
            }
            this._isRevoming = false;
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Invalidate();
        }
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Brush foreBrush = new SolidBrush(this.ForeColor);
            //   base.OnPaint(e);
            string text = DisplayName.ToString();
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = this.ClientRectangle;
            rect.Inflate(-1, -1);
            if (_isCreating || _isRevoming)
            {
                Color c = Color.FromArgb(_alpha, Color.Red);

                e.Graphics.FillEllipse(new SolidBrush(c), rect);
            }
            else
            {
                Pen p = new Pen(this.ForeColor);
                if (this.Selected)
                {
                    p = new Pen(Color.Red, 3);
                }
                e.Graphics.DrawEllipse(p, rect);

            }
            rect.Inflate(1, 1);

            // draw text
            SizeF s = e.Graphics.MeasureString(text, this.Font);
            e.Graphics.DrawString(text, this.Font, foreBrush, (this.Width - s.Width) / 2, (this.Height - s.Height) / 2);

            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(rect);

            this.Region = new System.Drawing.Region(path);
        }
        public override string ToString()
        {
            return DisplayName.ToString();
        }
    }
}
