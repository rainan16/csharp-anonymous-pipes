Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.IO.Pipes

Namespace ChildProcessVBNET
    Module Main
        Sub Main()
            Dim args() As String = Environment.GetCommandLineArgs()
            If args Is Nothing OrElse args.Length < 3 Then Return
            Dim pipeWriteHandle As String = args(1)
            Dim pipeReadHandle As String = args(2)

            Using pipeRead = New AnonymousPipeClientStream(PipeDirection.[In], pipeReadHandle)

                Using pipeWrite = New AnonymousPipeClientStream(PipeDirection.Out, pipeWriteHandle)

                    Try
                        Dim values = New List(Of String)()

                        Using sr = New StreamReader(pipeRead)
                            Dim temp As String

                            Do
                                temp = sr.ReadLine()
                            Loop While temp Is Nothing OrElse Not temp.StartsWith("SYNC")

                            While temp IsNot Nothing AndAlso Not temp.StartsWith("END")
                                values.Add(temp)
                                temp = sr.ReadLine()
                            End While
                        End Using

                        Using sw = New StreamWriter(pipeWrite)
                            sw.AutoFlush = True
                            sw.WriteLine("SYNC")
                            pipeWrite.WaitForPipeDrain()
                            sw.WriteLine("Hello from Process B!")
                            sw.WriteLine("END")
                        End Using

                    Catch ex As Exception
                        Throw
                    End Try
                End Using
            End Using
        End Sub

    End Module
End Namespace