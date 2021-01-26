Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.IO.Pipes

Namespace ParentProcess
    Module Main
        Sub Main()
            Const CHILDPROCNAME As String = "ChildProcessVBNET.exe"

            Console.WriteLine("Started application (Process A)...")

            ' Create child process
            Dim result = New List(Of String)()

            Dim startInfo As New ProcessStartInfo With {
                .FileName = CHILDPROCNAME,
                .CreateNoWindow = True,
                .UseShellExecute = False
            }

            Dim anotherProcess As New Process With {
                .StartInfo = startInfo
            }

            ' Create 2 anonymous pipes (read and write) for duplex communications (each pipe is one-way)
            Using pipeRead = New AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable)

                Using pipeWrite = New AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable)
                    ' Pass to the other process handles to the 2 pipes
                    anotherProcess.StartInfo.Arguments = pipeRead.GetClientHandleAsString() & " " & pipeWrite.GetClientHandleAsString()
                    anotherProcess.Start()
                    Console.WriteLine("Started other process (Process B)...")
                    Console.WriteLine()
                    pipeRead.DisposeLocalCopyOfClientHandle()
                    pipeWrite.DisposeLocalCopyOfClientHandle()

                    Try

                        Using sw = New StreamWriter(pipeWrite)
                            ' Send a 'sync message' and wait for the other process to receive it
                            sw.Write("SYNC")
                            pipeWrite.WaitForPipeDrain()
                            Console.WriteLine("Sending message to Process B...")

                            ' Send message to the other process
                            sw.Write("Hello from Process A!")
                            sw.Write("END")
                        End Using


                        ' Get message from the other process
                        Using sr = New StreamReader(pipeRead)
                            Dim temp As String


                            ' Wait for 'sync message' from the other process
                            Do
                                temp = sr.ReadLine()
                            Loop While Equals(temp, Nothing) OrElse Not temp.StartsWith("SYNC")


                            ' Read until 'end message' from the other process
                            While Not IsNothing(temp) AndAlso Not temp.StartsWith("END")
                                result.Add(temp)
                                temp = sr.ReadLine()
                            End While
                        End Using

                    Catch ex As Exception
                        'TODO Exception handling/logging
                        Throw
                    Finally
                        anotherProcess.WaitForExit()
                        anotherProcess.Close()
                    End Try

                    If result.Count > 0 Then Console.WriteLine("Received message from Process B: " & result(1))
                    Console.ReadLine()
                End Using
            End Using
        End Sub
    End Module

End Namespace