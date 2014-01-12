using System;
using Lab06_2;
using System.Drawing;
using System.Collections;
using System.Drawing.Text;
using System.Drawing.Drawing2D;

namespace Lab05
{
	/// <summary>
	/// Represents a car, complete with position and the ability to draw itself to a Graphics context
	/// </summary>
	public class Car : IDrawableObject
	{
		// the car remembers its min and max speed
		private float m_maxSpeed, m_minSpeed, m_angleStep, m_speedThreshold;

		// position of the car
		private PointF m_position;

		// position to add for every call to tick
		private PointF m_deltaPosition;

		private Rectangle m_carSpan1, m_carSpan2;

		// flag to remember if this car is dead
		private bool m_dieFlag = false;
		private bool m_addedDyingSmoke = false;

		// counter to remember the number of ticks received after death - this one
		// is used to control the animation of the smoke and fire
		private int m_dieTicksAfterDie = 0;

		// the movement is specified by an angle and a speed and m_deltaPosition is calculated from these
		private float m_angle, m_speed;

		// remember if front tires must be turned
		private int m_turnDirection;

		// randomness thing to use when hit by rocket
		private Random rnd;

		// remember reference to scene objects list, so we can add smoke to it some time
		private ArrayList m_sceneObjectsList;

		private Color m_color;
		private string m_carNumber;

		private bool m_showBoundingBox = false;

		// counter to track if we are ready to shoot
		private int m_shooterCounter, m_shooterCounterInit = 8;

		// this car has some energy
		private int m_energy;

		// remember if we must antialias gfx
		private bool m_antiAliasMode;

		// remember reference to level obstacles
		private CWLevel m_level;

		// Properties
		public PointF Position 
		{
			set 
			{
				m_position = value;
			}

			get 
			{
				return m_position;
			}
		}

		public float Angle 
		{
			set 
			{
				m_angle = value % 360;
				calculateDeltaPosition();
			}

			get 
			{
				return m_angle;
			}
		}

		public float Speed 
		{
			set
			{
				m_speed = limit(m_minSpeed, m_maxSpeed, value);
				calculateDeltaPosition();
			}

			get 
			{
				return m_speed;
			}
		}

		public int Energy 
		{
			set 
			{
				m_energy = Math.Max( Math.Min(value, 100), 0 );;
			}

			get 
			{
				return m_energy;
			}
		}

		public void showBoundingBox(bool val) 
		{
			m_showBoundingBox = val;
		}

		public Car(string carNumber, Color color, ArrayList sceneObjectsList)
		{
			// remember reference
			m_sceneObjectsList = sceneObjectsList;

			// initialize everything
			m_minSpeed = -2;
			m_maxSpeed = 4;

			setAntialias(true);

			m_shooterCounter = 0;

			m_angleStep = 1.2f;
			m_speedThreshold = 0.3f;

			Position = new PointF(0,0);
			Angle = 0;
			Speed = 0;

			m_turnDirection = 0;
			
			calculateDeltaPosition();

			m_color = color;
			m_carNumber = carNumber;

			m_energy = 100;

			// create two rectangles (X and Y coordinates will be set by tick() - the important
			// thing right now is just the size
			m_carSpan1 = new Rectangle(0,0, 22, 22);
			m_carSpan2 = new Rectangle(0,0, 22, 22);

			rnd = new Random();
		}

		public void setLevel(CWLevel level) 
		{
			m_level = level;
		}

		// we can only shoot if the shooter counter is below zero and we are not dead
		public bool canShoot() 
		{
			return (m_shooterCounter < 0 && !m_dieFlag);
		}

		// this function should be called when a shot is fired
		public void fireShot() 
		{
			m_shooterCounter = m_shooterCounterInit;
		}

		public void fireRocket()
		{
			m_shooterCounter = m_shooterCounterInit * 4;
		}

		// private utility function to return val limited by the specified min and max values
		private float limit(float min, float max, float val)
		{
			return Math.Max( Math.Min(val, max), min );
		}

		// private utility function to calculate the delta PointF values used in tick()
		private void calculateDeltaPosition() 
		{
			// we need to act differently depending on direction of car
			if (m_speed < 0) 
			{
				if ( Math.Abs(m_speed) > m_speedThreshold) 
				{
					m_deltaPosition.X = (m_speed+m_speedThreshold) * (float)Math.Cos( -m_angle*2*Math.PI/360 );
					m_deltaPosition.Y = (m_speed+m_speedThreshold) * (float)Math.Sin( -m_angle*2*Math.PI/360 );
				} 
				else 
				{
					m_deltaPosition.X = 0;
					m_deltaPosition.Y = 0;
				}
			} 
			else 
			{
				if ( Math.Abs(m_speed) > m_speedThreshold) 
				{
					m_deltaPosition.X = (m_speed-m_speedThreshold) * (float)Math.Cos( -m_angle*2*Math.PI/360 );
					m_deltaPosition.Y = (m_speed-m_speedThreshold) * (float)Math.Sin( -m_angle*2*Math.PI/360 );
				} 
				else 
				{
					m_deltaPosition.X = 0;
					m_deltaPosition.Y = 0;
				}
			}
		}

