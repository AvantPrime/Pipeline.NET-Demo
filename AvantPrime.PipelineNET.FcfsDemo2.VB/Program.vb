Imports System.Threading

Module Program
	Private ReadOnly Randomizer As Random
	Private _activeTasks As Integer

	Sub New()
		Randomizer = New Random(CType(DateTime.Now.Ticks Mod Int32.MaxValue, Integer))
	End Sub

	Sub Main(args As String())
		Dim scheduler = New PipelineScheduler(6, True, True, 1000, False, 0, Nothing, Nothing, Nothing)
		Const taskCount As Integer = 10

		scheduler.Start()

		' Use delegate / lambda expression task
		Console.WriteLine("Running {0} tasks using lambda task & default settings including First Come First Serve scheduling." & vbLf, taskCount)
		For i As Integer = 0 To taskCount - 1
			Dim taskId As Integer = i
			scheduler.Push(Sub()
							   Interlocked.Increment(_activeTasks)
							   Dim delay = Randomizer.[Next](3000)
							   Console.WriteLine("Starting task {0}, {1} with duration of {2}ms. Thread: {3}. Active: {4}.", taskId, DateTime.Now.ToString("hh:mm:ss.fff"), delay, Thread.CurrentThread.ManagedThreadId, _activeTasks)
							   Thread.Sleep(delay)
							   Console.ForegroundColor = ConsoleColor.Yellow
							   Console.WriteLine("Task {0} finished at {1}.", taskId, DateTime.Now.ToString("hh:mm:ss.fff"))
							   Console.ResetColor()
							   Interlocked.Decrement(_activeTasks)
						   End Sub)
		Next

		Console.WriteLine("Application/Process (main thread) finished. Press any key to exit..." & vbLf)
		Console.ReadKey()

		scheduler.Dispose()
	End Sub
End Module