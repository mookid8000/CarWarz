using System;
using System.Drawing;
using System.Collections;

namespace Lab06_2
{
	[Serializable]
	public class CWLevel
	{
		private ArrayList m_levelObjects;
		private bool m_antialiasingMode = false;

		public CWLevel()
		{
			// construct new list for level objects
			m_levelObjects = new ArrayList();
		}

		public void add(IGraphicObject obj) 
		{
			obj.setAntialias( m_antialiasingMode );
			m_levelObjects.Add(obj);
		}

		public void draw(Graphics g) 
		{
			foreach(IGraphicObject obj in m_levelObjects) 
			{
				obj.draw(g);
			}
		}

		public void setAntialias(bool mode) 
		{
			m_antialiasingMode = mode;
		}

		public bool intersectsWith(RectangleF rect) 
		{
			bool returnValue = false;

			foreach(IGraphicObject obj in m_levelObjects) 
			{
				returnValue |= obj.intersectsWith(rect);
			}

			return returnValue;
		}

		public void createLevelBounds(int width, int height) 
		{
			m_levelObjects.Add(	new CWRectangle(
					new RectangleF(		0,				0,			width,			40	),
					Color.FromArgb(20, 0,0,0)
				)
			);

			m_levelObjects.Add(	new CWRectangle(
					new RectangleF(		0,				height-60,	width,			40	),
					Color.FromArgb(20, 0,0,0)
				)
			);

			m_levelObjects.Add(	new CWRectangle(
					new RectangleF(		0,				40,			40,				height-100	),
					Color.FromArgb(20, 0,0,0)
				)
			);

			m_levelObjects.Add(	new CWRectangle(
					new RectangleF(		width-45,		40,			40,				height-100	),
					Color.FromArgb(20, 0,0,0)
				)
			);
		}
	}
}
