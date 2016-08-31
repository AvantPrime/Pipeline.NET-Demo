using System;
using System.Diagnostics;
using System.Threading;

namespace AvantPrime.PipelineNET.FcfsDemo1
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

			// Create a pipeline scheduler supporting 6 concurrent threads
			// Also support FCFS scheduling
			var scheduler = new PipelineScheduler(6);
			const int taskCount = 10;

			// Use IPipelineTask interface to define work.
			// This allows extensive custom configuration for your task.

			Console.WriteLine("Running {0} tasks using default settings including First Come First Serve scheduling.\n", taskCount);
			long elapsedMs = 0;
			long elapsedTicks = 0;

			scheduler.Start();
			for(int i = 0; i < taskCount; i++)
			{
				sw.Start();
				scheduler.Push(null, new CustomTask(i));
				sw.Stop();

				Console.WriteLine("Task started in {0}ms or {1} ticks", sw.ElapsedMilliseconds-elapsedMs, sw.ElapsedTicks-elapsedTicks);
				elapsedMs = sw.ElapsedMilliseconds;
				elapsedTicks = sw.ElapsedTicks;
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
				Console.WriteLine("Starting task {0}, {1} with duration of {2}ms. Thread: {3}. Priority: {4}. Active: {5}.", _id, DateTime.Now.ToString("HH:mm:ss.fff"), delay, Thread.CurrentThread.ManagedThreadId, Priority, _activeTasks);
				Thread.Sleep(delay);
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("Task {0} finished at {1} with priority {2}.", _id, DateTime.Now.ToString("hh:mm:ss.fff"), Priority);
				Console.ResetColor();
				Interlocked.Decrement(ref _activeTasks);
			}

			public DateTime ArrivalTime { get; set; }
			public TaskPriority Priority { get; set; }
			public TaskExecutionResult ExecutionResult { get; set; }
			public TaskPriority OriginalPriority { get; set; }
			public DateTime PriorityBoostTime { get; set; }
			public TimeSpan ThreadAbortTimeout { get; set; }
			public Guid Id { get; set; }

			#endregion
		}
	}
}
