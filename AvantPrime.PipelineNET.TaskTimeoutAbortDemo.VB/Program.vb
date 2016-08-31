Imports System.Diagnostics
Imports System.Threading

Module Program
	Private ReadOnly Randomizer As Random
	Private _activeTasks As Integer

	Sub New()
		Randomizer = New Random(CInt(DateTime.Now.Ticks))
	End Sub

	Sub Main(args As String())
		Dim sw = New Stopwatch()

		' Create a pipeline scheduler supporting 6 concurrent threads
		' Also support FCFS scheduling
		Dim scheduler = New PipelineScheduler(5, abortLongRunningTasks:=True, abortTaskTimeoutInterval:=500, onException:= AddressOf TaskMonitorExceptionLog)
		Const taskCount As Integer = 15

		' Use IPipelineTask interface to define work.
		' This allows extensive custom configuration for your task.

		Console.WriteLine("Running {0} tasks using default settings including First Come First Serve scheduling." & vbLf, taskCount)
		Dim elapsedMs As Long = 0
		Dim elapsedTicks As Long = 0

		scheduler.Start()

		For i As Integer = 0 To taskCount - 1
			sw.Start()
			scheduler.Push(New CustomTask() With { _
				.ThreadAbortTimeout = TimeSpan.FromMilliseconds(1000) _
			})
			sw.[Stop]()

			Console.WriteLine("{0}ms, {1} ticks", sw.ElapsedMilliseconds - elapsedMs, sw.ElapsedTicks - elapsedTicks)
			elapsedMs = sw.ElapsedMilliseconds
			elapsedTicks = sw.ElapsedTicks
		Next

		Console.WriteLine("Application/Process (main thread) finished. Press any key to exit..." & vbLf)
		Console.ReadKey()

		scheduler.Dispose()
	End Sub

	Private Sub TaskMonitorExceptionLog(task As IPipelineTask, e As Exception)
		Console.ForegroundColor = ConsoleColor.Red
		Console.WriteLine("Task running on thread {0} threw and exception: {1} at {2}.", Thread.CurrentThread.ManagedThreadId, e, DateTime.Now.ToString("hh:mm:ss.fff"))
		Console.ResetColor()
	End Sub

	Private Class CustomTask
		Implements IPipelineTask

#Region "Implementation of IPipelineTask"

		Public Sub Execute() Implements IPipelineTask.Execute
			Try
				Interlocked.Increment(_activeTasks)
				Dim delay = Randomizer.[Next](2000)
				Console.WriteLine("Starting task {0}, {1} with duration of {2}ms. Thread: {3}. Priority: {4}. Active: {5}.", _id, DateTime.Now.ToString("HH:mm:ss.fff"), delay, Thread.CurrentThread.ManagedThreadId, Priority, _
					_activeTasks)
				Thread.Sleep(delay)
				Console.ForegroundColor = ConsoleColor.Yellow
				Console.WriteLine("Task {0} finished at {1} with priority {2}.", _id, DateTime.Now.ToString("hh:mm:ss.fff"), Priority)
				Console.ResetColor()
			Catch generatedExceptionName As ThreadAbortException
				Console.ForegroundColor = ConsoleColor.Red
				Console.WriteLine("Task running on thread {0} was aborted at {1}.", Thread.CurrentThread.ManagedThreadId, DateTime.Now)
				Console.ResetColor()
			Finally
				Interlocked.Decrement(_activeTasks)
			End Try
		End Sub

		''' <summary>
		''' Gets or sets the date and time the task was added to the queue.
		''' </summary>
		Public Property ArrivalTime As Date Implements IPipelineTask.ArrivalTime

		''' <summary>
		''' Gets or sets the scheduling priority of a task.
		''' </summary>
		Public Property Priority As TaskPriority Implements IPipelineTask.Priority

		''' <summary>
		''' Gets or sets the details of the task execution.
		''' </summary>
		Public Property ExecutionResult As TaskExecutionResult Implements IPipelineTask.ExecutionResult

		''' <summary>
		''' Gets or sets the original scheduling priority of a task.
		''' </summary>
		Public Property OriginalPriority As TaskPriority Implements IPipelineTask.OriginalPriority

		''' <summary>
		''' Gets or sets the last date and time the priority
		''' was boosted.
		''' </summary>
		Public Property PriorityBoostTime As Date Implements IPipelineTask.PriorityBoostTime

		''' <summary>
		''' Gets or sets the time to wait before forcefully aborting
		''' the task. Setting the value to <see cref="TimeSpan.Zero"/> 
		''' will ensure that the task is not monitored for termination.
		''' </summary>
		Public Property ThreadAbortTimeout As TimeSpan Implements IPipelineTask.ThreadAbortTimeout

		Public Property Id As Guid Implements IPipelineTask.Id

#End Region
	End Class
End Module