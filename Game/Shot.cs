using System;
using Lab06_2;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Lab05
{
	/// <summary>
	/// This class represents a shot, fired by the car
	/// It has the ability to draw itself and receive ticks in the same manner as the car
	/// </summary>
	public class Shot : IDrawableObject, IHittableObject
	{		
		private PointF m_position, m_deltaPosition, m_startPosition;

		// every shot must remember who fired it
		private Car m_owner;
		private Color m_color1, m_color2;
		private int m_ticksReceived;
		private bool m_antiAliasMode;
		private Random rnd;

		public Car Owner 
		{
			get 
			{
				return m_owner;
			}
		}

		public Point Position 
		{
			get
			{
				return new Point((int)m_position.X, (int)m_position.Y);
			}
		}

		public Point DeltaPosition 
		{
			get 
			{
				return new Point((int)m_deltaPosition.X, (int)m_deltaPosition.Y);
			}
		}


		// create a shot
		public Shot(PointF position, float angle, float speed, Car owner)
		{
			m_deltaPosition.X = speed * (float)Math.Cos( -angle*2*Math.PI/360 );
			m_deltaPosition.Y = speed * (float)Math.Sin( -angle*2*Math.PI/360 );

			m_position.X = position.X + m_deltaPosition.X*2;
			m_position.Y = position.Y + m_deltaPosition.Y*2;

			m_owner = owner;
			m_ticksReceived = 0;

			m_startPosition.X = position.X + m_deltaPosition.X*3;
			m_startPosition.Y = position.Y + m_deltaPosition.Y*3;

			setAntialias(true);

			rnd = new Random();
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Interface: IHittableObject

		// test if this shot hits the car passed to it - if the car in question is the owner of this
		// shot, false is returned
		public bool hits(Car car) 
		{
			// if this shot is owned by the car we are to test, we just return false
			if (car == m_owner) return false;

			return car.isHitByShot(this);
		}

		// test if this shot is out of the bounds passed to this function
		public bool isOutOfBounds(Rectangle bounds) 
		{
			return !bounds.Contains( new Point((int)m_position.X, (int)m_position.Y) );
		}

		public int getPower() 
		{
			return (10 + rnd.Next(0,10));
		}

		public bool hits(CWLevel level) 
		{
			bool shotHitsLevelObstacle = false;

			if (null != level) 
			{
				shotHitsLevelObstacle = level.intersectsWith(new RectangleF( m_position.X-1, m_position.Y-1, 2, 2 ) );
			}

			return shotHitsLevelObstacle;
		}
		
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Interface: IDrawableObject
		// give this shot a tick, so it updates its position and color

		public void tick() 
		{
			m_ticksReceived ++;

			m_position.X += m_deltaPosition.X;
			m_position.Y += m_deltaPosition.Y;

			m_color1 = Color.FromArgb( Math.Min(3*m_ticksReceived, 100), Math.Min(4*m_ticksReceived, 255),Math.Min(4*m_ticksReceived, 255), Math.Min(2*m_ticksReceived, 255));
			m_color2 = Color.FromArgb( Math.Max(70-3*m_ticksReceived, 0), 0,0,0 );
		}

		// this shot knows how to draw itself
		public void draw(Graphics g) 
		{
			Brush	shotBrush = new SolidBrush( Color.FromArgb(150, 0,0,0 ) );
			Pen		shotPen = new Pen( m_color1, 3 );
			Brush	smokeBrush = new SolidBrush( m_color2 );

			if (m_antiAliasMode) 
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
			}

			int smokeRadius = Math.Min(12, 12-m_ticksReceived);

			g.FillEllipse(shotBrush, m_position.X - 2, m_position.Y - 2, 4, 4);
			g.DrawLine(shotPen, m_position.X, m_position.Y, m_position.X - 1.5f*m_deltaPosition.X, m_position.Y - 1.5f*m_deltaPosition.Y);

			g.FillEllipse( smokeBrush, m_startPosition.X-smokeRadius, m_startPosition.Y-smokeRadius, 2*smokeRadius, 2*smokeRadius);
			g.FillEllipse( smokeBrush, m_startPosition.X-0.5f*smokeRadius, m_startPosition.Y-0.5f*smokeRadius, smokeRadius, smokeRadius);
		}

		public void setAntialias(bool mode) 
		{
			m_antiAliasMode = mode;
		}
	}
}
