Imports System.Threading

Module Program
	Private ReadOnly Randomizer As Random
	Private _activeTasks As Integer

	Sub New()
		Randomizer = New Random(DateTime.Now.Millisecond)
	End Sub

	Sub Main(args As String())
		Dim scheduler = New PipelineScheduler(6, Nothing, True, 1000, False, Nothing, new PriorityScheduler(TimeSpan.FromMilliseconds(500)), 0, True, 100, TaskQueueOverflow.Append, Nothing, Nothing, Nothing)
		Const taskCount As Integer = 10

		scheduler.Start()

		' Priority scheduling
		Console.WriteLine("Running {0} tasks using default settings with priority scheduling." & vbLf, taskCount)
		For i As Integer = 0 To taskCount - 1
			scheduler.Push(New CustomTask(i))
		Next

		Console.WriteLine("Application/Process (main thread) finished. Press any key to exit..." & vbLf)
		Console.ReadKey()

		scheduler.Dispose()
	End Sub

	Private Class CustomTask
		Implements IPipelineTask
		Private ReadOnly _identifier As Integer

		Public Sub New(identifier As Integer)
			_identifier = identifier
		End Sub

#Region "Implementation of IPipelineTask"

		Public Sub Execute() Implements IPipelineTask.Execute
			Interlocked.Increment(_activeTasks)
			Dim delay = Randomizer.[Next](3000)
			Console.WriteLine("Starting task {0}, {1} with duration of {2}ms. Thread: {3}. Priority: {4}. Active: {5}.", _identifier, DateTime.Now.ToString("hh:mm:ss.fff"), delay, Thread.CurrentThread.ManagedThreadId, Priority, _
				_activeTasks)
			Thread.Sleep(delay)
			Console.ForegroundColor = ConsoleColor.Yellow
			Console.WriteLine("Task {0} finished at {1} with priority {2}.", _identifier, DateTime.Now.ToString("hh:mm:ss.fff"), Priority)
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