		// call this to turn left
		public void turnLeft() 
		{
			if ( Math.Abs(m_speed) > m_speedThreshold) 
			{
				Angle += m_angleStep * (m_speed-m_speedThreshold);
			}
		}

		// call this to turn right
		public void turnRight() 
		{
			if ( Math.Abs(m_speed) > m_speedThreshold) 
			{
				Angle -= m_angleStep * (m_speed-m_speedThreshold);
			}

		}

		// should be called whenever car has moved or turned to update the car spac rectangles
		private void updateCarSpan() 
		{
			m_carSpan1.X = (int)m_position.X - 11;
			m_carSpan1.Y = (int)m_position.Y - 11;

			m_carSpan2.X = (int)(m_position.X + 15 * Math.Cos(-m_angle*2*Math.PI/360)) - 11;
			m_carSpan2.Y = (int)(m_position.Y + 15 * Math.Sin(-m_angle*2*Math.PI/360)) - 11;
		}

		// call this function to accelerate
		public void accelerate() 
		{
			Speed += 0.1f;
		}

		// call this function to decelerate
		public void decelerate() 
		{
			Speed -= 0.1f;
		}

		// call this function to turn tires of car
		public void tiresTurnLeft() 
		{
			m_turnDirection = 1;
		}

		// call this function to turn tires of car
		public void tiresTurnRight() 
		{
			m_turnDirection = -1;
		}

		// call this function to straighten tires of car
		public void tiresStraight() 
		{
			m_turnDirection = 0;
		}

		// test if this car is hit by the shot passed to this function
		public bool isHitByShot(Shot shot) 
		{
			if ( (m_carSpan1.Contains(shot.Position) || m_carSpan2.Contains(shot.Position)) && !m_dieFlag) 
			{
				m_position.X += 0.10f * shot.DeltaPosition.X;
				m_position.Y += 0.10f * shot.DeltaPosition.Y;

				return true;
			} 
			else 
			{
				return false;
			}
		}

		// test if this car is hit by the rocket passed to this function
		public bool isHitByRocket(Rocket rocket) 
		{
			if ( (m_carSpan1.Contains(rocket.Position) || m_carSpan2.Contains(rocket.Position)) && !m_dieFlag) 
			{
				m_position.X += rocket.DeltaPosition.X;
				m_position.Y += rocket.DeltaPosition.Y;

				m_speed *= 0.2f;

				m_angle += rnd.Next(-10,10);


				return true;
			} 
			else 
			{
				return false;
			}
		}

		// separate function for impact
		private void doCarImpact(Car hitter, Car hittee) 
		{
			// the hittee has its position altered by the impact vector = hitter's movement vector
			hittee.m_position.X += hitter.m_deltaPosition.X;
			hittee.m_position.Y += hitter.m_deltaPosition.Y;

			// if the bounding rectangles overlap after this function is run, they will probably stay overlapped for a long time

			// position of hitter has its own delta position subtracted+a tiny bit extra, to make
			// sure there is no overlap between hitter's and hittee's bounding rectangles
			hitter.m_position.X -= 1.5f*hitter.m_deltaPosition.X;
			hitter.m_position.Y -= 1.5f*hitter.m_deltaPosition.Y;

			hittee.Speed *= 0.7f;
		}

		// test if this car somehow intersects the car passed to the function (only applies if both cars are non-dead)
		public bool crashesWith(Car otherCar) 
		{
			if ( (m_carSpan1.IntersectsWith(otherCar.m_carSpan1)
				|| m_carSpan1.IntersectsWith(otherCar.m_carSpan2)
				|| m_carSpan2.IntersectsWith(otherCar.m_carSpan1)
				|| m_carSpan2.IntersectsWith(otherCar.m_carSpan2))
				&& !m_dieFlag && !otherCar.m_dieFlag ) 
			{
				doCarImpact(this, otherCar);
				doCarImpact(otherCar, this);

				return true;
			} 
			else 
			{
				return false;
			}
		}

