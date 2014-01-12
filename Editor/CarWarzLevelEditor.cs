using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Data;
using Lab06_2;

namespace Lab06
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class CarWarzLevelEditor : System.Windows.Forms.Form
	{
		private System.ComponentModel.Container components = null;

		// keep track of which modifier keys are active/inactive
		private struct ModifierKeysActive 
		{
			public bool shift;
			public bool ctrl;
		}

		// create instance - used in KeyDown KeyUp evt handlers
		private ModifierKeysActive m_modifierKeys;

		// we need a Car Warz level, CWLevel
		private CWLevel m_level;

		// we need a reference to store an IGraphicObject if one is currently being drawn
		private IGraphicObject m_graphicBeingDrawn = null;

		public CarWarzLevelEditor()
		{
			InitializeComponent();

			m_level = new CWLevel();

			m_level.createLevelBounds(Width, Height);

			// avoid flicker by enabling double buffering
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.DoubleBuffer, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);

			// set antialiasing for level
			m_level.setAntialias(true);

			// subscribe to paint events
			Paint += new PaintEventHandler( CWLevelPainter );
			Paint += new PaintEventHandler( CWGraphicsPainter );
			Paint += new PaintEventHandler( CarStartingPositionPainter );
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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

		// paint event handler to paint level
		private void CWLevelPainter(object sender, PaintEventArgs pea) 
		{
			m_level.draw( pea.Graphics );
		}

		private void CarStartingPositionPainter(object sender, PaintEventArgs pea) 
		{
			Graphics g = pea.Graphics;
			Pen circlePen = new Pen( Color.FromArgb(100, 0,0,0) );
			int circleRadius = 24;

            g.DrawEllipse( circlePen, 100-circleRadius+5, 100-circleRadius, circleRadius*2, circleRadius*2 );				
			g.DrawEllipse( circlePen, Width-100-circleRadius-5, Height-140-circleRadius, circleRadius*2, circleRadius*2 );
		}

		// paint event handler to paint object currently being drawn (if any)
		private void CWGraphicsPainter(object sender, PaintEventArgs pea) 
		{
			// if reference is currently pointing to something, draw it
			if (null != m_graphicBeingDrawn) 
			{
				m_graphicBeingDrawn.draw( pea.Graphics );
			}
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// CarWarzLevelEditor
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(720, 582);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "CarWarzLevelEditor";
			this.Text = "Car Warz Level Editor";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CarWarzLevelEditor_KeyDown);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CarWarzLevelEditor_MouseDown);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.CarWarzLevelEditor_MouseUp);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CarWarzLevelEditor_KeyUp);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.CarWarzLevelEditor_MouseMove);
			this.Deactivate += new System.EventHandler(this.CarWarzLevelEditor_Deactivate);

		}
		#endregion

		private void CarWarzLevelEditor_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			switch( e.KeyCode ) 
			{
				case Keys.ShiftKey:
					m_modifierKeys.shift = true;
					break;

				case Keys.ControlKey:
					m_modifierKeys.ctrl = true;
					break;

				case Keys.S:
					// if S was pressed and modifier CTRL is down, serialize level object to disk
					if (m_modifierKeys.ctrl) 
					{
						// create FileInfo object
						FileInfo file;
						// file stream object
						FileStream fstream;
						// binary formatter for serialization
						BinaryFormatter bw;

						try 
						{
							//file = new FileInfo(@"C:\TestLevel.dat");
							SaveFileDialog of = new SaveFileDialog();

							of.ShowDialog((IWin32Window)this);
							
							file = new FileInfo(@of.FileName);
							
							// get stream
							fstream = file.Open( FileMode.OpenOrCreate, FileAccess.Write, FileShare.None );

							// use stream for serialization
							bw = new BinaryFormatter();
							bw.Serialize(fstream, m_level);

							// close stram
							fstream.Close();
						} 
						catch(IOException ex) 
						{
							MessageBox.Show("Exception:\n     "+ex.ToString());
						}
						catch(ArgumentException ex) 
						{
							// this exception happens when the user presses cancel in the save filedialog
						}
					}
					break;

				case Keys.L:
					if (m_modifierKeys.ctrl) 
					{
						BinaryFormatter bw = new BinaryFormatter();
						FileInfo file;
						FileStream fstream;

						try 
						{
							string fileName;
							OpenFileDialog fd = new OpenFileDialog();

							fd.ShowDialog((IWin32Window)this);

							fileName = fd.FileName;

							fd.Dispose();

							// load file
							file = new FileInfo(@fileName);

							fstream = file.OpenRead();
							m_level = (CWLevel) bw.Deserialize(fstream);
							fstream.Close();
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
					break;
			}
		}

		private void CarWarzLevelEditor_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			switch( e.KeyCode ) 
			{
				case Keys.ShiftKey:
					m_modifierKeys.shift = false;
					break;

				case Keys.ControlKey:
					m_modifierKeys.ctrl = false;
					break;
			}
		}

		private void CarWarzLevelEditor_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
            // we must start drawing (if not currently drawing - MouseUp event could accidentally have been missed due to loss of focus
			if (null == m_graphicBeingDrawn) 
			{
				PointF mousePosition = new PointF(e.X, e.Y);

				// now add a graphic depending on active modifier keys
				if (m_modifierKeys.shift) 
				{
					m_graphicBeingDrawn = new CWRectangle(new RectangleF(mousePosition, new Size(0,0)));
				} 
				else if (m_modifierKeys.ctrl) 
				{
					m_graphicBeingDrawn = new CWEllipsis(new RectangleF(mousePosition, new Size(0,0)));
				} 
				else 
				{
					m_graphicBeingDrawn = new CWLine( mousePosition, mousePosition );
				}

				Refresh();
			} 
			else 
			{
				// this means that another button on the mouse was pressed while we were drawing an object
				// - in this situation we cancel the drawing
				m_graphicBeingDrawn = null;

				Refresh();
			}
		}

		private void CarWarzLevelEditor_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (null != m_graphicBeingDrawn) 
			{
				// add object to level
				m_level.add( m_graphicBeingDrawn );

				// nullify reference
				m_graphicBeingDrawn = null;

				Refresh();
			}
		
		}

		private void CarWarzLevelEditor_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			// if we are drawing an object, update its endpoint
			if (null != m_graphicBeingDrawn) 
			{
				PointF mousePosition = new PointF(e.X, e.Y);

				m_graphicBeingDrawn.setEndpoint( mousePosition );

				Refresh();
			}
		}


		[STAThread]
		static void Main() 
		{
			Application.Run(new CarWarzLevelEditor());
		}

		private void CarWarzLevelEditor_Deactivate(object sender, System.EventArgs e)
		{
			// when focus is lost, all modifier keys must be forgotten and we cancel the drawing operation if one is currently in action
			m_modifierKeys.ctrl = false;
			m_modifierKeys.shift = false;

			m_graphicBeingDrawn = null;

			// this is due to the fact that we may never receive the KeyUp events
		}

	}
}
