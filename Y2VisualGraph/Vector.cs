using System;


namespace Y2VisualGraph
{
	public class Vector2D
	{
		public float X{ get;set;}
		public float Y{get;set;}

		public float Length
		{
			get { return (float) Math.Sqrt(X*X + Y*Y); }
		}
		public Vector2D(float x, float y)
		{
			X = x;
			Y = y;
		}

		public void Contract(float size)
		{
			if(this.Length>0)
			{
				float d=size/this.Length;
				this.X -= (this.X*d);
				this.Y -= (this.Y*d);
			}
		}

        public void Normalize(){
            this.X /= this.Length;
            this.Y /= this.Length;
        }
	}
}
