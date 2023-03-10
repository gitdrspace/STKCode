'//=====================================================//
'//  Copyright 2006, Analytical Graphics, Inc.          //
'//=====================================================//

Option Strict On
Imports System.EnterpriseServices
Imports System.Runtime.InteropServices
Imports Microsoft.Win32.RegistryKey

Imports AGI.Attr
Imports AGI.Plugin
Imports AGI.Hpop.Plugin
Imports AGI.VectorGeometryTool.Plugin
Imports AGI.STK.Plugin

' NOTE: Generate your own Guid using Microsoft's GuidGen.exe
    ' NOTE: Create your own ProgId to match your plugin's namespace and name
    ' NOTE: Specify the ClassInterfaceType.None enumeration, so the custom COM Interface 
    ' you created, i.e. IExample1, is used instead of an autogenerated COM Interface.
<JustInTimeActivation(True), _
GuidAttribute("F1404834-7E09-4b03-8C44-EEA1036D232F"), _
ProgId("AGI.Hpop.Plugin.Examples.Stk.ForceModeling.VB_NET.Example1"), _
ClassInterface(ClassInterfaceType.None)> _
Public Class Example1
    Implements IExample1
    Implements IAgAsHpopPlugin
    Implements IAgUtPluginConfig

#Region "Plugin Private Data Members"

    Private m_UPS As IAgUtPluginSite
    Private m_SPS As IAgStkPluginSite
    Private m_CPP As AgCrdnPluginProvider
    Private m_CCV As AgCrdnConfiguredVector
    Private m_Scope As Object
    Private m_PreNextCntr As Integer
    Private m_EvalCntr As Integer
    Private m_PostEvalCntr As Integer
    Private m_SrpIsOn As Boolean

#End Region

#Region "Life Cycle Methods"

    ' Default constructor.
    Public Sub New()
        MyBase.New()

        m_UPS = Nothing
        m_SPS = Nothing
        m_CPP = Nothing
        m_CCV = Nothing
        m_Scope = Nothing

        m_SRPArea = 0.0
        m_PreNextCntr = 0
        m_EvalCntr = 0
        m_PostEvalCntr = 0
        m_SrpIsOn = False

        Call SetAttributeConfigDefaults()

    End Sub

    '/// <summary>
    '/// Initializes the Plugin Attribute configuration 
    '/// Data Members to their defaults
    '/// </summary>
    Public Sub SetAttributeConfigDefaults()
        '/===========================
        '// General Plugin attributes
        '//===========================

        m_Name = "VB_Net.Example1"
        m_Enabled = True
        m_VectorName = "Periapsis"

        '//===========================
        '// Propagation related
        '//===========================
        m_AccelX = 0.0
        m_AccelY = 0.07
        m_AccelZ = 0.0

        m_AccelRefFrame = AgEUtFrame.eUtFrameNTC
        m_AccelRefFrameChoices = New Object(3) {}
        m_AccelRefFrameChoices(0) = AgEUtFrame.eUtFrameInertial.ToString()   '0
        m_AccelRefFrameChoices(1) = AgEUtFrame.eUtFrameFixed.ToString()      '1
        m_AccelRefFrameChoices(2) = AgEUtFrame.eUtFrameLVLH.ToString()       '2
        m_AccelRefFrameChoices(3) = AgEUtFrame.eUtFrameNTC.ToString()        '3

        '//===========================
        '// Logging related attributes
        '//===========================
        m_MsgStatus = False
        m_EvalMsgInterval = 5000
        m_PostEvalMsgInterval = 5000
        m_PreNextMsgInterval = 1000

    End Sub

#End Region

