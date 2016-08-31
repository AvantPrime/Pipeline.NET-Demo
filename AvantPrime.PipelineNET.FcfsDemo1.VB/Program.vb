Imports System.Threading

Module Program
	Private _randomizer As Random
	Private _activeTasks As Integer

	Sub Main()
		_randomizer = New Random(CType(DateTime.Now.Ticks Mod Int32.MaxValue, Integer))
		Dim scheduler = New PipelineScheduler(6, ThreadingMechanism.ThreadPool, True, 1000, False, 0, Nothing, 0, True, 100, TaskQueueOverflow.Append, Nothing, Nothing, Nothing)
		Const taskCount As Integer = 10

		' Use IPipelineTask interface to define work.
		' This allows extensive custom configuration for your task.

		Console.WriteLine("Running {0} tasks using default settings including First Come First Serve scheduling." & vbLf, taskCount)
		For i As Integer = 0 To taskCount - 1
			scheduler.Push(New CustomTask())
		Next

		scheduler.Start()
		Console.WriteLine("Application/Process (main thread) finished. Press any key to exit..." & vbLf)
		Console.ReadKey()

		scheduler.Dispose()
	End Sub

	Private Class CustomTask
		Implements IPipelineTask

#Region "Implementation of IPipelineTask"

		Public Sub Execute() Implements IPipelineTask.Execute
			Interlocked.Increment(_activeTasks)
			Dim delay = _randomizer.[Next](3000)
			Console.WriteLine("Starting task {0}, {1} with duration of {2}ms. Thread: {3}. Priority: {4}. Active: {5}.", _id, DateTime.Now.ToString("HH:mm:ss.fff"), delay, Thread.CurrentThread.ManagedThreadId, Priority, _
				_activeTasks)
			Thread.Sleep(delay)
			Console.ForegroundColor = ConsoleColor.Yellow
			Console.WriteLine("Task {0} finished at {1} with priority {2}.", _id, DateTime.Now.ToString("hh:mm:ss.fff"), Priority)
			Console.ResetColor()
			Interlocked.Decrement(_activeTasks)
		End Sub

		''' <summary>
		''' Gets or sets the date and time the task was added to the queue.
		''' </summary>
		Public Property ArrivalTime() As DateTime Implements IPipelineTask.ArrivalTime

		''' <summary>
		''' Gets or sets the scheduling priority of a task.
		''' </summary>
		Public Property Priority() As TaskPriority Implements IPipelineTask.Priority

		''' <summary>
		''' Gets or sets the details of the task execution.
		''' </summary>
		Public Property ExecutionResult() As TaskExecutionResult Implements IPipelineTask.ExecutionResult

		''' <summary>
		''' Gets or sets the original scheduling priority of a task.
		''' </summary>
		Public Property OriginalPriority() As TaskPriority Implements IPipelineTask.OriginalPriority

		''' <summary>
		''' Gets or sets the last date and time the priority
		''' was boosted.
		''' </summary>
		Public Property PriorityBoostTime() As DateTime Implements IPipelineTask.PriorityBoostTime

		Public Property ThreadAbortTimeout() As TimeSpan Implements IPipelineTask.ThreadAbortTimeout
		Public Property Id As Guid Implements IPipelineTask.Id

#End Region
	End Class
End Module