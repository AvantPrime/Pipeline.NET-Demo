using System;
using System.Threading;

namespace AvantPrime.PipelineNET.FcfsDemo2
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
			var scheduler = new PipelineScheduler(6);
			const int taskCount = 10;

			scheduler.Start();

			// Use delegate / lambda expression task
			Console.WriteLine("Running {0} tasks using lambda task & default settings including First Come First Serve scheduling.\n", taskCount);
			for (int i = 0; i < taskCount; i++)
			{
				int taskId = i;
				scheduler.Push(() =>
					{
						Interlocked.Increment(ref _activeTasks);
						var delay = Randomizer.Next(3000);
						Console.WriteLine("Starting task {0}, {1} with duration of {2}ms. Thread: {3}. Active: {4}.", taskId, DateTime.Now.ToString("hh:mm:ss.fff"), delay, Thread.CurrentThread.ManagedThreadId, _activeTasks);
						Thread.Sleep(delay);
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.WriteLine("Task {0} finished at {1}.", taskId, DateTime.Now.ToString("hh:mm:ss.fff"));
						Console.ResetColor();
						Interlocked.Decrement(ref _activeTasks);
					});
			}

			Console.WriteLine("Application/Process (main thread) finished. Press any key to exit...\n");
			Console.ReadKey();

			scheduler.Dispose();
		}
	}
}
