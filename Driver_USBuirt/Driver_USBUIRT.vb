﻿Imports HoMIDom
Imports HoMIDom.HoMIDom.Device
Imports HoMIDom.HoMIDom.Server
Imports UsbUirt

' Driver USBUIRT
' Nécessite la dll usbuirtmanagedwrapper
' Auteur : Seb
' Date : 10/02/2011

''' <summary>
''' Class USBUIRT, permet d'apprendre des codes IR et de les restituer par la suite 
''' </summary>
''' <remarks></remarks>
<Serializable()> Public Class Driver_USBUIRT
    Implements HoMIDom.HoMIDom.IDriver

#Region "Variable Driver"
    '!!!Attention les variables ci-dessous doivent avoir une valeur par défaut obligatoirement
    'aller sur l'adresse http://www.somacon.com/p113.php pour avoir un ID
    Dim _ID As String = "74FD4E7C-34ED-11E0-8AC4-70CEDED72085"
    Dim _Nom As String = "USBuirt"
    Dim _Enable As String = False
    Dim _Description As String = "Emetteur/Récepteur Infrarouge sur port USB"
    Dim _StartAuto As Boolean = False
    Dim _Protocol As String = "IR"
    Dim _IsConnect As Boolean = False
    Dim _IP_TCP As String = ""
    Dim _Port_TCP As String = ""
    Dim _IP_UDP As String = ""
    Dim _Port_UDP As String = ""
    Dim _Com As String = ""
    Dim _Refresh As Integer = 0
    Dim _Modele As String = "USBuirt"
    Dim _Version As String = "1.0"
    Dim _Picture As String = "usbuirt.png"
    Dim _Server As HoMIDom.HoMIDom.Server
    Dim _Device As HoMIDom.HoMIDom.Device
    Dim _DeviceSupport As New ArrayList
    Dim MyTimer As New Timers.Timer

    'A ajouter dans les ppt du driver
    Dim _tempsentrereponse As Integer = 1500
    Dim _ignoreadresse As Boolean = False
    Dim _lastetat As Boolean = True
#End Region

#Region "Déclaration"
    'variables propres à ce driver
    <NonSerialized()> Dim mc As Controller 'var pour l'usb uirt
    <NonSerialized()> Private learn_code_modifier As LearnCodeModifier = LearnCodeModifier.ForceStruct
    <NonSerialized()> Private code_format As CodeFormat = CodeFormat.Uuirt
    <NonSerialized()> Dim args As LearnCompletedEventArgs = Nothing     'arguments récup lors de l'apprentissage

    Private last_received_code As String        'dernier code recu

    Public Structure ircodeinfo
        Public code_to_send As String
        Public code_to_receive As String
    End Structure

#End Region

