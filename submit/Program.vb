Imports System
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Net.Sockets.TcpClient
Imports System.Text
Module Program
    Dim build As String = "0.0.2"
    Public RemoteHost As New TcpClient
    Public IOStream As NetworkStream
    Sub Main(args As String())
        Console.WriteLine(String.Format("Submit build ({0}), 2024 ScottJ, westdalefarmer@gmail.com" & vbCrLf, build))
        If args.Count < 2 Then
            Console.WriteLine("submit host:port file-to-submit")
            End
        End If
        Console.WriteLine("SBMT00I submitting job to " & args(0))
        Console.WriteLine("SBMT01I submitting local file " & args(1))
        If Not CheckFile(args(1)) Then
            Console.WriteLine("SMBT80T Unable to open local file " & args(1))
            End
        End If
        Dim myHost As String = args(0).Split(":")(0)
        Dim myPort As Integer = args(0).Split(":")(1)
        If Not Connect(myHost, myPort) Then
            Console.WriteLine("SBMT90T Unable to connect to remote host")
            End
        Else
            Console.WriteLine("SBMT90I Connected to remote host.")
        End If
        LoadJCL(args(1))
        IOStream.Close()
        RemoteHost.Close()
        End
    End Sub

    Function CheckFile(infile As String) As Boolean
        If File.Exists(infile) Then
            Return True
        Else
            Return False
        End If
    End Function

    Function Connect(host As String, port As Integer) As Boolean
        Try
            RemoteHost.Connect(host, port)
            IOStream = RemoteHost.GetStream
            'Hercules card readers are devices of few words.  They don't talk back
            'so I'm not going to assign a buffer for any responses.
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Sub LoadJCL(infile As String)

        Dim fStream As New StreamReader(infile, True)
        Dim ThisLine As String = ""
        Dim lineBuff As Byte()
        While Not fStream.EndOfStream
            ThisLine = fStream.ReadLine()
            Console.WriteLine("SBMT99I " & ThisLine)
            ThisLine = ThisLine & vbCrLf
            lineBuff = Encoding.ASCII.GetBytes(ThisLine)
            IOStream.Write(lineBuff)
        End While
        fStream.Close()

    End Sub


End Module
