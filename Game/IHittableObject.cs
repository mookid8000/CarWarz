using System;
using Lab06_2;
using System.Drawing;

namespace Lab05
{
	interface IHittableObject
	{
		bool	hits(Car car);
		int		getPower();
		bool	isOutOfBounds(Rectangle bounds);
		bool	hits(CWLevel level);
	}
}