#Region "Fonctions génériques"
    Public Property COM() As String Implements HoMIDom.HoMIDom.IDriver.COM
        Get
            Return _Com
        End Get
        Set(ByVal value As String)
            _Com = value
        End Set
    End Property

    Public ReadOnly Property Description() As String Implements HoMIDom.HoMIDom.IDriver.Description
        Get
            Return _Description
        End Get
    End Property

    Public ReadOnly Property DeviceSupport() As System.Collections.ArrayList Implements HoMIDom.HoMIDom.IDriver.DeviceSupport
        Get
            Return _DeviceSupport
        End Get
    End Property

    Public Event DriverEvent(ByVal DriveName As String, ByVal TypeEvent As String, ByVal Parametre As Object) Implements HoMIDom.HoMIDom.IDriver.DriverEvent

    Public Property Enable() As Boolean Implements HoMIDom.HoMIDom.IDriver.Enable
        Get
            Return _Enable
        End Get
        Set(ByVal value As Boolean)
            _Enable = value
        End Set
    End Property

    Public ReadOnly Property ID() As String Implements HoMIDom.HoMIDom.IDriver.ID
        Get
            Return _ID
        End Get
    End Property

    Public Property IP_TCP() As String Implements HoMIDom.HoMIDom.IDriver.IP_TCP
        Get
            Return _IP_TCP
        End Get
        Set(ByVal value As String)
            _IP_TCP = value
        End Set
    End Property

    Public Property IP_UDP() As String Implements HoMIDom.HoMIDom.IDriver.IP_UDP
        Get
            Return _IP_UDP
        End Get
        Set(ByVal value As String)
            _IP_UDP = value
        End Set
    End Property

    Public ReadOnly Property IsConnect() As Boolean Implements HoMIDom.HoMIDom.IDriver.IsConnect
        Get
            Return _IsConnect
        End Get
    End Property

    Public ReadOnly Property Modele() As String Implements HoMIDom.HoMIDom.IDriver.Modele
        Get
            Return _Modele
        End Get
    End Property

    Public ReadOnly Property Nom() As String Implements HoMIDom.HoMIDom.IDriver.Nom
        Get
            Return _Nom
        End Get
    End Property

    Public Property Picture() As String Implements HoMIDom.HoMIDom.IDriver.Picture
        Get
            Return _Picture
        End Get
        Set(ByVal value As String)
            _Picture = value
        End Set
    End Property

    Public Property Port_TCP() As Object Implements HoMIDom.HoMIDom.IDriver.Port_TCP
        Get
            Return _Port_TCP
        End Get
        Set(ByVal value As Object)
            _Port_TCP = value
        End Set
    End Property

    Public Property Port_UDP() As String Implements HoMIDom.HoMIDom.IDriver.Port_UDP
        Get
            Return _Port_UDP
        End Get
        Set(ByVal value As String)
            _Port_UDP = value
        End Set
    End Property

    Public ReadOnly Property Protocol() As String Implements HoMIDom.HoMIDom.IDriver.Protocol
        Get
            Return _Protocol
        End Get
    End Property

    Public Sub Read(ByVal Objet As Object) Implements HoMIDom.HoMIDom.IDriver.Read

    End Sub

    Public Property Refresh() As Integer Implements HoMIDom.HoMIDom.IDriver.Refresh
        Get
            Return _Refresh
        End Get
        Set(ByVal value As Integer)
            _Refresh = value
        End Set
    End Property

    Public Sub Restart() Implements HoMIDom.HoMIDom.IDriver.Restart
        [Stop]()
        Start()
    End Sub

    Public Property Server() As HoMIDom.HoMIDom.Server Implements HoMIDom.HoMIDom.IDriver.Server
        Get
            Return _Server
        End Get
        Set(ByVal value As HoMIDom.HoMIDom.Server)
            _Server = value
        End Set
    End Property

    Public Sub Start() Implements HoMIDom.HoMIDom.IDriver.Start
        Try
            Me.mc = New Controller
            'capte les events
            AddHandler mc.Received, AddressOf handler_mc_received
            _IsConnect = True
            _Server.Log(TypeLog.INFO, TypeSource.DRIVER, "USBUIRT", "Driver démarré")
        Catch ex As Exception
            _IsConnect = False
            _Server.Log(TypeLog.ERREUR, TypeSource.DRIVER, "USBUIRT", "Driver erreur lors du démarrage: " & ex.Message)
        End Try
    End Sub

    Public Property StartAuto() As Boolean Implements HoMIDom.HoMIDom.IDriver.StartAuto
        Get
            Return _StartAuto
        End Get
        Set(ByVal value As Boolean)
            _StartAuto = value
        End Set
    End Property

    Public Sub [Stop]() Implements HoMIDom.HoMIDom.IDriver.Stop
        Me.mc = Nothing
        _IsConnect = False
        _Server.Log(TypeLog.INFO, TypeSource.DRIVER, "USBUIRT", "Driver arrêté")
    End Sub

    Public ReadOnly Property Version() As String Implements HoMIDom.HoMIDom.IDriver.Version
        Get
            Return _Version
        End Get
    End Property

    Public Sub Write(ByVal Objet As Object, ByVal Commande As String, Optional ByVal Parametre1 As Object = Nothing, Optional ByVal Parametre2 As Object = Nothing) Implements HoMIDom.HoMIDom.IDriver.Write
        If Objet.type = "MULTIMEDIA" Then
            If Command() = "SendCodeIR" Then
                SendCodeIR(Parametre1, Parametre2)
            Else
                _Server.Log(TypeLog.DEBUG, TypeSource.DRIVER, "USBUIRT", "La commande " & Commande & " est inconnue pour ce driver")
            End If
        Else
            _Server.Log(TypeLog.ERREUR, TypeSource.DRIVER, "USBUIRT", "Impossible d'envoyer un code IR pour un type de device autre que MULTIMEDIA")
        End If
    End Sub

    Public Sub New()
        _DeviceSupport.Add(ListeDevices.MULTIMEDIA)
    End Sub
