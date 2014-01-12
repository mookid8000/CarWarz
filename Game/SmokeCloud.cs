using System;
using Lab06_2;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Lab05
{
	/// <summary>
	/// Summary description for SmokeCloud.
	/// </summary>
	public class SmokeCloud : IDrawableObject, IHittableObject
	{
		private int m_ticksReceived;
		private float m_radius;
		private PointF m_position;
		private bool m_antiAliasMode;
		private Random m_rnd;
		private float m_maxRadius;
		private float m_deltaRadius;
		private int m_alphaValue;

		public SmokeCloud(PointF position)
		{
			m_rnd = new Random();

			m_ticksReceived = 0;
			m_radius = 0;
			
			m_position.X = position.X + (m_rnd.Next(1,50) * 0.1f);
			m_position.Y = position.Y + (m_rnd.Next(1,50) * 0.1f);

			m_maxRadius = m_rnd.Next(5,15);
			m_alphaValue = m_rnd.Next(5, 20);

			m_deltaRadius = 0.2f;
			
			setAntialias(true);
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Interface: IDrawableObject

		public void tick()
		{
			m_ticksReceived++;

            m_radius += m_deltaRadius;

			if (m_radius >= m_maxRadius) 
			{
				m_deltaRadius = -m_deltaRadius;
				m_radius = m_maxRadius;
			}
		}

		public void draw(System.Drawing.Graphics g)
		{
			Brush smokeBrush1 = new SolidBrush( Color.FromArgb(m_alphaValue, 0,0,0) );
//			Brush smokeBrush2 = new SolidBrush( Color.FromArgb((int)(m_alphaValue*0.5f), 0,0,0) );
//			Brush smokeBrush3 = new SolidBrush( Color.FromArgb((int)(m_alphaValue*0.2f), 0,0,0) );

			if (m_antiAliasMode) 
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
			}

			g.FillEllipse( smokeBrush1, m_position.X - m_radius, m_position.Y - m_radius, m_radius*2, m_radius*2 );
//			g.FillEllipse( smokeBrush2, m_position.X - (int)(m_radius*0.5f), (int)(m_position.Y - m_radius*0.5f), (int)m_radius, (int)m_radius );
//			g.FillEllipse( smokeBrush3, m_position.X - m_radius*2, m_position.Y - m_radius*2, m_radius*4, m_radius*4 );
		}

		public void setAntialias(bool mode)
		{
			m_antiAliasMode = mode;
		}


		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Interface: IHittableObject

		public bool hits(Car car)
		{
			// TODO:  Add SmokeCloud.hits implementation
			return false;
		}

		public int getPower()
		{
			// TODO:  Add SmokeCloud.getPower implementation
			return 0;
		}

		public bool isOutOfBounds(Rectangle bounds)
		{
			// out of bounds for this smoke cloud means that m_radius is under 0
			return m_radius < 0;
		}

		public bool hits(CWLevel level) 
		{
			// a smoke cloud can not hit stuff
			return false;
		}
	}
}
