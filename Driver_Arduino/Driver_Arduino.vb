﻿Imports HoMIDom
Imports HoMIDom.HoMIDom.Server
Imports HoMIDom.HoMIDom.Device
Imports Driver_Arduino.Arduino

Public Class Driver_Arduino
    Implements HoMIDom.HoMIDom.IDriver

#Region "Variable Driver"
    '!!!Attention les variables ci-dessous doivent avoir une valeur par défaut obligatoirement
    'aller sur l'adresse http://www.somacon.com/p113.php pour avoir un ID
    Dim _ID As String = "958D5812-390B-11E1-A6F7-D4CA4724019B"
    Dim _Nom As String = "Arduino"
    Dim _Enable As String = False
    Dim _Description As String = "Arduino"
    Dim _StartAuto As Boolean = False
    Dim _Protocol As String = "RS232"
    Dim _IsConnect As Boolean = False
    Dim _IP_TCP As String = "@"
    Dim _Port_TCP As String = "@"
    Dim _IP_UDP As String = "@"
    Dim _Port_UDP As String = "@"
    Dim _Com As String = "COM2"
    Dim _Refresh As Integer = 0
    Dim _Modele As String = "Arduino"
    Dim _Version As String = "1.0"
    Dim _Picture As String = ""
    Dim _Server As HoMIDom.HoMIDom.Server
    Dim _Device As HoMIDom.HoMIDom.Device
    Dim _DeviceSupport As New ArrayList
    Dim _Parametres As New ArrayList
    Dim MyTimer As New Timers.Timer
    Dim _idsrv As String
    Dim _DeviceCommandPlus As New List(Of HoMIDom.HoMIDom.Device.DeviceCommande)

    'A ajouter dans les ppt du driver
    Dim _tempsentrereponse As Integer = 1500
    Dim _ignoreadresse As Boolean = False
    Dim _lastetat As Boolean = True
#End Region

#Region "Declaration"
    Private WithEvents Arduino As Arduino
#End Region

