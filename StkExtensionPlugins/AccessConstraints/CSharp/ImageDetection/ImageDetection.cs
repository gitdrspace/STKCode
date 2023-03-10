//=====================================================//
//  Copyright 2005, Analytical Graphics, Inc.          //
//=====================================================//
using AGI.Attr;
using AGI.Plugin;
using AGI.VectorGeometryTool.Plugin;
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace AGI.Access.Constraint.Plugin.ImageDetection
{
	/// <summary>
	/// Constraint Plugin component NIIRS_Constraint
	/// Original Author:     Tom Johnson
	/// Converted to C#  by:	 Vince Coppola
	/// Company:    Analytical Graphics, Inc.
	/// Copyright:  None.  Modify and distribute at will
	///
	/// Description:
	/// 
	/// This constraint is registered only for Facilities/Targets when doing
	/// Access to a Sensor.
	///
	/// This constraint calculates a modified form of the NIIRS image quality
	/// metric.  It's modified in that it doesn't calculate all the terms since
	/// I didn't have enough information to do so.  The first step is to calculate
	/// the Ground Sample Distance (GSD) for a sensor
	/// viewing another object.  This version of the GSD equation is parameterized
	/// in terms of Q, the optical ratio. Reference document is 
	/// ?Image Quality and lamdaFN/p For Remote Sensing Systems,? Robert D. Fiete, 
	/// Optical Engineering, Vol. 38 No 7, July 1999
	///
	///                  SR * lamda
	///       GSD =  --------------------------
	///              Q * D * Math.Sqrt( sin(elev)  )
	///
	/// where
	///
	///       SR = slant range (meters) from STK
	///
	///       lamda = wavelenth of the sensor you are observing with
	///
	///       Q     = optical ratio (unitless)
	///
	///       D     = optical diameter (meters)
	///
	///       elev  = elevation angle between the ground object and the sensor (from STK)
	///
	/// The assumption is that the user provides lamda, Q, and D within the script and
	/// STK does the rest.  GSD is returned in meters.  Then we pass it into the NIIRS
	/// equation and return the NIIRS value.
	/// </summary>

	[Guid("A52A0602-BA50-42fc-8893-45DC81F2DE7D")]
    [ProgId("AGI.Access.Constraint.Plugin.ImageDetection")]
	// NOTE: Specify the ClassInterfaceType.None enumeration, so the custom COM Interface 
	// you created, i.e. IParameters, is used instead of an autogenerated COM Interface.
	[ClassInterface(ClassInterfaceType.None)]
	public class ImageDetection : 
		IParameters,
		IAgAccessConstraintPlugin,
		IAgUtPluginConfig
	{
		#region Data Members
		private string	m_DisplayName = "ImageDetection";

		private IAgUtPluginSite  m_Site;
		private object			 m_Scope;	
		private Hashtable	     m_AxesHash = new Hashtable();

		// sensor properties

		private double			m_lamda;	// Sensor wavelenth (microns)
		private double			m_diameter;	// Optical diameter (meters)
		private double			m_Q;		// Optical ratio (unitless). 
        private double m_size;

		///  Optical ratio (unitless). 
		/// 
		///                 f
		///    Q = lamda * ---
		///                 D
		///        ------------
		///              pp
		/// 
		///    where
		/// 
		///    f = focal length (meters)
		/// 
		///    pp = pixel pitch (meters)
		/// 

		// NIIRS coefficients

		private double			m_a;
		private double			m_b;
		private double			m_logRER;

		// debug

		private bool			m_DebugMode;
		private int				m_MsgCntr;
		private int				m_MsgInterval;

		// conversions

		static private double			s_micron2meter = 1.0e-6;
		static private double			s_meters2inches = 39.37007874015876;

		// ellipsoid shape

		static private double			s_invASqr = 1.0 /(6378137.0 * 6378137.0);
		static private double			s_invCSqr = 1.0 /(6356752.31424 * 6356752.31424);

		#endregion
		
		public ImageDetection()
		{
			// defaults

			m_Site = null;
			m_Scope = null;

			m_lamda = 0.65;			// 700 nanometers = 0.7 microns
			m_diameter = .085;
            m_Q = .4316 / 0.9406680947115166;

			m_a   = 3.32;
			m_b   = 1.559;
			m_logRER = Math.Log10(0.9);

            m_size = 3;

			m_DebugMode = false;	// NOTE: if true, will output a msg when
									// entering events other than Evaluate().
									//
									// DON'T set to true when using constraint as a
									// Figure of Merit,because PreCompute() and PostCompute()
									// are called once per animation step, which will cause
									// lots of messages to be written to the Message Viewer.
			m_MsgCntr = 0;
			m_MsgInterval = 100;
		}
		
		#region IAgAccessConstraintPlugin implementation

		public string DisplayName
		{
			get
			{
				return m_DisplayName;
			}
		}

		public void Register( AgAccessConstraintPluginResultRegister Result )
		{
			Result.BaseObjectType = AgEAccessConstraintObjectType.eFacility;
			Result.BaseDependency = (int)AgEAccessConstraintDependencyFlags.eDependencyRelativePosVel;
			Result.Dimension = "Unitless";	
			Result.MinValue = 0.0;
		
			Result.TargetDependency = (int)AgEAccessConstraintDependencyFlags.eDependencyNone;
			Result.AddTarget(AgEAccessConstraintObjectType.eSensor);
			Result.Register();

			Result.Message(AGI.Plugin.AgEUtLogMsgType.eUtLogMsgInfo, 
						m_DisplayName+": Register(Facility to Sensor)");
		
			Result.BaseObjectType = AgEAccessConstraintObjectType.eTarget;
			Result.Register();

			Result.Message(AGI.Plugin.AgEUtLogMsgType.eUtLogMsgInfo, 
				m_DisplayName+": Register(Target to Sensor)");

			Result.BaseObjectType = AgEAccessConstraintObjectType.ePlace;
			Result.Register();

			Result.Message(AGI.Plugin.AgEUtLogMsgType.eUtLogMsgInfo,
				m_DisplayName + ": Register(Place to Sensor)");
		}

		public bool Init( IAgUtPluginSite site )
		{
			m_Site = site;
			
			if( m_Site != null && m_DebugMode)
			{
				Message( AgEUtLogMsgType.eUtLogMsgInfo, m_DisplayName+": Init()" );
			}
			m_AxesHash.Clear();

			return true;
		}

		public bool PreCompute( AgAccessConstraintPluginResultPreCompute Result )
		{
			// Get the topocentric Axes from the Vector Tool,
			// for the Facility/Target, to be used in the computation later

			AgAccessConstraintPluginObjectDescriptor baseDesc = Result.Base;
			string basePath = baseDesc.ObjectPath;

			if(basePath != "")
			{
				if(!m_AxesHash.ContainsKey(basePath))
				{
					AgCrdnPluginProvider vgtProvider = baseDesc.VectorToolProvider;

					if(vgtProvider != null)
					{
						string cbName = "Earth";//
						cbName = baseDesc.CentralBodyName;
						cbName = "CentralBody/"+cbName; 

						AgCrdnConfiguredAxes topoAxes = vgtProvider.ConfigureAxes(
							"Fixed", cbName, "TopoCentric", "");

						if(topoAxes != null && topoAxes.IsConfigured)
						{
							m_AxesHash.Add(basePath, topoAxes);
						}
					}
				}
			}
			
			if( m_Site != null && m_DebugMode)
			{
				Message( AgEUtLogMsgType.eUtLogMsgInfo, m_DisplayName+": PreCompute()" );
			}

			return true;
		}

	
		public bool Evaluate( 
			AgAccessConstraintPluginResultEval Result, 
			AgAccessConstraintPluginObjectData baseObj, 
			AgAccessConstraintPluginObjectData targetObj )
		{
			if(Result != null)
			{
				Result.Value = 0.0;
			
				if( baseObj != null)
				{
					double range = baseObj.Range(
						AGI.Access.Constraint.Plugin.AgEAccessApparentPositionType.eProperApparentPosition);
					
					double detectability = -1.0, relX = 0.0, relY = 0.0, relZ = 0.0;

					baseObj.RelativePosition(
						AGI.Access.Constraint.Plugin.AgEAccessApparentPositionType.eProperApparentPosition,
						AGI.Plugin.AgEUtFrame.eUtFrameFixed, ref relX, ref relY, ref relZ);

					bool usePosToComputeImageDetection = true;

					AgAccessConstraintPluginObjectDescriptor baseDesc = baseObj.Descriptor;
					string basePath = baseDesc.ObjectPath;

					if(basePath != "" && m_AxesHash.ContainsKey(basePath))
					{
						AgCrdnConfiguredAxes topoAxes = (AgCrdnConfiguredAxes) m_AxesHash[basePath];

						if(topoAxes != null)
						{
							topoAxes.TransformComponents(baseObj, ref relX, ref relY, ref relZ);

							double sinElev = relZ / range;

                            detectability = computeDetectability(range, sinElev, m_size);

							usePosToComputeImageDetection = false;
						}
					}
						
					if(usePosToComputeImageDetection)
					{
						// will only work with Facility/Targets on Earth
						double x = 0.0, y = 0.0, z = 0.0;
						baseObj.Position(AGI.Plugin.AgEUtFrame.eUtFrameFixed, ref x, ref y, ref z);
                        detectability = computeImageDetectionFromPos(range, x, y, z, relX, relY, relZ);
					}

                    Result.Value = detectability;
				}
			}

			return true;
		}
		
		public bool PostCompute(AgAccessConstraintPluginResultPostCompute Result)
		{
			if( m_Site != null && m_DebugMode)
			{
				Message( AgEUtLogMsgType.eUtLogMsgInfo, m_DisplayName+": PostCompute()" );
			}
			return true;
		}

		public void Free()
		{
			m_AxesHash.Clear();

			m_AxesHash = null;

			if( m_Site != null && m_DebugMode)
			{
				Message( AgEUtLogMsgType.eUtLogMsgInfo, m_DisplayName+": Free()" );
			}

			m_Site = null;
		}
		
		#endregion

		#region ImageDetection Computation code

        private double computeGSD(double range, double sinElev)
        {
            double GSD = -1;
            if (sinElev > 0.0)
            {
                // Now calculate the GSD. 

                GSD = range * m_lamda * s_micron2meter / (m_Q * m_diameter * Math.Sqrt(sinElev));
            }

            return GSD;

        }

        public double computeDetectability(double range, double sinElev, double targetSize)
        {
            double GSD = computeGSD(range, sinElev);

            double DetProb = (.25 * targetSize / GSD) * 100;

            if (DetProb > 100) DetProb = 100;
            
            return DetProb;
        }



		public double computeImageDetection(double range, double sinElev)
		{
			double ImageDetection = -1.0;

			// Check for negative elevation angle (e.g. z component of relative
			// position vector is negative.  If so, bail out gracefully.
            double GSD = -1;
			if (sinElev > 0.0 )
			{
				// Now calculate the GSD. 

				GSD = range * m_lamda * s_micron2meter / (m_Q * m_diameter * Math.Sqrt(sinElev));
        
				// Now calculate the NIIRS value.  
				// Terms after the RER are truncated since I don't have any values
				// to use for them.
        
				// NIIRS value
               
				ImageDetection = 10.251 - m_a * Math.Log10(GSD * s_meters2inches) + m_b * m_logRER;
			}

			return GSD;
		}

		public double computeImageDetectionFromPos(double range, double x, double y, double z,
									double relX, double relY, double relZ)
		{
			double sinElev = computeSinElev(x, y, z, relX, relY, relZ);

			double ImageDetection = computeImageDetection(range, sinElev);

			return ImageDetection;
		}

		public double computeSinElev(double x, double y, double z,
										double relX, double relY, double relZ)
		{
			// this rotuine computes the detic elevation angle (ie the angle
			// from the tangent plane at the Facility/Target to the Sensor,
			// where the tangent plane is tangent to the Earth as represented by the
			// WGS84 oblate spheroid)

			double sinElev = 0.0;

			double nx = 0.0, ny = 0.0, nz = 0.0;

			computeSurfNormalVec(x, y, z, ref nx, ref ny, ref nz);

			double relPosNormal = nx * relX + ny * relY + nz * relZ;

			double magRelPos = Math.Sqrt (relX*relX + relY*relY + relZ*relZ );

			sinElev = relPosNormal / magRelPos;

			return sinElev;
		}

		public void computeSurfNormalVec(double x, double y, double z,
										ref double nx, ref double ny, ref double nz)
		{
			nx = ny = 0.0; nz = 1.0;

			double tempXY = ( x*x + y*y ) * s_invASqr;
			double tempZ  = z*z*s_invCSqr;

			double alpha = Math.Sqrt(tempXY + tempZ);

			if(alpha > 0.1)
			{
				double radx = x / alpha;
				double rady = y / alpha;
				double radz = z / alpha;

				double normx = radx * s_invASqr;
				double normy = rady * s_invASqr;
				double normz = radz * s_invCSqr;

				double normMag = Math.Sqrt( normx*normx + normy*normy + normz*normz);

				double radMag = Math.Sqrt( x*x + y*y + z*z);

				double lam = (1.0 - 1.0 / alpha) * radMag / normMag;

				// Newton iteration

				double xyDenom  = 1.0 / (1.0 + lam*s_invASqr);
				double zDenom   = 1.0 / (1.0 + lam*s_invCSqr);

				double xyDenom2, zDenom2;

				double F = 1.0; double dF;

				while (Math.Abs(F) > 1.0e-12)
				{
					xyDenom2 = xyDenom*xyDenom;
					zDenom2  = zDenom*zDenom;

					/*
					 * The function F is the surface constraint, dF is the
					 * partial of F with respect to alpha
					 */
					F  = tempXY*xyDenom2 + tempZ*zDenom2 - 1.0;
					dF = -2.0 * (tempXY*xyDenom2*xyDenom*s_invASqr + 
						tempZ*zDenom2*zDenom*s_invCSqr);

					/* Newton - Raphson update */
					alpha = alpha - F/dF;

					xyDenom  = 1.0 / (1.0 + alpha*s_invASqr);
					zDenom   = 1.0 / (1.0 + alpha*s_invCSqr);
				}

				/*
				 * The detic vector is computed via equations of the form
				 * Sx = Px / (1.0 + alpha/a^2)
				 * which come from the relation Sx + alpha (Sx/a^2) = Px
				 */
				double deticx = x * xyDenom;
				double deticy = y * xyDenom;
				double deticz = z * zDenom;

				// surface normal

				nx = deticx * s_invASqr;
				ny = deticy * s_invASqr;
				nz = deticz * s_invCSqr;

				double nMag = Math.Sqrt (nx*nx + ny*ny + nz*nz);

				nx /= nMag;
				ny /= nMag;
				nz /= nMag;
			}
		}

		#endregion

		#region IAgUtPluginConfig Interface Implementation
		public object GetPluginConfig( AgAttrBuilder aab )
		{
			try
			{
				if( this.m_Scope == null )
				{
					this.m_Scope = aab.NewScope();

                    //aab.AddQuantityDispatchProperty ( this.m_Scope, "Diameter", 
                    //    "Optical diameter", 
                    //    "Diameter", 
                    //    "Meters",
                    //    "Meters",
                    //    (int)AgEAttrAddFlags.eAddFlagNone );

                    //aab.AddQuantityDispatchProperty ( this.m_Scope, "Wavelength", 
                    //    "Wavelength of the sensor you are observing with", 
                    //    "Wavelength", 
                    //    "Meters",
                    //    "Meters",
                    //    (int)AgEAttrAddFlags.eAddFlagNone );

                    //aab.AddDoubleDispatchProperty ( this.m_Scope, "OpticalRatio", 
                    //    "Optical ratio (Q)", 
                    //    "OpticalRatio", 
                    //    (int)AgEAttrAddFlags.eAddFlagNone );

                    //aab.AddDoubleDispatchProperty ( this.m_Scope, "NIIRS_a", 
                    //    "The a coefficient for the NIIRS equation", 
                    //    "NIIRS_a", 
                    //    (int)AgEAttrAddFlags.eAddFlagNone );

                    //aab.AddDoubleDispatchProperty ( this.m_Scope, "NIIRS_b", 
                    //    "The b coefficient for the NIIRS equation", 
                    //    "NIIRS_b", 
                    //    (int)AgEAttrAddFlags.eAddFlagNone );

                    //aab.AddDoubleDispatchProperty ( this.m_Scope, "NIIRS_RER", 
                    //    "The RER coefficient for the NIIRS equation", 
                    //    "NIIRS_RER", 
                    //    (int)AgEAttrAddFlags.eAddFlagNone );

                    aab.AddDoubleDispatchProperty(this.m_Scope, "TargetSize",
                        "The rough diameter of the target cross section",
                        "TargetSize",
                        (int)AgEAttrAddFlags.eAddFlagNone);
				
					//===========================
					// Debug attributes
					//===========================
					aab.AddBoolDispatchProperty( this.m_Scope, "DebugMode", 
						"Turn debug messages on or off", 
						"DebugMode", 
						(int)AgEAttrAddFlags.eAddFlagNone );
				
					aab.AddIntDispatchProperty( this.m_Scope, "MessageInterval", 
						"The interval at which to send messages during propagation in Debug mode", 
						"MsgInterval", 
						(int)AgEAttrAddFlags.eAddFlagNone );
				}
			}
			finally
			{

			}
	
			return this.m_Scope;
		}

		public void VerifyPluginConfig( AgUtPluginConfigVerifyResult apcvr )
		{
			bool	result	= true;
			string	message = "Ok";

			if(m_diameter < 0.000001)
			{
				result = false;
				message = "Diameter is too small.";
			}
			
			apcvr.Result	= result;
			apcvr.Message	= message;
		}

#endregion

		#region IParameters Interface Implementation

		public double Diameter
		{
			get
			{
				return this.m_diameter;
			}
			set
			{
				this.m_diameter = value;
			}
		}

		public double Wavelength
		{
			get
			{
				return this.m_lamda;
			}
			set
			{
				this.m_lamda = value;
			}
		}

		public double OpticalRatio
		{
			get
			{
				return this.m_Q;
			}
			set
			{
				this.m_Q = value;
			}
		}

		public double NIIRS_a
		{
			get
			{
				return this.m_a;
			}
			set
			{
				this.m_a = value;
			}
		}

		public double NIIRS_b
		{
			get
			{
				return this.m_b;
			}
			set
			{
				this.m_b = value;
			}
		}

        public double TargetSize
        {
            get
            {
                return this.m_size;
            }
            set
            {
                this.m_size = value;
            }
        }

		public double NIIRS_RER
		{
			get
			{
				return Math.Pow(10, this.m_logRER);
			}
			set
			{
				if(value > 0.0)
				{
					this.m_logRER = Math.Log10(value);
				}
			}
		}

		public bool DebugMode
		{
			get
			{
				return this.m_DebugMode;
			}
			set
			{
				this.m_DebugMode = value;
			}
		}

		public int MsgInterval
		{
			get
			{
				return this.m_MsgInterval;
			}
			set
			{
				this.m_MsgInterval = value;
			}
		}

		#endregion

		#region Messaging Code

		private void Message (AgEUtLogMsgType severity, String msgStr)
		{
			if(  this.m_Site != null )
			{
				this.m_Site.Message( severity, msgStr);
			}
		}

		private void DebugMessage(String msgStr)
		{
			if(m_DebugMode)
			{
				if(m_MsgCntr % m_MsgInterval == 0)
				{
					Message(AgEUtLogMsgType.eUtLogMsgDebug, msgStr);
				}
			}
		}

		#endregion
	}
}
//=====================================================//
//  Copyright 2006, Analytical Graphics, Inc.          //
//=====================================================//