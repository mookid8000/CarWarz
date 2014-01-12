using System;
using Lab06_2;
using System.Drawing;
using System.Collections;
using System.Drawing.Drawing2D;

namespace Lab05
{
	/// <summary>
	/// This class represents a rocket, fired by the car
	/// It has the ability to draw itself and receive ticks in the same manner as the car
	/// </summary>
	public class Rocket : IDrawableObject, IHittableObject
	{		
		private PointF m_position, m_deltaPosition, m_startPosition;

		// every rocket must remember who fired it
		private Car m_owner;
		private Color m_color1, m_color2;
		private int m_ticksReceived;

		private Random rnd;
		private ArrayList m_sceneObjectsList;

		// remember if we must antialias gfx
		private bool m_antiAliasMode;

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


		// create a shot (Rocket needs to be able to add smoke clouds to the scene, hence the
		// sceneObjectsList ArrayList reference)
		public Rocket(PointF position, float angle, float speed, Car owner, ArrayList sceneObjectsList)
		{
			m_deltaPosition.X = speed * (float)Math.Cos( -angle*2*Math.PI/360 );
			m_deltaPosition.Y = speed * (float)Math.Sin( -angle*2*Math.PI/360 );

			setAntialias(true);

			m_position.X = position.X + m_deltaPosition.X*2;
			m_position.Y = position.Y + m_deltaPosition.Y*2;

			m_owner = owner;
			m_ticksReceived = 0;

			m_startPosition.X = position.X + m_deltaPosition.X*3;
			m_startPosition.Y = position.Y + m_deltaPosition.Y*3;

			rnd = new Random();

			// remember reference to list of scene objects
			m_sceneObjectsList = sceneObjectsList;
		}


		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Interface: IHittableObject

		// test if this shot hits the car passed to it - if the car in question is the owner of this
		// shot, false is returned
		public bool hits(Car car) 
		{
			bool rocketHitsCar = false;

			// if this shot is owned by the car we are to test, we just return false
			if (car == m_owner) return false;

			rocketHitsCar = car.isHitByRocket(this);

			if (rocketHitsCar) 
			{
				for (int t=0; t<50; t++) 
				{
					PointF smokePosition = new PointF( rnd.Next(-20, 20), rnd.Next(-20, 20) );
					SmokeCloud smokeCloud = new SmokeCloud( new PointF(m_position.X + smokePosition.X,m_position.Y + smokePosition.Y) );

					smokeCloud.setAntialias( m_antiAliasMode );
					m_sceneObjectsList.Add(smokeCloud);
				}
			}
			return rocketHitsCar;
		}

		// test if this shot is out of the bounds passed to this function
		public bool isOutOfBounds(Rectangle bounds) 
		{
			return !bounds.Contains( new Point((int)m_position.X, (int)m_position.Y) );
		}

		public int getPower() 
		{
			return (30 + rnd.Next(0,30));
		}

		public bool hits(CWLevel level) 
		{
			bool nowWeHitTheLevelObstaclesAndMustDie = false;
			
			if (null != level) 
			{
				nowWeHitTheLevelObstaclesAndMustDie = level.intersectsWith(new RectangleF( m_position.X-1, m_position.Y-1, 2, 2 ) );
			}

			if (nowWeHitTheLevelObstaclesAndMustDie) 
			{
				for (int t=0; t<50; t++) 
				{
					PointF smokePosition = new PointF( rnd.Next(-20, 20), rnd.Next(-20, 20) );
					SmokeCloud smokeCloud = new SmokeCloud( new PointF(m_position.X + smokePosition.X,m_position.Y + smokePosition.Y) );

					smokeCloud.setAntialias( m_antiAliasMode );
					m_sceneObjectsList.Add(smokeCloud);
				}
			}

			return nowWeHitTheLevelObstaclesAndMustDie;
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Interface: IDrawableObject

		// this shot knows how to draw itself
		public void draw(Graphics g) 
		{
			Brush	shotBrush = new SolidBrush( Color.FromArgb(150, 0,0,0 ) );
			Pen		shotPen1 = new Pen( m_color1, 5 );
			Pen		shotPen2 = new Pen( m_color1, 3 );
			Brush	smokeBrush = new SolidBrush( m_color2 );

			if (m_antiAliasMode) 
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
			}

			int smokeRadius = Math.Min(12, 12-m_ticksReceived);

			g.FillEllipse(shotBrush, m_position.X - 3, m_position.Y - 3, 6, 6);
			g.DrawLine(shotPen1, m_position.X, m_position.Y, m_position.X - 3.0f*m_deltaPosition.X, m_position.Y - 3.0f*m_deltaPosition.Y);
			g.DrawLine(shotPen2, m_position.X, m_position.Y, m_position.X - 5.0f*m_deltaPosition.X, m_position.Y - 5.0f*m_deltaPosition.Y);

			g.FillEllipse( smokeBrush, m_startPosition.X-smokeRadius, m_startPosition.Y-smokeRadius, 2*smokeRadius, 2*smokeRadius);
			g.FillEllipse( smokeBrush, m_startPosition.X-0.5f*smokeRadius, m_startPosition.Y-0.5f*smokeRadius, smokeRadius, smokeRadius);
		}

		// give this shot a tick, so it updates its position and color
		public void tick() 
		{
			m_ticksReceived ++;

			m_position.X += m_deltaPosition.X;
			m_position.Y += m_deltaPosition.Y;

			m_color1 = Color.FromArgb(
				Math.Min(3*m_ticksReceived, 180),
				Math.Min(5*m_ticksReceived, 255),
				Math.Min(m_ticksReceived, 255),
				Math.Min(m_ticksReceived, 255)
			);

			m_color2 = Color.FromArgb( Math.Max(70-3*m_ticksReceived, 0), 0,0,0 );

			// add smoke sometimes
			if (rnd.Next(1,5) == 1) 
			{
				SmokeCloud smokeCloud = new SmokeCloud(m_position);
				smokeCloud.setAntialias( m_antiAliasMode );
				m_sceneObjectsList.Add( smokeCloud );
			}
		}

		public void setAntialias(bool mode) 
		{
			m_antiAliasMode = mode;
		}
	}
}