#Region "Fonctions génériques"

    Public WriteOnly Property IdSrv As String Implements HoMIDom.HoMIDom.IDriver.IdSrv
        Set(ByVal value As String)
            _idsrv = value
        End Set
    End Property

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

    Public Property Parametres() As System.Collections.ArrayList Implements HoMIDom.HoMIDom.IDriver.Parametres
        Get
            Return _Parametres
        End Get
        Set(ByVal value As System.Collections.ArrayList)
            _Parametres = value
        End Set
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

    Public Property Modele() As String Implements HoMIDom.HoMIDom.IDriver.Modele
        Get
            Return _Modele
        End Get
        Set(ByVal value As String)
            _Modele = value
        End Set
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
        If _Enable = False Then Exit Sub

        Arduino.GetDigitalValue(Objet.Adresse1)
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

    ''' <summary>
    ''' Retourne la liste des Commandes avancées
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetCommandPlus() As List(Of DeviceCommande)
        Return _DeviceCommandPlus
    End Function

    ''' <summary>
    ''' Execute une commande avancée
    ''' </summary>
    ''' <param name="Command"></param>
    ''' <param name="Param"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ExecuteCommand(ByVal Command As String, Optional ByVal Param() As Object = Nothing) As Boolean
        Dim retour As Boolean = False

        If Command = "" Then
            Return False
            Exit Function
        End If

        Select Case UCase(Command)
            Case ""
            Case Else
        End Select

        Return retour
    End Function

    Public Sub Start() Implements HoMIDom.HoMIDom.IDriver.Start
        'cree l'objet
        Try
            Arduino = New Arduino(_Com, "9600")
            Arduino.DigitalCount = 14
            Arduino.AnalogCount = 6
            Arduino.PWMPorts = New Integer() {3, 5, 6, 9, 10, 11}

            AddHandler Arduino.DigitalDataReceived, AddressOf ArduinoDigitalData
            AddHandler Arduino.AnalogDataReceived, AddressOf ArduinoAnalogData
            AddHandler Arduino.LogMessageReceived, AddressOf WriteLog
            AddHandler Arduino.ConnectionLost, AddressOf ConnectionLost

            If Arduino.StartCommunication() = True Then
                _Server.Log(TypeLog.INFO, TypeSource.DRIVER, Me.Nom & " Start", "Carte connectée")
                _IsConnect = True
            Else
                _Server.Log(TypeLog.ERREUR, TypeSource.DRIVER, Me.Nom & " Start", "Le driver n'a pas réussit à se connecter à la carte ")
                _IsConnect = False
            End If
            
        Catch ex As Exception
            _Server.Log(TypeLog.ERREUR, TypeSource.DRIVER, Me.Nom & " Start", "Erreur lors du démarrage du driver: " & ex.ToString)
            _IsConnect = False
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
        'cree l'objet
        Try
            RemoveHandler Arduino.DigitalDataReceived, AddressOf ArduinoDigitalData
            RemoveHandler Arduino.AnalogDataReceived, AddressOf ArduinoAnalogData
            RemoveHandler Arduino.LogMessageReceived, AddressOf WriteLog
            RemoveHandler Arduino.ConnectionLost, AddressOf ConnectionLost

            Arduino = Nothing
            _Server.Log(TypeLog.INFO, TypeSource.DRIVER, Me.Nom & " Stop", "Driver arrêté")
            _IsConnect = False
        Catch ex As Exception
            _Server.Log(TypeLog.ERREUR, TypeSource.DRIVER, Me.Nom & " Stop", "Erreur lors de l'arrêt du driver: " & ex.ToString)
            _IsConnect = False
        End Try
    End Sub

    Public ReadOnly Property Version() As String Implements HoMIDom.HoMIDom.IDriver.Version
        Get
            Return _Version
        End Get
    End Property

    Public Sub Write(ByVal Objet As Object, ByVal Commande As String, Optional ByVal Parametre1 As Object = Nothing, Optional ByVal Parametre2 As Object = Nothing) Implements HoMIDom.HoMIDom.IDriver.Write
        If _Enable = False Then Exit Sub
        If _IsConnect = False Then Exit Sub
        Try
            'Select Case Objet.Type
            '    Case "APPAREIL" Or "GENERIQUEBOOLEEN" 'APPAREIL Or GENERIQUEBOOLEAN
            If Commande = "ON" Then
                Arduino.SetDigitalValue(Objet.adresse1, 1)
            End If
            If Commande = "OFF" Then
                Arduino.SetDigitalValue(Objet.adresse1, 0)
            End If
            'End Select
        Catch ex As Exception
            _Server.Log(TypeLog.ERREUR, TypeSource.DRIVER, Me.Nom & " Write", "Erreur: " & ex.ToString)
        End Try
    End Sub

    Public Sub DeleteDevice(ByVal DeviceId As String) Implements HoMIDom.HoMIDom.IDriver.DeleteDevice

    End Sub

    Public Sub NewDevice(ByVal DeviceId As String) Implements HoMIDom.HoMIDom.IDriver.NewDevice

    End Sub

    Public Sub New()
        _DeviceSupport.Add(ListeDevices.SWITCH)
        _DeviceSupport.Add(ListeDevices.GENERIQUEBOOLEEN)
        _DeviceSupport.Add(ListeDevices.CONTACT)
        _DeviceSupport.Add(ListeDevices.APPAREIL)

        'ajout des commandes avancées pour les devices
        'Ci-dessous un exemple
        'Dim x As New DeviceCommande
        'x.NameCommand = "Test"
        'x.DescriptionCommand = "Ceci est une commande avancée de test"
        'x.CountParam = 1
        '_DeviceCommandPlus.Add(x)
    End Sub
#End Region

#Region "Fonctions propres au driver"
    Delegate Sub ArduinoDigitalDataCallback(ByVal PortNr As Integer, ByVal Value As Integer)

    Private Sub WriteLog(ByVal LogText As String)
        _Server.Log(TypeLog.DEBUG, TypeSource.DRIVER, Me.Nom & " WriteLog", "Log:" & LogText)
    End Sub

    Private Sub ArduinoDigitalData(ByVal PortNr As Integer, ByVal Value As Integer)
        Select Case PortNr
            Case 2
                If Value = 0 Then
                    '            Label1.BackColor = Drawing.Color.Black
                Else
                    '           Label1.BackColor = Drawing.Color.Red
                End If
            Case 3
                If Value = 0 Then
                    '          Label2.BackColor = Drawing.Color.Black
                Else
                    '         Label2.BackColor = Drawing.Color.Red
                End If
        End Select
    End Sub

    Delegate Sub ArduinoAnalogDataCallback(ByVal PortNr As Integer, ByVal Value As Integer)

    Private Sub ArduinoAnalogData(ByVal PortNr As Integer, ByVal Value As Integer)
        Select Case PortNr
            Case 0
                '                Label3.Text = Value.ToString
        End Select

        'process analog data produced by analogtimer1
        If PortNr = 1 Then
            'convert value to distance...needs testing

            'http://www.acroname.com/robotics/info/articles/irlinear/irlinear.html

            Dim Distance As Integer 'distance in cm
            If Value > 50 Then
                Distance = (6787 / (Value - 3)) - 4
                If Distance > 80 Then Distance = 80
            Else
                Distance = 0
            End If

            '           AnalogValue1.Text = Distance.ToString + " cm"
            '          AnalogSlider1.Value = Distance

        End If
    End Sub

    Private Sub Watchdog()
        'WriteLog("Watchdog")
    End Sub

    Private Sub ConnectionLost()
        'WriteLog("Connection is lost")
    End Sub

    ''' <summary>Traite les paquets reçus</summary>
    ''' <remarks></remarks>
    Private Sub traitement(ByVal valeur As Boolean, ByVal adresse As String)
        Try
            'Recherche si un device affecté
            Dim listedevices As New ArrayList

            listedevices = _Server.ReturnDeviceByAdresse1TypeDriver(_idsrv, adresse, "CONTACT", Me._ID, True)
            'un device trouvé on maj la value
            If (listedevices.Count = 1) Then
                'correction valeur pour correspondre au type de value
                If TypeOf listedevices.Item(0).Value Is Integer Then
                    If valeur = True Then
                        valeur = 100
                    ElseIf valeur = False Then
                        valeur = 0
                    End If
                End If

                listedevices.Item(0).Value = valeur

            ElseIf (listedevices.Count > 1) Then
                _Server.Log(TypeLog.ERREUR, TypeSource.DRIVER, Me.Nom & " Process", "Plusieurs devices correspondent à : " & adresse & ":" & valeur)
            Else
                _Server.Log(TypeLog.ERREUR, TypeSource.DRIVER, Me.Nom & " Process", "Device non trouvé : " & adresse & ":" & valeur)

            End If
        Catch ex As Exception
            _Server.Log(TypeLog.ERREUR, TypeSource.DRIVER, Me.Nom & " traitement", "Exception : " & ex.Message & " --> " & adresse & " : " & valeur)
        End Try
    End Sub

#End Region


End Class