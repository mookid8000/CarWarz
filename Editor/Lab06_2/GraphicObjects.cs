using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Lab06_2
{
	[Serializable]
	public class CWLine : IGraphicObject
	{
		private PointF m_startingPoint, m_endingPoint;
		private Color m_color;
		private bool m_antialiasing = false;

		public CWLine(PointF start, PointF end) 
		{
			m_startingPoint = start;
			m_endingPoint = end;

			m_color = Color.FromArgb(190, 0,180,0);
		}

		public void draw(Graphics g)
		{
			Pen obstaclePen = new Pen(m_color, 5);

			if (m_antialiasing) 
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
			}

			g.DrawLine(obstaclePen, m_startingPoint, m_endingPoint);
		}

		public void setEndpoint(PointF end) 
		{
			m_endingPoint = end;
		}

		public void setAntialias(bool mode) 
		{
			m_antialiasing = mode;
		}

		public bool intersectsWith(RectangleF rect) 
		{
			return false;
		}
	}

	[Serializable]
	public class CWRectangle : IGraphicObject
	{
		private RectangleF m_rectangle;
		private Color m_color;
		private bool m_antialiasing = false;

		public CWRectangle(RectangleF rect) 
		{
			m_rectangle = rect;

			m_color = Color.FromArgb(190, 0,180,0);
		}

		public CWRectangle(RectangleF rect, Color color) 
		{
			m_rectangle = rect;
			m_color = color;
		}

		public void draw(Graphics g)
		{
			Brush obstacleBrush = new SolidBrush(m_color);

			if (m_antialiasing) 
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
			}

			g.FillRectangle(obstacleBrush, m_rectangle);
		}

		public void setEndpoint(PointF end) 
		{
			m_rectangle.Width = end.X - m_rectangle.X;
			m_rectangle.Height = end.Y - m_rectangle.Y;
		}

		public void setAntialias(bool mode) 
		{
			m_antialiasing = mode;
		}

		public bool intersectsWith(RectangleF rect) 
		{
			return m_rectangle.IntersectsWith(rect);
		}
	}

	[Serializable]
	public class CWEllipsis : IGraphicObject
	{
		private RectangleF m_rectangle;
		private Color m_color;
		private bool m_antialiasing = false;

		public CWEllipsis(RectangleF rect) 
		{
			m_rectangle = rect;

			m_color = Color.FromArgb(190, 0,180,0);
		}

		public void draw(Graphics g)
		{
			Brush obstacleBrush = new SolidBrush(m_color);

			if (m_antialiasing) 
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
			}

			g.FillEllipse(obstacleBrush, m_rectangle);
		}

		public void setEndpoint(PointF end) 
		{
			m_rectangle.Width = end.X - m_rectangle.X;
			m_rectangle.Height = end.Y - m_rectangle.Y;
		}

		public void setAntialias(bool mode) 
		{
			m_antialiasing = mode;
		}

		public bool intersectsWith(RectangleF rect) 
		{
			return m_rectangle.IntersectsWith(rect);
		}
	}

}
