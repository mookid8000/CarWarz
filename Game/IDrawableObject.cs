using System;
using System.Drawing;

namespace Lab05
{
	interface IDrawableObject
	{
		void tick();
		void draw(Graphics g);
		void setAntialias(bool mode);
	}
}