#End Region

#Region "Fonctions propre au driver"
    ''' <summary>
    ''' Apprendre un code IR
    ''' </summary>
    ''' <returns>Retourne le code IR</returns>
    ''' <remarks></remarks>
    Public Function LearnCodeIR() As String
        If _IsConnect = False Then
            Return "Impossible d'apprendre le code IR le driver n'est pas connecté"
        Else
            Dim x As ircodeinfo
            x = wait_for_code()
            Return x.code_to_send
        End If
    End Function

    'boucle qui attend kon recoive
    <System.STAThread()> _
    Private Function wait_for_code() As ircodeinfo

        'handler
        AddHandler mc.Learning, AddressOf handler_mc_learning
        AddHandler mc.LearnCompleted, AddressOf handler_mc_learning_completed
        Me.args = Nothing

        'lance l'apprentissage
        Try
            Try
                Me.mc.LearnAsync(Me.code_format, Me.learn_code_modifier, Me.args)
                'Me.mc.Learn(Me.code_format, Me.learn_code_modifier, TimeSpan.Zero)
            Catch ex As Exception
                'MsgBox(ex.Message)
                Return Nothing
            End Try

            'attend que ce soit appris
            Do While IsNothing(Me.args)
                'Application.DoEvents()          !!!!!!!!MODIFIE A CAUSE DOEVENTS
            Loop

            'c appris !!!
            RemoveHandler mc.Learning, AddressOf handler_mc_learning
            RemoveHandler mc.LearnCompleted, AddressOf handler_mc_learning_completed

        Catch ex As Exception
            _Server.Log(TypeLog.DEBUG, TypeSource.DRIVER, "USBUIRT", "Erreur lors de l'apprentissage:" & ex.Message)
            Return Nothing
        End Try

        'retourne le code
        wait_for_code.code_to_send = Me.args.Code
        wait_for_code.code_to_receive = last_received_code
        Return wait_for_code
    End Function

    '*****************************************************************************
    ''' <summary>
    ''' Emet un code infrarouge
    ''' </summary>
    ''' <param name="ir_code"></param>
    ''' <param name="RepeatCount"></param>
    ''' <remarks></remarks>
    Public Sub SendCodeIR(ByVal ir_code As String, ByVal RepeatCount As Integer)
        Try
            mc.Transmit(ir_code, CodeFormat.Uuirt, RepeatCount, TimeSpan.Zero)
            _Server.Log(TypeLog.MESSAGE, TypeSource.DRIVER, "USBUIRT", "Code IR envoyé: " & ir_code & " repeat: " & RepeatCount)
        Catch ex As Exception
            _Server.Log(TypeLog.ERREUR, TypeSource.DRIVER, "USBUIRT", "Problème de transmission: " & ex.Message)
        End Try
    End Sub

    '*****************************************************************************
    'handler code recu
    Private Sub handler_mc_received(ByVal sender As Object, ByVal e As ReceivedEventArgs)
        _Server.Log(TypeLog.MESSAGE, TypeSource.DRIVER, "USBUIRT", "Code IR reçu: " & e.IRCode)
        Debug.WriteLine("Code recu: " & e.IRCode)
        last_received_code = e.IRCode
        RaiseEvent DriverEvent(_Nom, "CODE_RECU", e.IRCode)
    End Sub

    '*****************************************************************************
    'handler en apprentissage
    Private Sub handler_mc_learning(ByVal sender As Object, ByVal e As LearningEventArgs)
        Try
            'Debug.WriteLine("Learning: " & e.Progress & " freq=" & e.CarrierFrequency & " quality=" & e.SignalQuality)
        Catch ex As Exception

        End Try
    End Sub

    '*****************************************************************************
    'handler a appris
    Private Sub handler_mc_learning_completed(ByVal sender As Object, ByVal e As LearnCompletedEventArgs)
        args = e
        _Server.Log(TypeLog.DEBUG, TypeSource.DRIVER, "USBUIRT", "Learning completed: " & e.Code)
        RaiseEvent DriverEvent(_Nom, "LEARN_TERMINE", e.Code)
    End Sub
#End Region
End Class