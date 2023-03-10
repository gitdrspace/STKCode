//=====================================================//
//  Copyright 2005, Analytical Graphics, Inc.          //
//=====================================================//
using Microsoft.Win32;
using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

using AGI.Attr;
using AGI.Plugin;
using AGI.Hpop.Plugin;
using AGI.VectorGeometryTool.Plugin;
using AGI.STK.Plugin;

namespace AGI.Hpop.Plugin.Examples.Stk.ForceModeling.CSharp
{
	/// <summary>
	/// Example1 class provides a force model plugin that allows
	/// for the adding acceleration forces in the three cardinal
	/// directions, within a choosen reference frame.
	/// </summary>
	// NOTE: Generate your own Guid using Microsoft's GuidGen.exe
	// If you used this plugin in STK 6, 7 you should create a new
	// copy of your plugin's source, and update it with a new GUID
	// for STK 8.  Then you will be able to make changes in the 
	// new STK 8 plugin and not affect your old STK 6,7 plugin.
	[Guid("4FCDA5C7-9C55-4248-8242-5ADB66A53CFB")]
	// NOTE: Create your own ProgId to match your plugin's namespace and name
	[ProgId("AGI.Hpop.Plugin.Examples.Stk.ForceModeling.CSharp.Example1")]
	// NOTE: Specify the ClassInterfaceType.None enumeration, so the custom COM Interface 
	// you created, i.e. IExample1, is used instead of an autogenerated COM Interface.
	[ClassInterface(ClassInterfaceType.None)]
	public class Example1 :
		IExample1,
		IAgAsHpopPlugin,
		IAgUtPluginConfig
	{
		#region Plugin Private Data Members

		private IAgUtPluginSite			m_UPS;
		private IAgStkPluginSite		m_SPS;
		private AgCrdnPluginProvider	m_CPP;
		private AgCrdnConfiguredVector	m_CCV;
		private object					m_Scope;
		private int						m_PreNextCntr;
		private int						m_EvalCntr;
		private int						m_PostEvalCntr;
		private bool					m_SrpIsOn;

		#endregion

		#region Life Cycle Methods
		public Example1() : base()
		{
			try
			{
				Debug.WriteLine( "--> Entered", this.m_Name+".Example1()");

				this.SetAttributeConfigDefaults();

				this.m_UPS = null;
				this.m_SPS = null;
				this.m_CPP = null;
				this.m_CCV = null;

				this.m_SRPArea				= 0.0;
				this.m_PreNextCntr			= 0; // CLR initializes to zero, but do it anyways.
				this.m_EvalCntr				= 0; // CLR initializes to zero, but do it anyways.
				this.m_PostEvalCntr			= 0; // CLR initializes to zero, but do it anyways.
				this.m_SrpIsOn				= false;
			}
			finally
			{
				Debug.WriteLine( "<-- Exited", this.m_Name+".Example1()");
			}
		}

		/// <summary>
		/// Initializes the Plugin Attribute configuration 
		/// Data Members to their defaults
		/// </summary>
		public void SetAttributeConfigDefaults()
		{
			try
			{
				Debug.WriteLine( "--> Entered", this.m_Name+".SetConfigDefaults()");

				//===========================
				// General Plugin attributes
				//===========================
				this.m_Name					= "CSharpNet.Example1";
				this.m_Enabled				= true;
				this.m_VectorName			= "Periapsis";

				//===========================
				// Propagation related
				//===========================
				this.m_AccelX					= 0.0;
				this.m_AccelY					= 0.07;
				this.m_AccelZ					= 0.0;

				this.m_AccelRefFrame			= (int)AgEUtFrame.eUtFrameNTC;
				this.m_AccelRefFrameChoices		= new object[4];
				this.m_AccelRefFrameChoices[0]  = AgEUtFrame.eUtFrameInertial.ToString();	//0
				this.m_AccelRefFrameChoices[1]  = AgEUtFrame.eUtFrameFixed.ToString();		//1
				this.m_AccelRefFrameChoices[2]  = AgEUtFrame.eUtFrameLVLH.ToString();		//2
				this.m_AccelRefFrameChoices[3]  = AgEUtFrame.eUtFrameNTC.ToString();		//3

				//===========================
				// Logging related attributes
				//===========================
				this.m_MsgStatus			= false;
				this.m_EvalMsgInterval		= 5000;
				this.m_PostEvalMsgInterval	= 5000;
				this.m_PreNextMsgInterval	= 1000;
			}
			finally
			{
				Debug.WriteLine( "<-- Exited", this.m_Name+".SetConfigDefaults()");
			}
		}
		
		#endregion
		
		#region IExample1 Interface Implementation
		//=============================================
		// Plugin Attributes to be configured
		// will be exposed via .NET properties
		// and using the Attribute Builder reference
		// passed as a parameter to the GetPluginConfig
		// Method.
		//==============================================
		private string			m_Name = "Hpop.FrcMdl.CSharp.Example1";	// Plugin Significant
		private bool			m_Enabled;						// Plugin Significant

		private string			m_VectorName;					// Init/Evaluate significant
		private double			m_SRPArea;						// Init/Evaluate significant

		private double			m_AccelX;						// Propagation Significant
		private double			m_AccelY;						// Propagation Significant
		private double			m_AccelZ;						// Propagation Significant
		private int				m_AccelRefFrame;				// Propagation Significant
		private object[]		m_AccelRefFrameChoices;			// Propagation Significant

		private bool			m_MsgStatus;					// Uncessesary Added Feature
		private int				m_EvalMsgInterval;				// Uncessesary Added Feature
		private int				m_PostEvalMsgInterval;			// Uncessesary Added Feature
		private int				m_PreNextMsgInterval;			// Uncessesary Added Feature

		public string Name
		{
			get
			{
				Debug.WriteLine( this.m_Name, this.m_Name+".getName()");
				return this.m_Name;
			}
			set
			{
				this.m_Name = value;
				Debug.WriteLine( this.m_Name, this.m_Name+".setName()");
			}
		}

		public bool Enabled
		{
			get
			{
				Debug.WriteLine( this.m_Enabled, this.m_Name+".getEnabled()");
				return this.m_Enabled;
			}
			set
			{
				this.m_Enabled = value;
				Debug.WriteLine( this.m_Enabled, this.m_Name+".setEnabled()");
			}
		}

		public string VectorName
		{
			get
			{
				Debug.WriteLine( this.m_VectorName, this.m_Name+".getVectorName()");
				return this.m_VectorName;
			}

			set
			{
				Debug.WriteLine( this.m_VectorName, this.m_Name+".setVectorName()");
				this.m_VectorName = value;
			}
		}
		public string AccelRefFrame
		{
			get
			{
				string sAccelRefFrame = "None";

				if( this.m_AccelRefFrame.Equals( (int)AgEUtFrame.eUtFrameInertial ) )
				{
					//0
					sAccelRefFrame = AgEUtFrame.eUtFrameInertial.ToString();
				}
				else if( this.m_AccelRefFrame.Equals( (int)AgEUtFrame.eUtFrameFixed ) )
				{
					//1
					sAccelRefFrame = AgEUtFrame.eUtFrameFixed.ToString();
				}
				else if( this.m_AccelRefFrame.Equals( (int)AgEUtFrame.eUtFrameLVLH ) )
				{
					//2
					sAccelRefFrame = AgEUtFrame.eUtFrameLVLH.ToString();
				}
				else if( this.m_AccelRefFrame.Equals( (int)AgEUtFrame.eUtFrameNTC ) )
				{
					//3
					sAccelRefFrame = AgEUtFrame.eUtFrameNTC.ToString();
				}

				Debug.WriteLine( sAccelRefFrame, this.m_Name+".getAccelRefFrame()");

				return sAccelRefFrame;
			}
			set
			{
				if( value.Equals( AgEUtFrame.eUtFrameInertial.ToString() ) )
				{
					//0
					this.m_AccelRefFrame = (int)AgEUtFrame.eUtFrameInertial;
				}
				else if( value.Equals( AgEUtFrame.eUtFrameFixed.ToString() ) )
				{
					//1
					this.m_AccelRefFrame = (int)AgEUtFrame.eUtFrameFixed;
				}
				else if( value.Equals( AgEUtFrame.eUtFrameLVLH.ToString() ) )
				{
					//2
					this.m_AccelRefFrame = (int)AgEUtFrame.eUtFrameLVLH;
				}
				else if( value.Equals( AgEUtFrame.eUtFrameNTC.ToString() ) )
				{
					//3
					this.m_AccelRefFrame = (int)AgEUtFrame.eUtFrameNTC;
				}

				Debug.WriteLine( value, this.m_Name+".setAccelRefFrame()");
			}
		}
		public object[] AccelRefFrameChoices
		{
			get
			{
				Debug.WriteLine( this.m_AccelRefFrameChoices, this.m_Name+".getAccelRefFrameChoices()");
				return this.m_AccelRefFrameChoices;
			}
		}
		public double AccelX
		{
			get
			{
				Debug.WriteLine( this.m_AccelX, this.m_Name+".getAccelX()");
				return this.m_AccelX;
			}
			set
			{
				this.m_AccelX = value;
				Debug.WriteLine( this.m_AccelX, this.m_Name+".setAccelX()");
			}
		}
		public double AccelY
		{
			get
			{
				Debug.WriteLine( this.m_AccelY, this.m_Name+".getAccelY()");
				return this.m_AccelY;
			}
			set
			{
				this.m_AccelY = value;
				Debug.WriteLine( this.m_AccelY, this.m_Name+".setAccelY()");
			}
		}
		public double AccelZ
		{
			get
			{
				Debug.WriteLine( this.m_AccelZ, this.m_Name+".getAccelZ()");
				return this.m_AccelZ;
			}
			set
			{
				this.m_AccelZ = value;
				Debug.WriteLine( this.m_AccelZ, this.m_Name+".setAccelZ()");
			}
		}
		public bool MsgStatus
		{
			get
			{
				Debug.WriteLine( this.m_MsgStatus, this.m_Name+".getMsgStatus()");
				return this.m_MsgStatus;
			}
			set
			{
				this.m_MsgStatus = value;
				Debug.WriteLine( this.m_MsgStatus, this.m_Name+".setMsgStatus()");
			}
		}
		public int EvalMsgInterval
		{
			get
			{
				Debug.WriteLine( this.m_EvalMsgInterval, this.m_Name+".getEvalMsgInterval()");
				return this.m_EvalMsgInterval;
			}
			set
			{
				this.m_EvalMsgInterval = value;
				Debug.WriteLine( this.m_EvalMsgInterval, this.m_Name+".setEvalMsgInterval()");
			}
		}
		public int PostEvalMsgInterval
		{
			get
			{
				Debug.WriteLine( this.m_PostEvalMsgInterval, this.m_Name+".getPostEvalMsgInterval()");
				return this.m_PostEvalMsgInterval;
			}
			set
			{
				this.m_PostEvalMsgInterval = value;
				Debug.WriteLine( this.m_PostEvalMsgInterval, this.m_Name+".setPOstEvalMsgInterval()");
			}
		}
		public int PreNextMsgInterval
		{
			get
			{
				Debug.WriteLine( this.m_PreNextMsgInterval, this.m_Name+".getPreNextMsgInterval()");
				return this.m_PreNextMsgInterval;
			}
			set
			{
				Debug.WriteLine( this.m_PreNextMsgInterval, this.m_Name+".setPreNextMsgInterval()");
				this.m_PreNextMsgInterval = value;
			}
		}
		#endregion

		#region IAgAsHpopPlugin Interface Implementation
		public bool Init( IAgUtPluginSite Ups )
		{
			try
			{
				Debug.WriteLine( "--> Entered", this.m_Name+".Init()");

				this.m_UPS = Ups;

				if( this.m_UPS != null )
				{
					if( this.m_Enabled )
					{
						m_SPS = this.m_UPS as IAgStkPluginSite;

						if( this.m_SPS != null )
						{
							this.m_CPP	= this.m_SPS.VectorToolProvider;

							if(this.m_CPP != null)
							{
								this.m_CCV	= this.m_CPP.ConfigureVector( this.m_VectorName, "", "J2000", "CentralBody/Earth");
							}
	
							if( this.m_MsgStatus )
							{
								this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name+".Init():");
                                this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".Init(): AccelRefFrame( " + this.AccelRefFrame + " )");
                                this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".Init(): AccelX( " + this.m_AccelX + " )");
                                this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".Init(): AccelY( " + this.m_AccelY + " )");
                                this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".Init(): AccelZ( " + this.m_AccelZ + " )");
							}

							if( this.m_CCV == null)
							{
                                this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".Init(): Could not obtain " + m_VectorName);
                                this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".Init(): Turning off the computation of SRP Area");
							}
						}
						else
						{
                            this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".Init(): Could not obtain IAgStkPluginSite from" + this.m_UPS.SiteName);
                            this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".Init(): Turning off the computation of SRP Area");
						}
					}
					else
					{
						if( this.m_MsgStatus )
						{
                            this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".Init(): Disabled");
						}
					}
				}
				else
				{
					throw new Exception( "UtPluginSite was null" );
				}
			}
			catch( Exception ex )
			{
				this.m_Enabled = false;

				if( this.m_UPS != null )
				{
                    this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgAlarm, this.m_Name + ".Init(): Exception Message( " + ex.Message + " )");
                    this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgAlarm, this.m_Name + ".Init(): Exception StackTr( " + ex.StackTrace + " )");
				}
				Debug.WriteLine( "Exception Message( " + ex.Message + " )", this.m_Name+".Init()" );
				Debug.WriteLine( "Exception StackTr( " + ex.StackTrace + " )", this.m_Name+".Init()");
			}
			finally
			{
				Debug.WriteLine( "<-- Exited", this.m_Name+".Init()" );
			}

			return this.m_Enabled;
		}

		public bool PrePropagate( IAgAsHpopPluginResult Result )
		{
			try
			{
				Debug.WriteLine( "--> Entered", this.m_Name+".PrePropagate()");

				if( this.m_UPS != null )
				{
					if( this.m_Enabled )
					{
						int		WholeDays	=	0;
						double	SecsIntoDay =	0.0;
						int		Year		=	0;
						int		DayOfYear	=	0;
						int		Month		=	0;
                        int     DayOfMonth  =   0;
                        int		Hours		=	0;
						int		Minutes		=	0;
						double	Seconds		=	0.0;

						if( Result != null )
						{
							Result.RefEpochElements( AgEUtTimeScale.eUtTimeScaleUTC, ref WholeDays, ref SecsIntoDay, ref Year, ref DayOfYear, ref Month, ref DayOfMonth, ref Hours, ref Minutes, ref Seconds );

							m_SrpIsOn = Result.IsForceModelOn(AgEForceModelType.eSRPModel);
							if( m_SrpIsOn )
							{
								this.m_SRPArea = Result.SRPArea;
							}
						}

						if( this.m_MsgStatus )
						{
                            this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".PrePropagate():");
                            this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".PrePropagate(): Epoch WholeDays( " + WholeDays + " )");
                            this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".PrePropagate(): Epoch SecsIntoDay( " + SecsIntoDay + " )");
                            this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".PrePropagate(): Epoch Year( " + Year + " )");
                            this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".PrePropagate(): Epoch DayOfYear( " + DayOfYear + " )");
                            this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".PrePropagate(): Epoch Month( " + Month + " )");
                            this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".PrePropagate(): Epoch Hours( " + Hours + " )");
                            this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".PrePropagate(): Epoch Minutes( " + Minutes + " )");
                            this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".PrePropagate(): Epoch Seconds( " + Seconds + " )");
						}
					}
					else
					{
						if( this.m_MsgStatus )
						{
                            this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".Prepropagate(): Disabled");
						}
					}
				}
				else
				{
					throw new Exception( "UtPluginSite was null" );
				}
			}
			catch( Exception ex )
			{
				this.m_Enabled = false;

				if( this.m_UPS != null )
				{
                    this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgAlarm, this.m_Name + ".PrePropagate(): Exception Message( " + ex.Message + " )");
                    this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgAlarm, this.m_Name + ".PrePropagate(): Exception StackTr( " + ex.StackTrace + " )");
				}
				Debug.WriteLine( "Exception Message( " + ex.Message + " )", this.m_Name+".PrePropagate()" );
				Debug.WriteLine( "Exception StackTr( " + ex.StackTrace + " )", this.m_Name+".PrePropagate()");
			}
			finally
			{
				Debug.WriteLine( "<-- Exited", this.m_Name+".PrePropagate()" );
			}

			return this.m_Enabled;
		}

		public bool PreNextStep( IAgAsHpopPluginResult Result )
		{
			try
			{
				if( this.m_PreNextCntr % this.m_PreNextMsgInterval == 0 )
				{
					Debug.WriteLine( "--> Entered", this.m_Name+".PreNextStep()");
				}

				this.m_PreNextCntr++;

				if( this.m_UPS != null )
				{
					if( this.m_Enabled )
					{
						if( this.m_MsgStatus )
						{
							if( this.m_PreNextCntr % this.m_PreNextMsgInterval == 0 )
							{
                                this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".PreNextStep( " + this.m_PreNextCntr + " ):");
							}
						}
					}
					else
					{
						if( this.m_MsgStatus )
						{
                            this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".PreNextStep(): Disabled");
						}
					}
				}
				else
				{
					throw new Exception( "UtPluginSite was null" );
				}
			}
			catch( Exception ex )
			{
				this.m_Enabled = false;

				if( this.m_UPS != null )
				{
                    this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgAlarm, this.m_Name + ".PreNextStep(): Exception Message( " + ex.Message + " )");
                    this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgAlarm, this.m_Name + ".PreNextStep(): Exception StackTr( " + ex.StackTrace + " )");
				}
				Debug.WriteLine( "Exception Message( " + ex.Message + " )", this.m_Name+".PreNextStep()" );
				Debug.WriteLine( "Exception StackTr( " + ex.StackTrace + " )", this.m_Name+".PreNextStep()");
			}
			finally
			{
				if( this.m_PreNextCntr % this.m_PreNextMsgInterval == 0 )
				{
					Debug.WriteLine( "<-- Exited", this.m_Name+".PreNextStep()");
				}
			}
			
			return this.m_Enabled;
		}

		public bool Evaluate( IAgAsHpopPluginResultEval ResultEval )
		{
			try
			{
				if( this.m_EvalCntr % this.m_EvalMsgInterval == 0 )
				{
					Debug.WriteLine( "--> Entered", this.m_Name+".Evaluate( " + this.m_EvalCntr + " )" );
				}

				this.m_EvalCntr++;

				if( this.m_UPS != null )
				{
					if( this.m_Enabled )
					{
						this.EvaluateSRPArea( ResultEval );

						ResultEval.AddAcceleration( (AgEUtFrame)this.m_AccelRefFrame, this.m_AccelX, this.m_AccelY, this.m_AccelZ );

						if( this.m_MsgStatus )
						{
							if( this.m_EvalCntr % this.m_EvalMsgInterval == 0 )
							{
                                this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".Evaluate( " + this.m_EvalCntr + " ):");
							}
						}
					}
					else
					{
						if( this.m_MsgStatus )
						{
                            this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".Evaluate(): Disabled");
						}
					}
				}
				else
				{
					throw new Exception( "UtPluginSite was null" );
				}
			}
			catch( Exception ex )
			{
				this.m_Enabled = false;

				if( this.m_UPS != null )
				{
                    this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgAlarm, this.m_Name + ".Evaluate(): Exception Message( " + ex.Message + " )");
                    this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgAlarm, this.m_Name + ".Evaluate(): Exception StackTr( " + ex.StackTrace + " )");
				}
				Debug.WriteLine( "Exception Message( " + ex.Message + " )", this.m_Name+".Evaluate()" );
				Debug.WriteLine( "Exception StackTr( " + ex.StackTrace + " )", this.m_Name+".Evaluate()");
			}
			finally
			{
				if( this.m_EvalCntr % this.m_EvalMsgInterval == 0 )
				{
					Debug.WriteLine( "<-- Exited", this.m_Name+".Evaluate( " + this.m_EvalCntr + " )" );
				}
			}

			return this.m_Enabled;
		}

		public void EvaluateSRPArea( IAgAsHpopPluginResultEval ResultEval )
		{
			if(!m_SrpIsOn)
			{
				return;
			}

			// may not have obtained a vector to do the SRP Area computation
			if( this.m_CCV == null )
			{
				return;
			}

			try
			{
				if( this.m_EvalCntr % this.m_EvalMsgInterval == 0 )
				{
					Debug.WriteLine( "--> Entered", this.m_Name+".EvaluateSRPArea( " + this.m_EvalCntr + " )" );
				}
				#region Local Variables
			
				bool	Result			= false;

				double	VecX			= 0.0;
				double	VecY			= 0.0;
				double	VecZ			= 0.0;
			
				double	PosX			= 0.0;
				double	PosY			= 0.0;
				double	PosZ			= 0.0;

				double	VelX			= 0.0;
				double	VelY			= 0.0;
				double	VelZ			= 0.0;

				double	VecPosDotProd	= 0.0; 
				double	VecMag			= 0.0;
				double	PosMag			= 0.0;
				double	Theta			= 0.0;
			
				#endregion

				//========================================
				// 1. Get the "User Choosen" Vector XYZ
				// 2. Get the Position Vector XYZ in 
				//    Central Body Inertial Frame ( CBI )
				//========================================
				if( ResultEval != null )
				{
					ResultEval.PosVel( AgEUtFrame.eUtFrameInertial, ref PosX, ref PosY, ref PosZ, ref VelX, ref VelY, ref VelZ );
					Result =  this.m_CCV.CurrentValue( ResultEval, ref VecX, ref VecY, ref VecZ );
			
					if( Result )
					{
						//===============================================================
						// Calculate the angle (Theta in radians) between the two vectors
						// 1. Calculate the Dot Product
						// 2. Calculate the 
						//================================================================
						VecPosDotProd	= (( VecX * PosX ) + ( VecY * PosY ) + ( VecZ * PosZ )); 
						VecMag			= Math.Sqrt( ( Math.Pow( VecX, 2 ) + Math.Pow( VecY, 2 ) + Math.Pow( VecZ, 2 )));
						PosMag			= Math.Sqrt( ( Math.Pow( PosX, 2 ) + Math.Pow( PosY, 2 ) + Math.Pow( PosZ, 2 )));
						Theta			= Math.Acos( VecPosDotProd / ( VecMag * PosMag ) );

						//===============================================
						// Calculate the new SRP area based on the Theta
						//===============================================

						// SRP must be on else this will throw an exception
						ResultEval.SRPArea = ( ( this.m_SRPArea / 4.0 ) * ( 3 - Math.Sin( Theta ) ) );
					
						if( this.m_UPS != null && this.m_MsgStatus )
						{
							if( this.m_EvalCntr % this.m_EvalMsgInterval == 0 )
							{
								double ThetaDeg = Theta * 57.2957795130823208767;

                                this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".EvaluateSRPArea( " + this.m_EvalCntr + " ): VecX( " + VecX + " ), VecY( " + VecY + " ), VecZ( " + VecZ + " ) meters/sec");
                                this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".EvaluateSRPArea( " + this.m_EvalCntr + " ): PosX(" + PosX + " ), PosY( " + PosY + " ), PosZ( " + PosZ + " ) meters");
                                this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".EvaluateSRPArea( " + this.m_EvalCntr + " ): VelX(" + VelX + " ), VelY( " + VelY + " ), VelZ(" + VelZ + " ) meters/sec");
                                this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".EvaluateSRPArea( " + this.m_EvalCntr + " ): SRPArea(" + ResultEval.SRPArea + " m^2), Theta( " + ThetaDeg + " deg)");
							}
						}
					}
					else
					{
						if( this.m_UPS != null && this.m_MsgStatus )
						{
							if( this.m_EvalCntr % this.m_EvalMsgInterval == 0 )
							{
                                this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".EvaluateSRPArea( " + this.m_EvalCntr + " ): Result( " + Result + " )");
							}
						}
					}
				}
				else
				{
					if( this.m_UPS != null && this.m_MsgStatus )
					{
                        this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgWarning, this.m_Name + ".Crdn Configured Vector or Result Eval was null");
					}
				}
			}
			catch( Exception ex )
			{
				this.m_Enabled = false;

				if( this.m_UPS != null )
				{
                    this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgAlarm, this.m_Name + ".EvaluateSRPArea(): Exception Message( " + ex.Message + " )");
                    this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgAlarm, this.m_Name + ".EvaluateSRPArea(): Exception StackTr( " + ex.StackTrace + " )");
				}
				Debug.WriteLine( "Exception Message( " + ex.Message + " )", this.m_Name+".EvaluateSRPArea()" );
				Debug.WriteLine( "Exception StackTr( " + ex.StackTrace + " )", this.m_Name+".EvaluateSRPArea()");
			}
			finally
			{
				if( this.m_EvalCntr % this.m_EvalMsgInterval == 0 )
				{
					Debug.WriteLine( "<-- Exited", this.m_Name+".EvaluateSRPArea( " + this.m_EvalCntr + " )" );
				}
			}
		}

		public bool PostEvaluate( IAgAsHpopPluginResultPostEval ResultEval )
		{
			try
			{
				if( this.m_PostEvalCntr % this.m_PostEvalMsgInterval == 0 )
				{
					Debug.WriteLine( "--> Entered", this.m_Name+".PostEvaluate( " + this.m_PostEvalCntr + " )" );
				}

				this.m_PostEvalCntr++;

				if( this.m_UPS != null )
				{
					if( this.m_Enabled )
					{
						if( this.m_MsgStatus )
						{
							if( this.m_PostEvalCntr % this.m_PostEvalMsgInterval == 0 )
							{
								AgEUtFrame		reportFrame = AgEUtFrame.eUtFrameNTC;
								string			frameName = "NTC";

								AgEAccelType	accelType = AgEAccelType.eSRPAccel;
								double			srpX=0.0, srpY=0.0, srpZ=0.0, AltInKm;

								AltInKm = ResultEval.Altitude*0.001;

                                this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, 
									this.m_Name+".PostEvaluate( " + this.m_PostEvalCntr +" ): SRPArea (" +
									ResultEval.SRPArea+" m^2), Altitude ("+AltInKm+" km)" );

								ResultEval.GetAcceleration( accelType, reportFrame, ref srpX, ref srpY, ref srpZ );

                                this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, 
									this.m_Name+".PostEvaluate( " + this.m_PostEvalCntr +" ): SRPAccel (" +
									frameName+") is ("+srpX+", "+srpY+", "+srpZ+") meters/secs^2" );

								// report out the added acceleration in NTC components
								double	thrustX =0.0, thrustY=0.0, thrustZ=0.0;
								accelType = AgEAccelType.eAddedAccel;

								ResultEval.GetAcceleration( accelType, reportFrame, ref thrustX, ref thrustY, ref thrustZ );

                                this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, 
									this.m_Name+".PostEvaluate( " + this.m_PostEvalCntr +" ): ThrustAccel (" +
									frameName+") is ("+thrustX+", "+thrustY+", "+thrustZ+") meters/secs^2" );
							}
						}
					}
					else
					{
						if( this.m_MsgStatus )
						{
                            this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".PostEvaluate(): Disabled");
						}
					}
				}
				else
				{
					throw new Exception( "UtPluginSite was null" );
				}
			}
			catch( Exception ex )
			{
				this.m_Enabled = false;

				if( this.m_UPS != null )
				{
                    this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgAlarm, this.m_Name + ".PostEvaluate(): Exception Message( " + ex.Message + " )");
                    this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgAlarm, this.m_Name + ".PostEvaluate(): Exception StackTr( " + ex.StackTrace + " )");
				}
				Debug.WriteLine( "Exception Message( " + ex.Message + " )", this.m_Name+".PostEvaluate()" );
				Debug.WriteLine( "Exception StackTr( " + ex.StackTrace + " )", this.m_Name+".PostEvaluate()");
			}
			finally
			{
				if( this.m_PostEvalCntr % this.m_PostEvalMsgInterval == 0 )
				{
					Debug.WriteLine( "<-- Exited", this.m_Name+".PostEvaluate( " + this.m_PostEvalCntr + " )" );
				}
			}

			return this.m_Enabled;
		}

		public bool PostPropagate( IAgAsHpopPluginResult Result )
		{
			try
			{
				Debug.WriteLine( "--> Entered", this.m_Name+".PostPropagate()");

				if( this.m_UPS != null )
				{
					if( this.m_Enabled )
					{
						if( this.m_MsgStatus )
						{
                            this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".PostPropagate():");
						}
					}	
					else
					{
						if( this.m_MsgStatus )
						{
                            this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".PostPropagate(): Disabled");
						}
					}
				}
				else
				{
					throw new Exception( "UtPluginSite was null" );
				}
			}
			catch( Exception ex )
			{
				this.m_Enabled = false;

				if( this.m_UPS != null )
				{
                    this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgAlarm, this.m_Name + ".PostPropagate(): Exception Message( " + ex.Message + " )");
                    this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgAlarm, this.m_Name + ".PostPropagate(): Exception StackTr( " + ex.StackTrace + " )");
				}
				Debug.WriteLine( "Exception Message( " + ex.Message + " )", this.m_Name+".PostPropagate()" );
				Debug.WriteLine( "Exception StackTr( " + ex.StackTrace + " )", this.m_Name+".PostPropagate()");
			}
			finally
			{
				Debug.WriteLine( "<-- Exited", this.m_Name+".PostPropagate()" );
			}

			return this.m_Enabled;
		}

		public void Free()
		{
			try
			{
				Debug.WriteLine( "--> Entered", this.m_Name+".Free()");

				if( this.m_UPS != null)
				{
					if ( this.m_MsgStatus )
					{
                        this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".Free():");
                        this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".Free(): PreNextCntr( " + this.m_PreNextCntr + " )");
                        this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".Free(): EvalCntr( " + this.m_EvalCntr + " )");
                        this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgDebug, this.m_Name + ".Free(): PostEvalCntr( " + this.m_PostEvalCntr + " )");
					}

					Marshal.ReleaseComObject( this.m_UPS );
				}
				this.m_UPS = null;

				if(this.m_SPS != null)
				{
					Marshal.ReleaseComObject( this.m_SPS );
				}
				this.m_SPS = null;

				if(this.m_CPP != null)
				{
					Marshal.ReleaseComObject( this.m_CPP );
				}
				this.m_CPP = null;

				if(this.m_CCV != null)
				{
					Marshal.ReleaseComObject( this.m_CCV );	
				}
				this.m_CCV = null;
			}
			catch( Exception ex )
			{
				if( this.m_UPS != null  )
				{
                    this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgAlarm, this.m_Name + ".Free(): Exception Message( " + ex.Message + " )");
                    this.m_UPS.Message( AgEUtLogMsgType.eUtLogMsgAlarm, this.m_Name + ".Free(): Exception StackTr( " + ex.StackTrace + " )");
				}
				Debug.WriteLine( "Exception Message( " + ex.Message + " )", this.m_Name+".Free()" );
				Debug.WriteLine( "Exception StackTr( " + ex.StackTrace + " )", this.m_Name+".Free()");
			}
			finally
			{
				Debug.WriteLine( "<-- Exited", this.m_Name+".Free()" );
			}
		}
		#endregion

		#region IAgUtPluginConfig Interface Implementation
		public object GetPluginConfig( AgAttrBuilder aab )
		{
			try
			{
                bool bUseArray = true;
				Debug.WriteLine( "--> Entered", this.m_Name+".GetPluginConfig()");

				if( this.m_Scope == null )
				{
					this.m_Scope = aab.NewScope();
				
					//===========================
					// General Plugin attributes
					//===========================
					aab.AddStringDispatchProperty( this.m_Scope, "PluginName", "Human readable plugin name or alias", "Name", (int)AgEAttrAddFlags.eAddFlagNone );
					aab.AddBoolDispatchProperty  ( this.m_Scope, "PluginEnabled", "If the plugin is enabled or has experience an error", "Enabled", (int)AgEAttrAddFlags.eAddFlagNone );
					aab.AddStringDispatchProperty( this.m_Scope, "VectorName", "Vector Name that affects the srp area", "VectorName", (int)AgEAttrAddFlags.eAddFlagNone );
				
					//===========================
					// Propagation related
					//===========================
                    if (bUseArray)
                    {
                        aab.AddChoicesDispatchProperty(this.m_Scope, "AccelRefFrame", "Acceleration Reference Frame", "AccelRefFrame", this.AccelRefFrameChoices);
                    }
                    else
                    {
                        aab.AddChoicesFuncDispatchProperty(this.m_Scope, "AccelRefFrame", "Acceleration Reference Frame", "AccelRefFrame", "AccelRefFrameChoices");
                    }
					aab.AddDoubleDispatchProperty( this.m_Scope, "AccelX", "Acceleration in the X direction", "AccelX", (int)AgEAttrAddFlags.eAddFlagNone );
					aab.AddDoubleDispatchProperty( this.m_Scope, "AccelY", "Acceleration in the Y direction", "AccelY", (int)AgEAttrAddFlags.eAddFlagNone );
					aab.AddDoubleDispatchProperty( this.m_Scope, "AccelZ", "Acceleration in the Z direction", "AccelZ", (int)AgEAttrAddFlags.eAddFlagNone );
				
					//===========================
					// Messaging related attributes
					//===========================
					aab.AddBoolDispatchProperty( this.m_Scope, "UsePropagationMessages", "Send messages to the message window during propagation", "MsgStatus", (int)AgEAttrAddFlags.eAddFlagNone );
					aab.AddIntDispatchProperty( this.m_Scope, "EvaluateMessageInterval", "The interval at which to send messages from the Evaluate method during propagation", "EvalMsgInterval", (int)AgEAttrAddFlags.eAddFlagNone );
					aab.AddIntDispatchProperty( this.m_Scope, "PostEvaluateMessageInterval", "The interval at which to send messages from the PostEvaluate method during propagation", "PostEvalMsgInterval", (int)AgEAttrAddFlags.eAddFlagNone );
					aab.AddIntDispatchProperty( this.m_Scope, "PreNextStepMessageInterval", "The interval at which to send messages from the PreNextStep method during propagation", "PreNextMsgInterval", (int)AgEAttrAddFlags.eAddFlagNone );
				}
				string config;
				config = aab.ToString( this, this.m_Scope );
				Debug.WriteLine( this.m_Name+".GetPluginConfig():");
				Debug.WriteLine( "============Attr Scope==============");
				Debug.WriteLine( config );
				Debug.WriteLine( "====================================");
			}
			finally
			{
				Debug.WriteLine( "<-- Exited", this.m_Name+".GetPluginConfig()");
			}

			return this.m_Scope;
		}

		public void VerifyPluginConfig( AgUtPluginConfigVerifyResult apcvr )
		{
			bool	result	= true;
			string	message = "Ok";

			try
			{
				Debug.WriteLine( "--> Entered", this.m_Name+".VerifyPluginConfig()" );

				//====================================================
				// NOTE:
				// The following reasonable acceleration limits are 
				// added only to show a sample of code that checks
				// for a range of attribute values that you impose 
				// on a user using the plugin.
				//====================================================
				if( !( this.m_AccelX <= 10 && this.m_AccelX >= -10 ) )
				{
					throw new Exception( "AccelX was not within the range of -10 to +10 meters per second squared");
				}
				else if( !( this.m_AccelY <= 10 && this.m_AccelY >= -10 ) )
				{
					throw new Exception( "AccelY was not within the range of -10 to +10 meters per second squared");
				}
				else if( !( this.m_AccelZ <= 10 && this.m_AccelZ >= -10 ) )
				{
					throw new Exception( "AccelZ was not within the range of -10 to +10 meters per second squared");
				}
			}
			catch( Exception ex )
			{
				Debug.WriteLine( "Exception Message( " + ex.Message + " )", this.m_Name+".VerifyPluginConfig()" );

				result  = false;
				message = ex.Message;
			}
			finally
			{
				Debug.WriteLine( "<-- Exited", this.m_Name+".VerifyPluginConfig()" );
			}

			apcvr.Result	= result;
			apcvr.Message	= message;
		}
		#endregion

        #region Registration functions
        /// <summary>
        /// Called when the assembly is registered for use from COM.
        /// </summary>
        /// <param name="t">The type being exposed to COM.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        public static void RegisterFunction(Type t)
        {
            RemoveOtherVersions(t);
        }

        /// <summary>
        /// Called when the assembly is unregistered for use from COM.
        /// </summary>
        /// <param name="t">The type exposed to COM.</param>
        [ComUnregisterFunctionAttribute]
        [ComVisible(false)]
        public static void UnregisterFunction(Type t)
        {
            // Do nothing.
        }

        /// <summary>
        /// Called when the assembly is registered for use from COM.
        /// Eliminates the other versions present in the registry for
        /// this type.
        /// </summary>
        /// <param name="t">The type being exposed to COM.</param>
        public static void RemoveOtherVersions(Type t)
        {
            try
            {
                using (RegistryKey clsidKey = Registry.ClassesRoot.OpenSubKey("CLSID"))
                {
                    StringBuilder guidString = new StringBuilder("{");
                    guidString.Append(t.GUID.ToString());
                    guidString.Append("}");
                    using (RegistryKey guidKey = clsidKey.OpenSubKey(guidString.ToString()))
                    {
                        if (guidKey != null)
                        {
                            using (RegistryKey inproc32Key = guidKey.OpenSubKey("InprocServer32", true))
                            {
                                if (inproc32Key != null)
                                {
                                    string currentVersion = t.Assembly.GetName().Version.ToString();
                                    string[] subKeyNames = inproc32Key.GetSubKeyNames();
                                    if (subKeyNames.Length > 1)
                                    {
                                        foreach (string subKeyName in subKeyNames)
                                        {
                                            if (subKeyName != currentVersion)
                                            {
                                                inproc32Key.DeleteSubKey(subKeyName);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignore all exceptions...
            }
        }
        #endregion
	}
}
//=====================================================//
//  Copyright 2005, Analytical Graphics, Inc.          //
//=====================================================//