#Region "IExample1 Interface Implementation"
    '//=============================================
    '// Plugin Attributes to be configured
    '// will be exposed via .NET properties
    '// and using the Attribute Builder reference
    '// passed as a parameter to the GetPluginConfig
    '// Method.
    '//==============================================

    Private m_Name As String                    'Plugin Significant
    Private m_Enabled As Boolean                'Plugin Significant

    Private m_VectorName As String              'Init/Evaluate significant
    Private m_SRPArea As Double                 'Init/Evaluate significant

    Private m_AccelX As Double                  'Propagation Significant
    Private m_AccelY As Double                  'Propagation Significant
    Private m_AccelZ As Double                  'Propagation Significant

    Private m_AccelRefFrame As AgEUtFrame       'Propagation Significant
    Private m_AccelRefFrameChoices() As Object 'Propagation Significant

    Private m_MsgStatus As Boolean              'Uncessesary Added Feature
    Private m_EvalMsgInterval As Integer        'Uncessesary Added Feature
    Private m_PostEvalMsgInterval As Integer    'Uncessesary Added Feature
    Private m_PreNextMsgInterval As Integer     'Uncessesary Added Feature

    Public ReadOnly Property Name() As String Implements IAgAsHpopPlugin.Name
        Get
            Return m_Name
        End Get
    End Property

    Public Property MyName() As String Implements IExample1.MyName
        Get
            Return m_Name
        End Get
        Set(ByVal Value As String)
            m_Name = Value
        End Set
    End Property

    Public Property Enabled() As Boolean Implements IExample1.Enabled
        Get
            Return m_Enabled
        End Get
        Set(ByVal Value As Boolean)
            m_Enabled = Value
        End Set
    End Property

    Public Property VectorName() As String Implements IExample1.VectorName
        Get
            Return m_VectorName
        End Get
        Set(ByVal Value As String)
            m_VectorName = Value
        End Set
    End Property

    Public Property AccelRefFrame() As String Implements IExample1.AccelRefFrame
        Get
            Dim sAccelRefFrame As String
            sAccelRefFrame = "None"

            If (m_AccelRefFrame = AgEUtFrame.eUtFrameInertial) Then
                ' 0
                sAccelRefFrame = "eUtFrameInertial"
            ElseIf (m_AccelRefFrame = AgEUtFrame.eUtFrameFixed) Then
                ' 1
                sAccelRefFrame = "eUtFrameFixed"
            ElseIf (m_AccelRefFrame = AgEUtFrame.eUtFrameLVLH) Then
                ' 2
                sAccelRefFrame = "eUtFrameLVLH"
            ElseIf (m_AccelRefFrame = AgEUtFrame.eUtFrameNTC) Then
                ' 3
                sAccelRefFrame = "eUtFrameNTC"
            End If

            Return sAccelRefFrame
        End Get
        Set(ByVal Value As String)

            If (Value = "eUtFrameInertial") Then
                m_AccelRefFrame = AgEUtFrame.eUtFrameInertial
            ElseIf (Value = "eUtFrameFixed") Then
                m_AccelRefFrame = AgEUtFrame.eUtFrameFixed
            ElseIf (Value = "eUtFrameLVLH") Then
                m_AccelRefFrame = AgEUtFrame.eUtFrameLVLH
            ElseIf (Value = "eUtFrameNTC") Then
                m_AccelRefFrame = AgEUtFrame.eUtFrameNTC
            End If
        End Set
    End Property

    Public ReadOnly Property AccelRefFrameChoices() As Object() Implements IExample1.AccelRefFrameChoices
        Get
            Return m_AccelRefFrameChoices
        End Get
    End Property

    Public Property AccelX() As Double Implements IExample1.AccelX
        Get
            Return m_AccelX
        End Get
        Set(ByVal Value As Double)
            m_AccelX = Value
        End Set
    End Property

    Public Property AccelY() As Double Implements IExample1.AccelY
        Get
            Return m_AccelY
        End Get
        Set(ByVal Value As Double)
            m_AccelY = Value
        End Set
    End Property

    Public Property AccelZ() As Double Implements IExample1.AccelZ
        Get
            Return m_AccelZ
        End Get
        Set(ByVal Value As Double)
            m_AccelZ = Value
        End Set
    End Property

    Public Property MsgStatus() As Boolean Implements IExample1.MsgStatus
        Get
            Return m_MsgStatus
        End Get
        Set(ByVal Value As Boolean)
            m_MsgStatus = Value
        End Set
    End Property

    Public Property EvalMsgInterval() As Integer Implements IExample1.EvalMsgInterval
        Get
            Return m_EvalMsgInterval
        End Get
        Set(ByVal Value As Integer)
            m_EvalMsgInterval = Value
        End Set
    End Property

    Public Property PostEvalMsgInterval() As Integer Implements IExample1.PostEvalMsgInterval
        Get
            Return m_PostEvalMsgInterval
        End Get
        Set(ByVal Value As Integer)
            m_PostEvalMsgInterval = Value
        End Set
    End Property

    Public Property PreNextMsgInterval() As Integer Implements IExample1.PreNextMsgInterval
        Get
            Return m_PreNextMsgInterval
        End Get
        Set(ByVal Value As Integer)
            m_PreNextMsgInterval = Value
        End Set
    End Property

