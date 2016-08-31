using System;
using System.Collections.Generic;
using System.Threading;

namespace AvantPrime.PipelineNET.ProgressMonitorDemo
{
	class Program
	{
		private static readonly object SyncObjRunning = new object();
		private static readonly Random Randomize = new Random((int)DateTime.Now.Ticks);

		private static int TotalTasks = 658;
		private static int _tasksCompleted = 0;
		private static int _tasksStarted = 0;

		static void Main(string[] args)
		{
			Console.WriteLine("Starting main application.");
			Console.WriteLine("Total Tasks: {0}", TotalTasks);

			// Create the pipeline scheduler
			var pipe = new PipelineScheduler();

			// Create the tasks first. Strictly this 
			// approach is not necessary. The task can
			// be created and launched at the same time.
			var tasks = new List<Action>();
			
			for (int i = 1; i < TotalTasks+1; i++)
			{
				var taskNumber = i;
				tasks.Add(() => MyTask(taskNumber, ProgressUpdate));
			}

			// Push tasks into the pipeline.
			for(int i = 0; i < TotalTasks; i++)
				pipe.Push(tasks[i]);

			pipe.Start();
			Console.SetCursorPosition(0, 6);
			Console.WriteLine("Press any key to quit...");
			Console.ReadKey();
			pipe.Stop();
		}

		static void MyTask(int taskNumber, Action progressUpdate)
		{
			// Register that another task has started
			Interlocked.Increment(ref _tasksStarted);

			// Simulate something being done here.
			Thread.Sleep(Randomize.Next(50));

			// These locks are only in place to ensure that
			// within this console application the text is
			// printed at the correct place. This hinders 
			// performance and may not be necessary for your
			// applications.
			lock (SyncObjRunning)
			{
				Console.SetCursorPosition(0, 3);
				Console.Write("Total tasks started: {0}.", _tasksStarted);
			}

			// Register that another task has completed
			Interlocked.Increment(ref _tasksCompleted);

			// Call the progress monitor at the end
			// of the task work being done.
			if (progressUpdate != null)
				progressUpdate();
		}

		/// <summary>
		/// Progress monitor.
		/// </summary>
		static void ProgressUpdate()
		{
			lock (SyncObjRunning)
			{
				Console.SetCursorPosition(0, 4);
				Console.Write("Completed: {0}|{1}%.", _tasksCompleted, ((double)_tasksCompleted/TotalTasks*100).ToString("0.00"));
			}
		}
	}
}
