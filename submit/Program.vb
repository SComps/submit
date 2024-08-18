' SUBMIT 8/2024 SJohnson, westdalefarmer@gmail.com
' No license.  Public domain.  Do what you will.

Imports System
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Net.Sockets.TcpClient
Imports System.Text

' Version 0.0.1 Was originally targetting NET 6.0 which apparently caused some wierd issues with linux.
'               Rewrote and pushed to github, however reliably building became a problem and systems
'               that didn't have NET 6 but did have NET 8 had problems.
' Version 0.0.2 Targetting NET 8.0, so that MUST be installed on any system using this tool.
'               Cleaned up, commented code.  Added more error handling, graceful exits, and 
'               colorized output.  In general made the code at least a little more civilized.
'
' https://learn.microsoft.com/en-us/dotnet/core/install/linux?WT.mc_id=dotnet-35129-website
'

Module Program
    Dim build As String = "0.0.2"               ' build number to display
    Public RemoteHost As New TcpClient          ' Object handling connection to the hercules card reader
    Public IOStream As NetworkStream            ' I/O stream object for sending information to RemoteHost
    Sub Main(args As String())
        Console.WriteLine(String.Format("Submit build ({0}), 2024 ScottJ, westdalefarmer@gmail.com" & vbCrLf, build))
        If args.Count < 2 Then
            ' Not enough command line parameters were specified.  Display some information and command parameters.
            Console.WriteLine("SUBMIT, A command line tool used to submit jobs to a guest running under hercules.")
            Console.WriteLine("Jobs are submitted to a simulated 3505 card reader.  The guest must be configured")
            Console.WriteLine("as a sockdev in the hercules configuration file.")
            Console.WriteLine("")
            Console.WriteLine("This tool requires a host:port and the fully qualified filename to send to the remote")
            Console.WriteLine("system.  This tool does NOT validate your job's JCL, nor does it retrieve the job's")
            Console.WriteLine("output." & vbCrLf)
            Console.WriteLine("submit host:port file-to-submit")
            End
        End If
        ErrorMessage("SBMT00I", "submitting job to " & args(0))
        ErrorMessage("SBMT01I", "submitting local file " & args(1))

        ' Check if the specified file exists.

        If Not CheckFile(args(1)) Then
            ErrorMessage("SMBT80T", "Unable to open local file " & args(1))
            ErrorQuit()
        End If

        ' Separate the host from the port

        Dim myHost As String = args(0).Split(":")(0)
        Dim myPort As Integer = args(0).Split(":")(1)
        If Not Connect(myHost, myPort) Then
            ' A connection error took place.  Display a message and quit gracefully.
            ErrorMessage("SBMT90T", "Unable to connect to remote host")
            ErrorQuit()
        Else
            ErrorMessage("SBMT90I", "Connected to remote host.")
        End If
        LoadJCL(args(1))

        ' Close up the stream and remote host in preparation to quit

        IOStream.Close()
        RemoteHost.Close()
        End
    End Sub

    Function CheckFile(infile As String) As Boolean
        ' Check if the selected file exists.  It does NOT attempt to open it, only 
        ' check for existence.
        If File.Exists(infile) Then
            Return True
        Else
            Return False
        End If
    End Function

    Function Connect(host As String, port As Integer) As Boolean
        ' Connect to the remote host.  If not, return False eating the exception.
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

        Dim FStream As StreamReader             ' Stream to read the specified file
        Dim ThisLine As String = ""             ' Placeholder for individual output lines
        Dim LineBuff As Byte()                  ' Byte array for GetBytes required by IOStream.Write
        Try
            FStream = New StreamReader(infile, True)
        Catch ex As Exception
            ErrorMessage("SBMT81T", " " & ex.Message)
            ErrorQuit()
        End Try
        While Not FStream.EndOfStream
            ThisLine = FStream.ReadLine()       ' Read the next line into ThisLine
            ErrorMessage("SBMT99I", ThisLine)   ' Display ThisLine on the console
            ThisLine = ThisLine & vbCrLf        ' Add a CRLF for the network stream
            LineBuff = Encoding.ASCII.GetBytes(ThisLine)    ' Encode an array of bytes in ASCII from ThisLine
            IOStream.Write(LineBuff)            ' Write the array of bytes to IOStream (hercules sockdev)
        End While
        FStream.Close()                         ' Close the specified file.
    End Sub

    Sub ErrorMessage(MsgID As String, MsgText As String)
        ' Format and colorize status messages
        Select Case Right(MsgID, 1)
            Case "I"
                Console.ForegroundColor = ConsoleColor.Green
            Case "T"
                Console.ForegroundColor = ConsoleColor.Yellow
            Case Else
                Console.ForegroundColor = ConsoleColor.Cyan
        End Select
        Console.Write(MsgID)
        If Right(MsgID, 3) = "99I" Then
            Console.ForegroundColor = ConsoleColor.Cyan
        Else
            Console.ForegroundColor = ConsoleColor.White
        End If
        Console.WriteLine(": " & MsgText)
        Console.ResetColor()
    End Sub

    Private Sub ErrorQuit()

        ' Gracefully quit the program in case of an error, waiting for a keypress in
        ' acknowlegment of that error.

        Console.ForegroundColor = ConsoleColor.Red
        Console.WriteLine("The job was not submitted.  Press any key.")
        Console.ReadKey()
        Console.ResetColor()
        IOStream.Close()
        RemoteHost.Close()
        End
    End Sub

End Module