#End Region

#Region "IAgAsHpopPlugin Interface Implementation"

    Public Function Init(ByVal Ups As IAgUtPluginSite) As Boolean Implements IAgAsHpopPlugin.Init

        Try
            Debug.WriteLine("--> Entered", m_Name & ".Init()")

            m_UPS = Ups

            If (Not m_UPS Is Nothing And IsReference(m_UPS)) Then

                If (m_Enabled) Then

                    Try
                        m_SPS = DirectCast(m_UPS, IAgStkPluginSite)
                    Catch
                        ' failure indicates that m_UPS can't be cast to IAgStkPluginSite
                        m_SPS = Nothing
                    End Try

                    If (Not m_SPS Is Nothing) Then

                        If (IsReference(m_SPS)) Then
                            m_CPP = m_SPS.VectorToolProvider
                            If (Not m_CPP Is Nothing And IsReference(m_CPP)) Then
                                m_CCV = m_CPP.ConfigureVector(m_VectorName, "", "J2000", "CentralBody/Earth")
                            End If
                        End If

                        If (m_MsgStatus) Then
                            m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".Init():")
                            m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".Init(): AccelRefFrame( " & AccelRefFrame & " )")
                            m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".Init(): AccelX( " & m_AccelX & " )")
                            m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".Init(): AccelY( " & m_AccelY & " )")
                            m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".Init(): AccelZ( " & m_AccelZ & " )")
                        End If

                        If (m_CCV Is Nothing) Then
                            Call m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & "Init(): Could not obtain " & m_VectorName)
                            Call m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & "Init(): Turning off the computation of SRP Area")
                        End If

                    Else

                        m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".Init(): Could not obtain IAgStkPluginSite interface from" & m_UPS.SiteName)
                        Call m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & "Init(): Turning off the computation of SRP Area")

                        Return True
                    End If
                Else
                    If (m_MsgStatus) Then
                        m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".Init(): Disabled")
                    End If
                End If
            Else
                Throw New Exception("UtPluginSite was null")
            End If

        Catch ex As Exception
            m_Enabled = False

            If (Not m_UPS Is Nothing And IsReference(m_UPS)) Then
                m_UPS.Message(AgEUtLogMsgType.eUtLogMsgAlarm, m_Name & ".Init(): Exception Message( " & ex.Message & " )")
                m_UPS.Message(AgEUtLogMsgType.eUtLogMsgAlarm, m_Name & ".Init(): Exception StackTr( " & ex.StackTrace & " )")
            End If
            Debug.WriteLine("Exception Message( " & ex.Message & " )", m_Name & ".Init()")
            Debug.WriteLine("Exception StackTr( " & ex.StackTrace & " )", m_Name & ".Init()")
        End Try

        Init = m_Enabled
    End Function

    Public Function PrePropagate(ByVal Result As IAgAsHpopPluginResult) As Boolean Implements IAgAsHpopPlugin.PrePropagate

        Try
            Debug.WriteLine("--> Entered", m_Name & ".PrePropagate()")

            If (Not m_UPS Is Nothing And IsReference(m_UPS)) Then
                If (m_Enabled) Then

                    Dim WholeDays As Integer, Year As Integer, DayOfYear As Integer, Month As Integer, DayOfMonth As Integer
                    Dim Hours As Integer, Minutes As Integer, Seconds As Double, SecsIntoDay As Double

                    WholeDays = 0
                    SecsIntoDay = 0.0
                    Year = 0
                    DayOfYear = 0
                    Month = 0
                    DayOfMonth = 0
                    Hours = 0
                    Minutes = 0
                    Seconds = 0.0

                    If (Not Result Is Nothing And IsReference(Result)) Then
                        Result.RefEpochElements(AgEUtTimeScale.eUtTimeScaleUTC, WholeDays, SecsIntoDay, Year, DayOfYear,
                                                              Month, DayOfMonth, Hours, Minutes, Seconds)

                        m_SrpIsOn = Result.IsForceModelOn(AgEForceModelType.eSRPModel)
                        If (m_SrpIsOn) Then
                            m_SRPArea = Result.SRPArea
                        End If
                    End If

                    If (m_MsgStatus) Then
                        m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".PrePropagate():")
                        m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".PrePropagate(): Epoch WholeDays( " & WholeDays & " )")
                        m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".PrePropagate(): Epoch SecsIntoDay( " & SecsIntoDay & " )")
                        m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".PrePropagate(): Epoch Year( " & Year & " )")
                        m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".PrePropagate(): Epoch DayOfYear( " & DayOfYear & " )")
                        m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".PrePropagate(): Epoch Month( " & Month & " )")
                        m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".PrePropagate(): Epoch Hours( " & Hours & " )")
                        m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".PrePropagate(): Epoch Minutes( " & Minutes & " )")
                        m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".PrePropagate(): Epoch Seconds( " & Seconds & " )")
                    End If
                Else
                    If (m_MsgStatus) Then
                        m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".Prepropagate(): Disabled")
                    End If
                End If
            Else
                Throw New Exception("UtPluginSite was null")
            End If

        Catch ex As Exception
            m_Enabled = False

            If (Not m_UPS Is Nothing And IsReference(m_UPS)) Then

                m_UPS.Message(AgEUtLogMsgType.eUtLogMsgAlarm, m_Name & ".PrePropagate(): Exception Message( " & ex.Message & " )")
                m_UPS.Message(AgEUtLogMsgType.eUtLogMsgAlarm, m_Name & ".PrePropagate(): Exception StackTr( " & ex.StackTrace & " )")
            End If
            Debug.WriteLine("Exception Message( " & ex.Message & " )", m_Name & ".PrePropagate()")
            Debug.WriteLine("Exception StackTr( " & ex.StackTrace & " )", m_Name & ".PrePropagate()")
        End Try
        PrePropagate = m_Enabled
    End Function

    Public Function PreNextStep(ByVal Result As IAgAsHpopPluginResult) As Boolean Implements IAgAsHpopPlugin.PreNextStep

        Try
            If (m_PreNextCntr Mod m_PreNextMsgInterval = 0) Then
                Debug.WriteLine("--> Entered", m_Name & ".PreNextStep()")
            End If

            m_PreNextCntr = m_PreNextCntr + 1

            If (Not m_UPS Is Nothing And IsReference(m_UPS)) Then
                If (m_Enabled) Then
                    If (m_MsgStatus) Then
                        If (m_PreNextCntr Mod m_PreNextMsgInterval = 0) Then
                            m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".PreNextStep( " & m_PreNextCntr & " ):")
                        End If
                    End If
                Else
                    If (m_MsgStatus) Then
                        m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".PreNextStep(): Disabled")
                    End If
                End If
            Else
                Throw New Exception("UtPluginSite was null")
            End If

        Catch ex As Exception
            m_Enabled = False

            If (Not m_UPS Is Nothing And IsReference(m_UPS)) Then
                m_UPS.Message(AgEUtLogMsgType.eUtLogMsgAlarm, m_Name & ".PreNextStep(): Exception Message( " & ex.Message & " )")
                m_UPS.Message(AgEUtLogMsgType.eUtLogMsgAlarm, m_Name & ".PreNextStep(): Exception StackTr( " & ex.StackTrace & " )")
            End If
            Debug.WriteLine("Exception Message( " + ex.Message + " )", m_Name & ".PreNextStep()")
            Debug.WriteLine("Exception StackTr( " + ex.StackTrace + " )", m_Name & ".PreNextStep()")
        End Try

        PreNextStep = m_Enabled
    End Function

    Public Function Evaluate(ByVal ResultEval As IAgAsHpopPluginResultEval) As Boolean Implements IAgAsHpopPlugin.Evaluate

        Try
            If (m_EvalCntr Mod m_EvalMsgInterval = 0) Then
                Debug.WriteLine("--> Entered", m_Name & ".Evaluate( " & m_EvalCntr & " )")
            End If

            m_EvalCntr = m_EvalCntr + 1

            If (Not m_UPS Is Nothing And IsReference(m_UPS)) Then
                If (m_Enabled) Then
                    EvaluateSRPArea(ResultEval)

                    ResultEval.AddAcceleration(m_AccelRefFrame, m_AccelX, m_AccelY, m_AccelZ)

                    If (m_MsgStatus) Then
                        If (m_EvalCntr Mod m_EvalMsgInterval = 0) Then

                            m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".Evaluate( " & m_EvalCntr & " ):")

                        End If
                    End If
                Else
                    If (m_MsgStatus) Then
                        m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".Evaluate(): Disabled")
                    End If
                End If
            Else
                Throw New Exception("UtPluginSite was null")
            End If

        Catch ex As Exception
            m_Enabled = False

            If (Not m_UPS Is Nothing And IsReference(m_UPS)) Then
                m_UPS.Message(AgEUtLogMsgType.eUtLogMsgAlarm, m_Name & ".Evaluate(): Exception Message( " & ex.Message & " )")
                m_UPS.Message(AgEUtLogMsgType.eUtLogMsgAlarm, m_Name & ".Evaluate(): Exception StackTr( " & ex.StackTrace & " )")
            End If
            Debug.WriteLine("Exception Message( " & ex.Message + " )", m_Name & ".Evaluate()")
            Debug.WriteLine("Exception StackTr( " & ex.StackTrace + " )", m_Name & ".Evaluate()")
        End Try

        Evaluate = m_Enabled
    End Function

    Public Sub EvaluateSRPArea(ByVal ResultEval As IAgAsHpopPluginResultEval)

        If (Not m_SrpIsOn) Then
            Return
        End If
        ' may not have gotten a vector to do the SRP Area computation
        If (m_CCV Is Nothing) Then
            Return
        End If

        Try
            If (m_EvalCntr Mod m_EvalMsgInterval = 0) Then
                Debug.WriteLine("--> Entered", m_Name & ".EvaluateSRPArea( " & m_EvalCntr & " )")
            End If

            ' Local Variables

            Dim Result As Boolean, VecPosDotProd As Double, VecMag As Double, PosMag As Double
            Dim Theta As Double
            Dim VecX As Double, VecY As Double, VecZ As Double
            Dim PosX As Double, PosY As Double, PosZ As Double
            Dim VelX As Double, VelY As Double, VelZ As Double

            Result = False
            VecPosDotProd = 0.0
            VecMag = 0.0
            PosMag = 0.0
            Theta = 0.0
            VecX = 0.0
            VecY = 0.0
            VecZ = 0.0
            VelX = 0.0
            VelY = 0.0
            VelZ = 0.0
            PosX = 0.0
            PosY = 0.0
            PosZ = 0.0

            '//========================================
            '// 1. Get the "User Choosen" Vector XYZ
            '// 2. Get the Position Vector XYZ in 
            '//    Central Body Inertial Frame ( CBI )
            '//========================================
            If (Not ResultEval Is Nothing And IsReference(ResultEval)) Then

                ResultEval.PosVel(AgEUtFrame.eUtFrameInertial, PosX, PosY, PosZ, VelX, VelY, VelZ)
                Result = m_CCV.CurrentValue(ResultEval, VecX, VecY, VecZ)

                If (Result) Then
                    '//===============================================================
                    '// Calculate the angle (Theta in radians) between the two vectors
                    '// 1. Calculate the Dot Product
                    '// 2. Calculate the 
                    '//================================================================

                    VecPosDotProd = ((VecX * PosX) + (VecY * PosY) + (VecZ * PosZ))
                    VecMag = Math.Sqrt((VecX * VecX) + (VecY * VecY) + (VecZ * VecZ))
                    PosMag = Math.Sqrt((Math.Pow(PosX, 2) + Math.Pow(PosY, 2) + Math.Pow(PosZ, 2)))
                    Theta = Math.Acos(VecPosDotProd / (VecMag * PosMag))

                    '//===============================================
                    '// Calculate the new SRP area based on the Theta
                    '//===============================================

                    ' SRP must be on else this throws an exception
                    ResultEval.SRPArea = ((m_SRPArea / 4.0) * (3 - Math.Sin(Theta)))

                    If (Not m_UPS Is Nothing And IsReference(m_UPS) And m_MsgStatus) Then

                        If (m_EvalCntr Mod m_EvalMsgInterval = 0) Then

                            Dim ThetaDeg As Double
                            ThetaDeg = Theta * 57.295779513082323

                            m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".EvaluateSRPArea( " & m_EvalCntr & " ): VecX( " & VecX & " ), VecY( " & VecY & " ), VecZ( " & VecZ & " ) meters/sec")
                            m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".EvaluateSRPArea( " & m_EvalCntr & " ): PosX(" & PosX & " ), PosY( " & PosY & " ), PosZ( " & PosZ & " ) meters")
                            m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".EvaluateSRPArea( " & m_EvalCntr & " ): VelX(" & VelX & " ), VelY( " & VelY & " ), VelZ(" & VelZ & " ) meters/sec")
                            m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".EvaluateSRPArea( " & m_EvalCntr & " ): SRPArea(" & ResultEval.SRPArea & " m^2), Theta( " & ThetaDeg & " deg)")
                        End If
                    End If
                Else
                    If (Not m_UPS Is Nothing And IsReference(m_UPS) And m_MsgStatus) Then
                        If (m_EvalCntr Mod m_EvalMsgInterval = 0) Then
                            m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".EvaluateSRPArea( " & m_EvalCntr & " ): Result( " & Result & " )")
                        End If
                    End If
                End If
            Else
                If (Not m_UPS Is Nothing And IsReference(m_UPS) And m_MsgStatus) Then
                    m_UPS.Message(AgEUtLogMsgType.eUtLogMsgWarning, m_Name & ".Crdn Configured Vector or Result Eval was null")
                End If
            End If
        Catch ex As Exception
            m_Enabled = False
            If (Not m_UPS Is Nothing And IsReference(m_UPS)) Then
                m_UPS.Message(AgEUtLogMsgType.eUtLogMsgAlarm, m_Name & ".EvaluateSRPArea(): Exception Message( " & ex.Message & " )")
                m_UPS.Message(AgEUtLogMsgType.eUtLogMsgAlarm, m_Name & ".EvaluateSRPArea(): Exception StackTr( " & ex.StackTrace & " )")
            End If
            Debug.WriteLine("Exception Message( " & ex.Message + " )", m_Name & ".EvaluateSRPArea()")
            Debug.WriteLine("Exception StackTr( " & ex.StackTrace + " )", m_Name & ".EvaluateSRPArea()")
        End Try

    End Sub

    Public Function PostEvaluate(ByVal ResultEval As IAgAsHpopPluginResultPostEval) As Boolean Implements IAgAsHpopPlugin.PostEvaluate

        Try
            If (m_PostEvalCntr Mod m_PostEvalMsgInterval = 0) Then
                Debug.WriteLine("--> Entered", m_Name & ".PostEvaluate( " & m_PostEvalCntr & " )")
            End If

            m_PostEvalCntr = m_PostEvalCntr + 1

            If (Not m_UPS Is Nothing And IsReference(m_UPS)) Then
                If (m_Enabled) Then
                    If (m_MsgStatus) Then
                        If (m_PostEvalCntr Mod m_PostEvalMsgInterval = 0) Then
                            Dim reportFrame As AgEUtFrame, frameName As String

                            reportFrame = AgEUtFrame.eUtFrameNTC
                            frameName = "NTC"

                            Dim accelType As AgEAccelType, srpX As Double, srpY As Double, srpZ As Double
                            Dim AltInKm As Double

                            accelType = AgEAccelType.eSRPAccel
                            srpX = 0.0
                            srpY = 0.0
                            srpZ = 0.0
                            AltInKm = ResultEval.Altitude * 0.001

                            m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, _
                                m_Name & ".PostEvaluate( " & m_PostEvalCntr & " ): SRPArea (" & _
                                ResultEval.SRPArea & " m^2), Altitude (" & AltInKm & " km)")

                            ResultEval.GetAcceleration(accelType, reportFrame, srpX, srpY, srpZ)

                            m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, _
                              m_Name & ".PostEvaluate( " & m_PostEvalCntr & " ): SRPAccel (" & _
                              frameName & ") is (" & srpX & ", " & srpY & ", " & srpZ & ") meters/secs^2")

                            '// report out the added acceleration in NTC components
                            Dim thrustX As Double, thrustY As Double, thrustZ As Double

                            thrustX = 0.0
                            thrustY = 0.0
                            thrustZ = 0.0
                            accelType = AgEAccelType.eAddedAccel

                            ResultEval.GetAcceleration(accelType, reportFrame, thrustX, thrustY, thrustZ)

                            m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, _
                                m_Name & ".PostEvaluate( " & m_PostEvalCntr & " ): ThrustAccel (" & _
                                frameName & ") is (" & thrustX & ", " & thrustY & ", " & thrustZ & ") meters/secs^2")
                        End If
                    End If
                Else
                    If (m_MsgStatus) Then
                        m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".PostEvaluate(): Disabled")
                    End If
                End If
            Else
                Throw New Exception("UtPluginSite was null")
            End If

        Catch ex As Exception
            m_Enabled = False

            If (Not m_UPS Is Nothing And IsReference(m_UPS)) Then
                m_UPS.Message(AgEUtLogMsgType.eUtLogMsgAlarm, m_Name & ".PostEvaluate(): Exception Message( " & ex.Message & " )")
                m_UPS.Message(AgEUtLogMsgType.eUtLogMsgAlarm, m_Name & ".PostEvaluate(): Exception StackTr( " & ex.StackTrace & " )")
            End If
            Debug.WriteLine("Exception Message( " & ex.Message & " )", m_Name & ".PostEvaluate()")
            Debug.WriteLine("Exception StackTr( " & ex.StackTrace & " )", m_Name & ".PostEvaluate()")
        End Try

        PostEvaluate = m_Enabled
    End Function

    Public Function PostPropagate(ByVal Result As IAgAsHpopPluginResult) As Boolean Implements IAgAsHpopPlugin.PostPropagate

        Try
            Debug.WriteLine("--> Entered", m_Name & ".PostPropagate()")

            If (Not m_UPS Is Nothing And IsReference(m_UPS)) Then
                If (m_Enabled) Then
                    If (m_MsgStatus) Then
                        m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".PostPropagate():")
                    End If
                Else
                    If (m_MsgStatus) Then
                        m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".PostPropagate(): Disabled")
                    End If
                End If
            Else
                Throw New Exception("UtPluginSite was null")
            End If

        Catch ex As Exception
            m_Enabled = False
            If (Not m_UPS Is Nothing And IsReference(m_UPS)) Then
                m_UPS.Message(AgEUtLogMsgType.eUtLogMsgAlarm, m_Name & ".PostPropagate(): Exception Message( " & ex.Message & " )")
                m_UPS.Message(AgEUtLogMsgType.eUtLogMsgAlarm, m_Name & ".PostPropagate(): Exception StackTr( " & ex.StackTrace & " )")
            End If
            Debug.WriteLine("Exception Message( " + ex.Message + " )", m_Name & ".PostPropagate()")
            Debug.WriteLine("Exception StackTr( " + ex.StackTrace + " )", m_Name & ".PostPropagate()")
        End Try

        PostPropagate = m_Enabled

    End Function

    Public Sub Free() Implements IAgAsHpopPlugin.Free

        Try
            Debug.WriteLine("--> Entered", m_Name & ".Free()")

            If (Not m_UPS Is Nothing And IsReference(m_UPS)) Then

                If (m_MsgStatus) Then
                    m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".Free():")
                    m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".Free(): PreNextCntr( " & m_PreNextCntr & " )")
                    m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".Free(): EvalCntr( " & m_EvalCntr & " )")
                    m_UPS.Message(AgEUtLogMsgType.eUtLogMsgDebug, m_Name & ".Free(): EvalCntr( " & m_PostEvalCntr & " )")
                End If

                Marshal.ReleaseComObject(m_UPS)
            End If
            m_UPS = Nothing

            If (Not m_SPS Is Nothing And IsReference(m_SPS)) Then
                Marshal.ReleaseComObject(m_SPS)
            End If
            m_SPS = Nothing

            If (Not m_CPP Is Nothing And IsReference(m_CPP)) Then
                Marshal.ReleaseComObject(m_CPP)
            End If
            m_CPP = Nothing

            If (Not m_CCV Is Nothing And IsReference(m_CCV)) Then
                Marshal.ReleaseComObject(m_CCV)
            End If
            m_CCV = Nothing
        Catch ex As Exception
            If (Not m_UPS Is Nothing And IsReference(m_UPS)) Then
                m_UPS.Message(AgEUtLogMsgType.eUtLogMsgAlarm, m_Name & ".Free(): Exception Message( " & ex.Message & " )")
                m_UPS.Message(AgEUtLogMsgType.eUtLogMsgAlarm, m_Name & ".Free(): Exception StackTr( " & ex.StackTrace & " )")
            End If
            Debug.WriteLine("Exception Message( " & ex.Message + " )", m_Name & ".Free()")
            Debug.WriteLine("Exception StackTr( " & ex.StackTrace + " )", m_Name & ".Free()")
        End Try

    End Sub

