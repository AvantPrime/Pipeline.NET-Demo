using System;
using System.Threading;

namespace AvantPrime.PipelineNET.PrioritySchedulerDemo
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
			var scheduler = new PipelineScheduler(6, scheduler: new PriorityScheduler(TimeSpan.FromMilliseconds(500)));
			const int taskCount = 10;

			scheduler.Start();

			// Priority scheduling
			Console.WriteLine("Running {0} tasks using default settings with priority scheduling.\n", taskCount);
			for (int i = 0; i < taskCount; i++)
			{
				scheduler.Push(new CustomTask(i));
			}

			Console.WriteLine("Application/Process (main thread) finished. Press any key to exit...\n");
			Console.ReadKey();

			scheduler.Dispose();
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
				Interlocked.Increment(ref _activeTasks);
				var delay = Randomizer.Next(3000);
				Console.WriteLine("Starting task {0}, {1} with duration of {2}ms. Thread: {3}. Priority: {4}. Active: {5}.", _id, DateTime.Now.ToString("hh:mm:ss.fff"), delay, Thread.CurrentThread.ManagedThreadId, Priority, _activeTasks);
				Thread.Sleep(delay);
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("Task {0} finished at {1} with priority {2}.", _id, DateTime.Now.ToString("hh:mm:ss.fff"), Priority);
				Console.ResetColor();
				Interlocked.Decrement(ref _activeTasks);
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

			/// <summary>Gets or sets the unique identifier of the task.</summary>
			/// <remarks>
			/// The <see cref="T:AvantPrime.PipelineNET.PipelineScheduler" /> assigns a unique
			/// value if it has not already been set by client.
			/// </remarks>
			public Guid Id { get; set; }

			#endregion
		}
	}
}