		// return if this car is dead - the car is not completely dead until the explosion is over
		public bool isDead() 
		{
			return m_dieFlag && m_dieTicksAfterDie > 250;
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Interface: IDrawableObject

		// tick-thingie to update position of car and some other stuff
		public void tick() 
		{
			m_dieFlag = m_energy <= 0;

			// if we have died, and we did not add the dying smoke: do it!
			if (m_dieFlag)
			{
				if (!m_addedDyingSmoke)
				{
					m_addedDyingSmoke = true;

					for (int t=0; t<120; t++) 
					{
						PointF smokePosition = new PointF( rnd.Next(-150, 150), rnd.Next(-150, 150) );
						SmokeCloud smokeCloud = new SmokeCloud( new PointF(m_position.X + smokePosition.X,m_position.Y + smokePosition.Y) );

						smokeCloud.setAntialias( m_antiAliasMode );
						m_sceneObjectsList.Add(smokeCloud);
					}
				}

				if (m_dieTicksAfterDie < 150) 
				{
					PointF smokePosition = new PointF( rnd.Next(-150+m_dieTicksAfterDie, 150-m_dieTicksAfterDie), rnd.Next(-150+m_dieTicksAfterDie, 150-m_dieTicksAfterDie) );
					SmokeCloud smokeCloud = new SmokeCloud( new PointF(m_position.X + smokePosition.X,m_position.Y + smokePosition.Y) );

					smokeCloud.setAntialias( m_antiAliasMode );
					m_sceneObjectsList.Add(smokeCloud);
				}
			}

			m_shooterCounter--;

			if (m_dieFlag) 
			{
				m_dieTicksAfterDie ++;

				// magic constant to make the fire cloud slow down instead of just keeping on moving
				m_speed *= 0.97f;

				calculateDeltaPosition();
			}

			m_position.X += m_deltaPosition.X;
			m_position.Y += m_deltaPosition.Y;

			updateCarSpan();

			if (null != m_level) 
			{
				// if we intersect with a level object we reverse the movement
				if ( m_level.intersectsWith(m_carSpan1) || m_level.intersectsWith(m_carSpan2) )
				{
					// subtract previous movement + some extra
					m_position.X -= 1.2f * m_deltaPosition.X;
					m_position.Y -= 1.2f * m_deltaPosition.Y;

					updateCarSpan();

					// simulate loss of speed due to impact
					m_speed *= 0.8f;
				}
			}
		}

		// draw method - this car knows how it wants to look
		// - it takes care of the animation as well
		public void draw(Graphics g) 
		{
			Pen		chassisPen = Pens.Black;
			Brush	tyreBrush = Brushes.Black;
			Pen		housePen = new Pen( m_color, 2);
			Font	numberFont = new Font("Arial",11, FontStyle.Bold);
			Brush	numberBrush = new SolidBrush( m_color );
			Brush	smokeBrush = new SolidBrush( Color.FromArgb(30, 0,0,0) );
			Brush	fireBrush1 = new SolidBrush( Color.FromArgb(200, 255,255,0) );

			// transform the world so that the middle of the car will be in origo
			g.TranslateTransform(m_position.X, m_position.Y);

			// we rotate by the negated angle so positive angles correspond to rotation on the unit circle
			g.RotateTransform(-m_angle);
			
			if (m_antiAliasMode) 
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
			}

			// only draw car if not dying
			if (!m_dieFlag) 
			{
				// draw chassis
				g.DrawRectangle(chassisPen, -10, -8, 35, 16);

				// draw back tires
				g.FillRectangle(tyreBrush, -5, -12, 6, 4);	//< back tires
				g.FillRectangle(tyreBrush, -5, 8, 6, 4);

				// house
				g.DrawRectangle(housePen, -5, -5, 20, 10);
				//g.DrawLine(housePen, -5, -5, 15, 5);
				//g.DrawLine(housePen, -5, 5, 15, -5);
				g.TextRenderingHint = TextRenderingHint.AntiAlias ;
				g.RotateTransform(90);
				g.DrawString(m_carNumber, numberFont, numberBrush, new Rectangle(-6, -11, 26, 25));
				g.RotateTransform(-90);

				// draw cannon
				g.DrawRectangle(chassisPen, 10, -2, 8, 4);
				g.DrawRectangle(chassisPen, 11, -1, 6, 2);

				// draw front tires
				if ( m_turnDirection == 0) 
				{
					g.FillRectangle(tyreBrush, 13, -12, 6, 4);	//< front tires
					g.FillRectangle(tyreBrush, 13, 8, 6, 4);
				} 
				else if ( m_turnDirection == 1 ) 
				{
					g.TranslateTransform(13, -11);
					g.RotateTransform(-20);
					g.FillRectangle(tyreBrush, 0,0, 6, 4);
					g.RotateTransform(20);
					g.TranslateTransform(0, 20);
					g.RotateTransform(-20);
					g.FillRectangle(tyreBrush, 0,0, 6, 4);
				} 
				else if ( m_turnDirection == -1 ) 
				{
					g.TranslateTransform(14, -13);
					g.RotateTransform(20);
					g.FillRectangle(tyreBrush, 0,0, 6, 4);
					g.RotateTransform(-20);
					g.TranslateTransform(0, 20);
					g.RotateTransform(20);
					g.FillRectangle(tyreBrush, 0,0, 6, 4);
				}
			}

			g.ResetTransform();

			if (m_showBoundingBox) 
			{
				// draws rectangles to show the "sensitive areas" of the car
				g.DrawRectangle(Pens.Blue, m_carSpan1);
				g.DrawRectangle(Pens.Blue, m_carSpan2);
			}
		}

		public void setAntialias(bool mode) 
		{
			m_antiAliasMode = mode;
		}

	}
}
