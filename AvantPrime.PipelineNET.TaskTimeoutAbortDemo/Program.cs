using System;
using System.Diagnostics;
using System.Threading;

namespace AvantPrime.PipelineNET.TaskTimeoutAbortDemo
{
	class Program
	{
		private static readonly Random Randomizer;
		private static int _activeTasks;

		static Program()
		{
			Randomizer = new Random((int)DateTime.Now.Ticks);
		}

		static void Main(string[] args)
		{
			var sw = new Stopwatch();

			// Create a pipeline scheduler using FCFS scheduling
			var scheduler = new PipelineScheduler
			(
				Environment.ProcessorCount,			// Match pipeline count (thread limit) with the number of cores
				new ThreadPoolThreadScheduler(),	// Use ThreadPool threads (suitable for non-web .NET apps)
				abortLongRunningTasks: true,		// Set long running tasks to be aborted
				abortTaskTimeoutInterval: 500,		// Set task timeout monitor interval to 500 milliseconds (1/2 second)

				// Custom handler for exceptions that occur inside the task
				onException: TaskMonitorExceptionLog
			);

			long elapsedMs = 0;
			long elapsedTicks = 0;
			const int taskCount = 15;
			Console.WriteLine("Running {0} tasks using default settings including First Come First Serve scheduling.\n", taskCount);

			scheduler.Start();
			for (int i = 0; i < taskCount; i++)
			{
				sw.Start();

				// Use IPipelineTask interface to define work allowing
				// extensive custom configuration for your task.
				// Push the task into the pipeline queue for execution.
				scheduler.Push
				(
					new CustomTask(i)
					{
						// Set this task to be terminated if it takes
						// longer than 1 second to complete.
						ThreadAbortTimeout = TimeSpan.FromSeconds(1)
					}
				);

				// Push another task using a simple lambda expression
				// without having to define a custom IPipelineTask.
				// Powerful but without the custom configuration available
				// with an IPipelineTask.
				scheduler.Push(() => { /* Do some work here */ } );

				sw.Stop();

				Console.WriteLine("{0}ms, {1} ticks", sw.ElapsedMilliseconds - elapsedMs, sw.ElapsedTicks - elapsedTicks);
				elapsedMs = sw.ElapsedMilliseconds;
				elapsedTicks = sw.ElapsedTicks;
			}

			Console.WriteLine("Application/Process (main thread) finished. Press any key to exit...\n");
			Console.ReadKey();

			scheduler.Dispose();
		}

		private static void TaskMonitorExceptionLog(IPipelineTask task, Exception e)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Task running on thread {0} threw and exception: {1} at {2}.", Thread.CurrentThread.ManagedThreadId, e, DateTime.Now.ToString("hh:mm:ss.fff"));
			Console.ResetColor();
		}

		private class CustomTask : IPipelineTask
		{
			private readonly int _id;

			public CustomTask(int id)
			{
				_id = id;
			}

			#region Implementation of IPipelineTask

			public void Execute()
			{
				try
				{
					Interlocked.Increment(ref _activeTasks);
					var delay = Randomizer.Next(2000);
					Console.WriteLine("Starting task {0}, {1} with duration of {2}ms. Thread: {3}. Priority: {4}. Active: {5}.", _id,
						DateTime.Now.ToString("HH:mm:ss.fff"), delay, Thread.CurrentThread.ManagedThreadId, Priority, _activeTasks);
					Thread.Sleep(delay);
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("Task {0} finished at {1} with priority {2}.", _id, DateTime.Now.ToString("hh:mm:ss.fff"), Priority);
					Console.ResetColor();
				}
				catch (ThreadAbortException)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Task running on thread {0} was aborted at {1}.", Thread.CurrentThread.ManagedThreadId, DateTime.Now);
					Console.ResetColor();
				}
				finally
				{
					Interlocked.Decrement(ref _activeTasks);
				}
			}

			/// <summary>
			/// Gets or sets the date and time the task was added to the queue.
			/// </summary>
			public DateTime ArrivalTime { get; set; }

			/// <summary>
			/// Gets or sets the scheduling priority of a task.
			/// </summary>
			public TaskPriority Priority { get; set; }

			/// <summary>
			/// Gets or sets the details of the task execution.
			/// </summary>
			public TaskExecutionResult ExecutionResult { get; set; }

			/// <summary>
			/// Gets or sets the original scheduling priority of a task.
			/// </summary>
			public TaskPriority OriginalPriority { get; set; }

			/// <summary>
			/// Gets or sets the last date and time the priority
			/// was boosted.
			/// </summary>
			public DateTime PriorityBoostTime { get; set; }

			/// <summary>
			/// Gets or sets the time to wait before forcefully aborting
			/// the task. Setting the value to <see cref="TimeSpan.Zero"/> 
			/// will ensure that the task is not monitored for termination.
			/// </summary>
			public TimeSpan ThreadAbortTimeout { get; set; }

			public Guid Id { get; set; }

			#endregion
		}
	}
}