#End Region

#Region "IAgUtPluginConfig Interface Implementation"

    Public Function GetPluginConfig(ByVal aab As AgAttrBuilder) As Object Implements IAgUtPluginConfig.GetPluginConfig

        Try
            Dim bUseArray As Boolean = True
            If (IsReference(m_Scope)) Then
                m_Scope = aab.NewScope()

                '//===========================
                '// General Plugin attributes
                '//===========================
                aab.AddStringDispatchProperty(m_Scope, "PluginName", "Human readable plugin name or alias", "MyName", CInt(AgEAttrAddFlags.eAddFlagNone))
                aab.AddBoolDispatchProperty(m_Scope, "PluginEnabled", "If the plugin is enabled or has experience an error", "Enabled", CInt(AgEAttrAddFlags.eAddFlagNone))
                aab.AddStringDispatchProperty(m_Scope, "VectorName", "Vector Name that affects the srp area", "VectorName", CInt(AgEAttrAddFlags.eAddFlagNone))

                '//===========================
                '// Propagation related
                '//===========================

                If bUseArray Then
                    aab.AddChoicesDispatchProperty(m_Scope, "AccelRefFrame", "Acceleration Reference Frame", "AccelRefFrame", AccelRefFrameChoices)
                Else
                    aab.AddChoicesFuncDispatchProperty(m_Scope, "AccelRefFrame", "Acceleration Reference Frame", "AccelRefFrame", "AccelRefFrameChoices")
                End If

                aab.AddDoubleDispatchProperty(m_Scope, "AccelX", "Acceleration in the X direction", "AccelX", CInt(AgEAttrAddFlags.eAddFlagNone))
                aab.AddDoubleDispatchProperty(m_Scope, "AccelY", "Acceleration in the Y direction", "AccelY", CInt(AgEAttrAddFlags.eAddFlagNone))
                aab.AddDoubleDispatchProperty(m_Scope, "AccelZ", "Acceleration in the Z direction", "AccelZ", CInt(AgEAttrAddFlags.eAddFlagNone))

                '//===========================
                '// Messaging related attributes
                '//===========================
                aab.AddBoolDispatchProperty(m_Scope, "UsePropagationMessages", "Send messages to the message window during propagation", "MsgStatus", CInt(AgEAttrAddFlags.eAddFlagNone))
                aab.AddIntDispatchProperty(m_Scope, "EvaluateMessageInterval", "The interval at which to send messages from the Evaluate method during propagation", "EvalMsgInterval", CInt(AgEAttrAddFlags.eAddFlagNone))
                aab.AddIntDispatchProperty(m_Scope, "PostEvaluateMessageInterval", "The interval at which to send messages from the PostEvaluate method during propagation", "PostEvalMsgInterval", CInt(AgEAttrAddFlags.eAddFlagNone))
                aab.AddIntDispatchProperty(m_Scope, "PreNextStepMessageInterval", "The interval at which to send messages from the PreNextStep method during propagation", "PreNextMsgInterval", CInt(AgEAttrAddFlags.eAddFlagNone))
            End If

            Dim config As String

            config = aab.ToString(Me, m_Scope)
        Catch
        End Try

        GetPluginConfig = m_Scope
    End Function

    Public Sub VerifyPluginConfig(ByVal apcvr As AgUtPluginConfigVerifyResult) Implements IAgUtPluginConfig.VerifyPluginConfig

        Dim result As Boolean
        Dim message As String

        result = True
        message = "Ok"

        Try

            '//====================================================
            '// NOTE:
            '// The following reasonable acceleration limits are 
            '// added only to show a sample of code that checks
            '// for a range of attribute values that you impose 
            '// on a user using the plugin.
            '//====================================================

            If (Not (m_AccelX <= 10 And m_AccelX >= -10)) Then
                Throw New Exception("AccelX was not within the range of -10 to +10 meters per second squared")
            ElseIf (Not (m_AccelY <= 10 And m_AccelY >= -10)) Then
                Throw New Exception("AccelY was not within the range of -10 to +10 meters per second squared")
            ElseIf (Not (m_AccelZ <= 10 And m_AccelZ >= -10)) Then
                Throw New Exception("AccelZ was not within the range of -10 to +10 meters per second squared")
            End If

        Catch ex As Exception

            result = False
            message = ex.Message
        End Try

        apcvr.Result = result
        apcvr.Message = message
    End Sub


#End Region

End Class


'//=====================================================//
'//  Copyright 2006, Analytical Graphics, Inc.          //
'//=====================================================//