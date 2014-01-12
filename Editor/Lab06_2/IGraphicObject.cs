using System;
using System.Drawing;

namespace Lab06_2
{
	public interface IGraphicObject
	{
		// function to let object represent itself
		void draw(Graphics g);

		// function to tell object to adjust its proportions according to some end point
		void setEndpoint(PointF end);

		// allow graphics objects to have antialiasing set or not
		void setAntialias(bool mode);

		// implement intersection detection
		bool intersectsWith(RectangleF rect);
	}
}
