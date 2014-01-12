using System;
using Lab06_2;
using System.IO;
using System.Data;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;

namespace Lab05
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class CarWarzGame : System.Windows.Forms.Form
	{
		
		// define keys to be used for controlling the characters
		enum ControlKeys	// keyboard scancode
		{
			playerOneUp		= 38,	// up arrow
			playerOneDown	= 40,	// down arrow
			playerOneLeft	= 37,	// left arrow
			playerOneRight	= 39,	// right arrow
			playerOneFire1	= 16,	// shift key
			playerOneFire2	= 13,	// enter key

			playerTwoUp		= 87,	// W
			playerTwoDown	= 83,	// S
			playerTwoLeft	= 65,	// A
			playerTwoRight	= 68,	// D
			playerTwoFire1	= 17,	// CTRL
			playerTwoFire2	= 9,	// TAB

			unPauseKey		= 112,	// F1
			antialiasToggle = 113,	// F2
			pauseKey		= 121,	// F10
			quitKey			= 27,	// ESC
			loadKey			= 76	// L
		}

		// define a struct to track what keys are being depressed
		private struct Keys 
		{
			public struct Player
			{
				public bool upArrow;
				public bool downArrow;
				public bool leftArrow;
				public bool rightArrow;
				public bool fire1;
				public bool fire2;
			}

			public Player pOne, pTwo;
		}

		private Keys keysDown;

		// define states for the state machine used to control game flow
		enum GameState 
		{
			paused,
			waitingToBePaused,
			playing,
			waitingToStartPlaying,
			playerOneWins,
			playerTwoWins,
			gameOverPlayerOneWon,
			gameOverPlayerTwoWon,
			waitingToRestartGame,
			quitting
		}

		// state memory for the game control state machine
		private GameState currentState;

		// car references to hold the two players' cars
		private Car car1, car2;

		// timer to control time resolution in game
		private Timer gameTimerTick;

		// timer to control time resolution in game control
		private Timer gameControlTick;

		// arraylist to hold all the objects in the scene
		private ArrayList sceneObjects;

		// while traversing sceneObjects new objects may need to be added -
		// this list is used for temp storage of new objects
		private ArrayList newSceneObjects;

		// rectangle to save bounds of the scene
		private Rectangle m_bounds;

		// whether or not all scene graphics must be created and have their anti alias settings turned on
		private bool m_antiAliasMode;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private CWLevel m_level = null;

		public CarWarzGame()
		{
			m_antiAliasMode = true;

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// call game initialization - all objects are allocated in here
			initGame();

			// add event handlers
			Paint	+= new PaintEventHandler(DrawPausePicturePaintEvent);
			Paint	+= new PaintEventHandler(DrawCarsPaintEvent);
			Paint	+= new PaintEventHandler(DrawPointBarPaintEvent);
			KeyDown	+= new KeyEventHandler(Form1_KeyDown);
			KeyUp	+= new KeyEventHandler(Form1_KeyUp);
		}

		private void toggleAntiAliasMode() 
		{
			m_antiAliasMode = !m_antiAliasMode;

			// toggle for scene objects
			foreach (IDrawableObject drawableObject in sceneObjects) 
			{
				drawableObject.setAntialias( m_antiAliasMode );
			}
		}

		private void loadLevel() 
		{
			// only load level if game is either playing or in pause mode
			if ( currentState == GameState.paused || currentState == GameState.playing ) 
			{
				BinaryFormatter bw = new BinaryFormatter();
				FileInfo file;
				FileStream fstream;

				try 
				{
					string fileName;

					OpenFileDialog fd = new OpenFileDialog();

					fd.ShowDialog((IWin32Window)this);

					fileName = @fd.FileName;

					fd.Dispose();

					// load file
					file = new FileInfo(fileName);

					fstream = file.OpenRead();
					m_level = (CWLevel) bw.Deserialize(fstream);
					fstream.Close();

					m_level.setAntialias(m_antiAliasMode);

					// now that level has been loaded, we must inform both cars of the obstacles
					car1.setLevel( m_level );
					car2.setLevel( m_level );
				} 
				catch(FileNotFoundException ex) 
				{
					MessageBox.Show("Exception:\n    " + ex.ToString());
				} 
				catch(ArgumentException ex) 
				{
					// this exception will be thrown by FileInfo ctor if no file is chosen
					Refresh();
				}

				Refresh();
			}
		}

		private void initGame() 
		{
			// game starts out in paused state
			currentState = GameState.paused;

			// allocate arraylist for 100 shots
			sceneObjects = new ArrayList(100);

			// allocate arrayList for 50 temporary new scene objects
			newSceneObjects = new ArrayList(50);

			// create cars with numbers on top
			car1 = new Car( "1", Color.Blue, newSceneObjects );
			car2 = new Car( "2", Color.Red, newSceneObjects );

			// if we have loaded a level, the cars must be informed
			car1.setLevel(m_level);
			car2.setLevel(m_level);

			// turn on or off anti alias settings
			car1.setAntialias( m_antiAliasMode );
			car2.setAntialias( m_antiAliasMode );

//			car1.showBoundingBox(true);
//			car2.showBoundingBox(true);

			// position cars in a fancy way
			car1.Position = new Point(100,100);
			car1.Angle = 20;

			car2.Position = new Point(Width-100,Height-140);
			car2.Angle = 200;

			// add cars to scene
			sceneObjects.Add(car1);
			sceneObjects.Add(car2);

			// set up game tick timer
			gameTimerTick = new Timer();
			gameTimerTick.Interval = 15;

			// set up control tick timer
			gameControlTick = new Timer();
			gameControlTick.Interval = 100;

			// set up event handlers for the game timer and the control timer
			gameTimerTick.Tick		+= new EventHandler(GameTimerTickEvent);
			gameControlTick.Tick	+= new EventHandler(GameControlTickEvent);

			// avoid flicker by enabling double buffering
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.DoubleBuffer, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);

			// reset key monitor struct
			resetKeyMonitorStruct();

			// create rectangle to be able to easily access the bounds of the scene
			m_bounds.X = 20; m_bounds.Y = 20; m_bounds.Width = Width-40; m_bounds.Height = Height-60;

			// state machine to control game flow must start now
			gameControlTick.Start();
		}

		// resets key monitor struct
		private void resetKeyMonitorStruct() 
		{
			keysDown.pOne.upArrow = false;
			keysDown.pOne.downArrow = false;
			keysDown.pOne.leftArrow = false;
			keysDown.pOne.rightArrow = false;
			keysDown.pOne.fire1 = false;
			keysDown.pOne.fire2 = false;

			keysDown.pTwo.upArrow = false;
			keysDown.pTwo.downArrow = false;
			keysDown.pTwo.leftArrow = false;
			keysDown.pTwo.rightArrow = false;
			keysDown.pTwo.fire1 = false;
			keysDown.pTwo.fire2 = false;
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(720, 582);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "Form1";
			this.Text = "Car Warz";
			this.Resize += new System.EventHandler(this.Form1_Resize);
			this.Deactivate += new System.EventHandler(this.Form1_Deactivate);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new CarWarzGame());
		}


		// Timer event handler which handles game control timer events as a simple state machine
		private void GameControlTickEvent(object obj, EventArgs ea) 
		{
			switch ( currentState ) 
			{
				case GameState.waitingToBePaused:
					currentState = GameState.paused;
					Paint += new PaintEventHandler(DrawPausePicturePaintEvent);

					gameTimerTick.Stop();

					Refresh();
					break;

				case GameState.waitingToStartPlaying:
					currentState = GameState.playing;
					Paint -= new PaintEventHandler(DrawPausePicturePaintEvent);

					gameTimerTick.Start();

					Refresh();
					break;

				case GameState.playerOneWins:
					Paint += new PaintEventHandler(DrawPlayerWinsPaintEvent);

					currentState = GameState.gameOverPlayerOneWon;

					gameTimerTick.Stop();

					Refresh();
					break;

				case GameState.playerTwoWins:
					Paint += new PaintEventHandler(DrawPlayerWinsPaintEvent);

					currentState = GameState.gameOverPlayerTwoWon;

					gameTimerTick.Stop();

					Refresh();
					break;

				case GameState.paused:
				case GameState.playing:
					break;

				case GameState.waitingToRestartGame:
					Paint -= new PaintEventHandler(DrawPlayerWinsPaintEvent);

					initGame();

					currentState = GameState.waitingToStartPlaying;

					Refresh();
					break;

				case GameState.quitting:
					// quit hard&fast :o)
					Application.Exit();
					break;
			}
		}

		// checks the left/right keys passed as argument and does stuff accordingly to the car passed along
		private void checkLeftAndRightTurnKeys(Car car, Keys.Player keys) 
		{
			if ( (keys.leftArrow && !keys.rightArrow) || (!keys.leftArrow && keys.rightArrow) ) 
			{
				if (keys.leftArrow) 
				{
					car.tiresTurnLeft();
					car.turnLeft();
				}

				if (keys.rightArrow) 
				{
					car.tiresTurnRight();
					car.turnRight();
				}
			}
			else
			{
				car.tiresStraight();
			}
		}

		// checks the up/down keys passed as argument and does stuff accordingly to the car passed along
		private void checkAccelerateKeys(Car car, Keys.Player keys) 
		{
			if ( !(keys.upArrow && keys.downArrow) ) 
			{
				if (keys.upArrow)		car.accelerate();
				if (keys.downArrow)	car.decelerate();
			}
		}

		// checks the fire keys passed as argument and does stuff accordingly to the car passed along
		private void checkFireKeys(Car car, Keys.Player keys) 
		{
			IDrawableObject newBullet;

			if ( keys.fire1 && !car.isDead() ) 
			{
				if ( car.canShoot() ) 
				{
					car.fireShot();
					newBullet = new Shot( car.Position, car.Angle, 6 + car.Speed*0.5f, car );
					newBullet.setAntialias( m_antiAliasMode );
					sceneObjects.Add( newBullet );
				}				
			}

			if ( keys.fire2 && !car.isDead() ) 
			{
				if ( car.canShoot() ) 
				{
					car.fireRocket();
					newBullet = new Rocket( car.Position, car.Angle, 4, car, newSceneObjects );
					newBullet.setAntialias( m_antiAliasMode );
					sceneObjects.Add( newBullet );
				}				
			}
		}

		// runs through the scene objects, detecting all possible collisions and acting accordingly
		private void checkShotsForCollisions() 
		{
			IHittableObject	hittableObject;

			ArrayList objectsToRemove = new ArrayList(20);

			// for loop so scene objects may be removed if necessary
			for (int t=0; t < sceneObjects.Count; t++) 
			{
				hittableObject = sceneObjects[ t ] as IHittableObject;

				if ( hittableObject != null ) 
				{
					// check if any cars are hit by this shot
					if (hittableObject.hits(car1)) 
					{
						car1.Energy -= hittableObject.getPower();
						objectsToRemove.Add( hittableObject );
					}

					if (hittableObject.hits(car2)) 
					{
						car2.Energy -= hittableObject.getPower();
						objectsToRemove.Add( hittableObject );
					}

					if (hittableObject.isOutOfBounds( m_bounds ) ) 
					{
						// "schedule" this index for removal
						objectsToRemove.Add( hittableObject );
					}

					if (hittableObject.hits( m_level )) 
					{
						objectsToRemove.Add(hittableObject);
					}
				}
			}

			// now clean up the list of scene objects
			foreach (object objToRemove in objectsToRemove) sceneObjects.Remove(objToRemove);
		}

		// Timer event handler to tick game objects in scene and implement all logic related to the game events...
		private void GameTimerTickEvent(object obj, EventArgs ea) 
		{
			// give all scene objects a tick
			foreach (IDrawableObject tickableObject in sceneObjects) tickableObject.tick();

			// perform left and right key actions for both cars
			checkLeftAndRightTurnKeys(car1, keysDown.pOne);
			checkLeftAndRightTurnKeys(car2, keysDown.pTwo);

			// perform accelerate/decelerate actions for both cars
			checkAccelerateKeys(car1, keysDown.pOne);
			checkAccelerateKeys(car2, keysDown.pTwo);

			// perform firing actions for both cars
			checkFireKeys(car1, keysDown.pOne);
			checkFireKeys(car2, keysDown.pTwo);

			// run through shots in scene, detecting collisions at the same time
			checkShotsForCollisions();

			// check if cars are hitting each other
			if (car1.crashesWith(car2)) 
			{
				car1.Energy -= (int)(Math.Abs(car2.Speed) * 0.4f);
				car2.Energy -= (int)(Math.Abs(car1.Speed) * 0.4f);
			}

			// change state if one of the cars is dead
			if (car1.isDead()) 
			{
				currentState = GameState.playerTwoWins;
				gameTimerTick.Stop();
			}

			if (car2.isDead())
			{
				currentState = GameState.playerOneWins;
				gameTimerTick.Stop();
			}

			// now, if somehow new scene objects were added, move from temp list to actual list
			foreach (IDrawableObject drawableObject in newSceneObjects) 
			{
				sceneObjects.Add( drawableObject );
			}

			// empty temp list
			newSceneObjects.Clear();

            // force a repaint of the scene
			Refresh();
		}

		// draws objects in the scene - cars as well as all the shots
		private void DrawCarsPaintEvent(object obj, PaintEventArgs pea) 
		{
			foreach (IDrawableObject drawableObject in sceneObjects)
			{
				// draw shot
				drawableObject.draw(pea.Graphics);
			}

			// if we have loaded a level, draw it
			if (null != m_level) 
			{
				m_level.draw(pea.Graphics);
			}
		}

		// draws point bars and border surrounding scene
		private void DrawPointBarPaintEvent(object obj, PaintEventArgs pea) 
		{
			Graphics g = pea.Graphics;
			Brush	pointBarBrush = new SolidBrush( Color.FromArgb(140, 200, 180, 180) );
			Pen		scoreBoxPen = new Pen(Color.Black, 2);
			Brush	scoreBarBrush = new SolidBrush( Color.FromArgb(255, 255,100, 100) );
			Brush	energyTextBrush = new SolidBrush( Color.White );

			Font	energyText = new Font("Arial", 8, FontStyle.Bold);
			string	car1Energy, car2Energy;
			int		car1EnergyX, car2EnergyX;

		/*	// now this rectangle will be part of the level
			// draw semi-translucent rectangle in the top
			g.FillRectangle(pointBarBrush, 0,0, Width, 30);

			// and in the other corners
			g.FillRectangle(pointBarBrush, 0,30, 30, Height-55);
			g.FillRectangle(pointBarBrush, Width-36, 30, 30, Height-85);
			g.FillRectangle(pointBarBrush, 30,Height-55, Width, Height);
*/
			// draw energy bars with a length of twice the amount of energy in the car
			g.FillRectangle(scoreBarBrush, 10, 8, car1.Energy * 2, 15);
			g.FillRectangle(scoreBarBrush, Width-220 + 200 - (car2.Energy * 2), 8, car2.Energy * 2, 15);

			// draw black outlines around energy bars
			g.DrawRectangle(scoreBoxPen, 10, 8, 200, 15);
			g.DrawRectangle(scoreBoxPen, Width-220, 8, 200, 15);

			// draw text showing amount of energy
            car1Energy = string.Format("{0} %", car1.Energy);
			car2Energy = string.Format("{0} %", car2.Energy);

			car1EnergyX = 110 - (int)g.MeasureString(car1Energy, energyText).Width/2;
			car2EnergyX = Width - 110 - (int)g.MeasureString(car2Energy, energyText).Width/2;

			g.DrawString(car1Energy, energyText, energyTextBrush, car1EnergyX, 10);
			g.DrawString(car2Energy, energyText, energyTextBrush, car2EnergyX, 10);
					
		}

		// draws a pause picture including instructions
		private void DrawPausePicturePaintEvent(object obj, PaintEventArgs pea) 
		{
			Graphics g = pea.Graphics;

			
			string pauseTxt = "Paused";
			string instructions
				= "Player one:\n"
				+ "  - use arrow keys to control movement\n"
				+ "  - press shift and enter to fire\n\n"
				+ "Player two:\n"
				+ "  - use AWSD keys to control movement\n"
				+ "  - press control and tab to fire\n\n"
				+ "Other keys: \n"
				+ "  - F1 to unpause\n"
				+ "  - F10 to pause\n"
				+ "  - F2 to toggle antialiasing\n"
				+ "  - L to load a level\n"
				+ "  - ESC to quit\n";

			Font pauseFont = new Font("Arial", 16);
			Font helpTextFont = new Font("Arial", 12);

			Brush boxBrush = new SolidBrush( Color.FromArgb(80, 155, 155, 255) );
			Brush textBrush = new SolidBrush( Color.FromArgb(200, 20, 20, 80 ) );

			g.FillRectangle( boxBrush, 200, 30, Width-400, Height-220 );

			g.DrawString(pauseTxt, pauseFont, textBrush, (Width - g.MeasureString(pauseTxt, pauseFont).Width)/2, 40);
			g.DrawString(instructions, helpTextFont, textBrush, (Width - g.MeasureString(instructions, helpTextFont).Width)/2, 78);
		}

		// draws a WINNER TEXT for one of the cars, depending on the current state
		private void DrawPlayerWinsPaintEvent(object sender, PaintEventArgs e)
		{
			Graphics	g = e.Graphics;
			Font		winnerFont1 = new Font("Arial", 28, FontStyle.Bold);
			Font		winnerFont2 = new Font("Arial", 28, FontStyle.Bold);
			Brush		winnerBrush = new SolidBrush( Color.FromArgb(30,0,0,0) );
			string		winnerText;
			int			yPosition = 30;
			string		helpText = "Press F1 to play again";
			Font		helpTextFont = new Font("Arial", 12);

			Brush boxBrush = new SolidBrush( Color.FromArgb(80, 155, 155, 255) );
			Brush textBrush = new SolidBrush( Color.FromArgb(200, 20, 20, 80 ) );

			g.FillRectangle( boxBrush, 200, 100, Width-400, 80 );

			g.DrawString(helpText, helpTextFont, textBrush, (Width - g.MeasureString(helpText, helpTextFont).Width)/2, 120);

			// create a descriptive string depending on who won the game
			if (currentState == GameState.gameOverPlayerOneWon) 
			{
				winnerText = "Player one wins!";
			} 
			else 
			{
				winnerText = "Player two wins!";
			}

			int stringWidth = (int)g.MeasureString(winnerText, winnerFont1).Width;

			// draw some shadows first
			g.DrawString(winnerText, winnerFont2, winnerBrush, 3+(Width-stringWidth)/2, 3+yPosition);
			g.DrawString(winnerText, winnerFont2, winnerBrush, -3+(Width-stringWidth)/2, 3+yPosition);
			g.DrawString(winnerText, winnerFont2, winnerBrush, 3+(Width-stringWidth)/2, -3+yPosition);
			g.DrawString(winnerText, winnerFont2, winnerBrush, -3+(Width-stringWidth)/2, -3+yPosition);

			// and then draw text
			g.DrawString(winnerText, winnerFont1, Brushes.Red, (Width-stringWidth)/2, yPosition);
		}
		
		// key event handler to keep the keysDown struct updated
		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			e.Handled = true;

			switch ( (ControlKeys)e.KeyValue ) 
			{
				case ControlKeys.loadKey:
					loadLevel();
					break;

				////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				// pause/play function keys
				case ControlKeys.unPauseKey:
					if ( currentState == GameState.paused ) 
					{
						currentState = GameState.waitingToStartPlaying;
					} 
					else if ( currentState == GameState.gameOverPlayerOneWon || currentState == GameState.gameOverPlayerTwoWon ) 
					{
						currentState = GameState.waitingToRestartGame;
					}
					break;

				case ControlKeys.pauseKey:
					if ( currentState == GameState.playing ) 
					{
						currentState = GameState.waitingToBePaused;
					}
					break;

				case ControlKeys.quitKey:
					currentState = GameState.quitting;
					break;

				case ControlKeys.antialiasToggle:
					toggleAntiAliasMode();
					break;

				////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				// player one car control keys
				case ControlKeys.playerOneUp:
					keysDown.pOne.upArrow = true;
					break;

				case ControlKeys.playerOneDown:
					keysDown.pOne.downArrow = true;
					break;

				case ControlKeys.playerOneLeft:
					car1.tiresTurnLeft();
					keysDown.pOne.leftArrow = true;
					break;

				case ControlKeys.playerOneRight:
					car1.tiresTurnRight();
					keysDown.pOne.rightArrow = true;
					break;

				case ControlKeys.playerOneFire1:
					keysDown.pOne.fire1 = true;
					break;

				case ControlKeys.playerOneFire2:
					keysDown.pOne.fire2 = true;
					break;

					////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
					// player two car control keys
				case ControlKeys.playerTwoUp:
					keysDown.pTwo.upArrow = true;
					break;

				case ControlKeys.playerTwoDown:
					keysDown.pTwo.downArrow = true;
					break;

				case ControlKeys.playerTwoLeft:
					car1.tiresTurnLeft();
					keysDown.pTwo.leftArrow = true;
					break;

				case ControlKeys.playerTwoRight:
					car1.tiresTurnRight();
					keysDown.pTwo.rightArrow = true;
					break;

				case ControlKeys.playerTwoFire1:
					keysDown.pTwo.fire1 = true;
					break;

				case ControlKeys.playerTwoFire2:
					keysDown.pTwo.fire2 = true;
					break;
					
				////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				// key event not handled
				default:
					// if we did not enter any of the cases above, tell environment that we did not handle the keypress
					e.Handled = false;
					break;
			}
		}

		// key event handler to keep the keysDown struct updated
		private void Form1_KeyUp(object sender, KeyEventArgs e)
		{
			e.Handled = true;

			switch ( (ControlKeys)e.KeyValue ) 
			{

				////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				// player one car control keys
				case ControlKeys.playerOneUp:
					keysDown.pOne.upArrow = false;
					break;

				case ControlKeys.playerOneDown:
					keysDown.pOne.downArrow = false;
					break;

				case ControlKeys.playerOneLeft:
					keysDown.pOne.leftArrow = false;
					break;

				case ControlKeys.playerOneRight:
					keysDown.pOne.rightArrow = false;
					break;

				case ControlKeys.playerOneFire1:
					keysDown.pOne.fire1 = false;
					break;

				case ControlKeys.playerOneFire2:
					keysDown.pOne.fire2 = false;
					break;

				////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				// player two car control keys
				case ControlKeys.playerTwoUp:
					keysDown.pTwo.upArrow = false;
					break;

				case ControlKeys.playerTwoDown:
					keysDown.pTwo.downArrow = false;
					break;

				case ControlKeys.playerTwoLeft:
					keysDown.pTwo.leftArrow = false;
					break;

				case ControlKeys.playerTwoRight:
					keysDown.pTwo.rightArrow = false;
					break;

				case ControlKeys.playerTwoFire1:
					keysDown.pTwo.fire1 = false;
					break;

				case ControlKeys.playerTwoFire2:
					keysDown.pTwo.fire2 = false;
					break;

					////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				// key event not handled
				default:
					// if we did not enter any of the cases above, tell environment that we did not handle the keypress
					e.Handled = false;
					break;
			}		
		}


		// resize event handler to adapt to new bounds of the scene
		private void Form1_Resize(object sender, System.EventArgs e)
		{
			m_bounds = new Rectangle(-20,-20,Width+40,Height+60);
		}

		// "lose focus"-event handler for the form - enters pause mode so game won't run out of control in the BG :o)
		private void Form1_Deactivate(object sender, System.EventArgs e)
		{
			// when form loses focus we can not expect to receive key down events... so
			// we reset the key monitoring struct
			resetKeyMonitorStruct();

			// enter pause mode if we are in playing mode
			if (currentState == GameState.playing) 
			{
				currentState = GameState.waitingToBePaused;
			}

			gameTimerTick.Stop();

			Refresh();
		}

		
	}
}
