using System;
using System.Threading;
using AvantPrime.PipelineNET;

namespace AvantPrime.PipelineNET.OnCancel
{
	class Program
	{
		static void Main(string[] args)
		{
			var scheduler = new PipelineScheduler
				(
					abortLongRunningTasks: true,
					abortTaskTimeoutInterval: 100,
					onException: OnFault,
					onCancel: OnCancel
				);

			using (scheduler)
			{
				scheduler.Push(new SolveProblem(OnFault));

				scheduler.Start();

				Console.WriteLine("Press any key to continue...\n");
				Console.ReadKey();
			}
		}

		static void OnCancel(IPipelineTask task)
		{
			// Do something when task cancels
			Console.WriteLine("Task cancelled at {0}.", DateTime.Now);
		}

		static void OnFault(IPipelineTask task, Exception e)
		{
			if (task != null)
			{
				var sTask = task as SolveProblem;

				// Do something when task completes
				if (sTask != null)
					Console.WriteLine("Task {0} faulted at {1}.", sTask.Id, DateTime.Now);
				else
					Console.WriteLine("Task {0} faulted at {1}.", task, DateTime.Now);
			}
			else
				Console.WriteLine("Task faulted at {0}.", DateTime.Now);
		}
	}

	class SolveProblem : IPipelineTask
	{
		private readonly Action<SolveProblem, Exception> _onFault;

		public SolveProblem(Action<SolveProblem, Exception> onFault)
		{
			_onFault = onFault;
			ThreadAbortTimeout = TimeSpan.FromMilliseconds(3000);
		}

		public void Execute()
		{
			// Do Something
			Console.WriteLine("Task {0} started at {1}", Id, DateTime.Now);

			Thread.Sleep(5000);

			Console.WriteLine("Task {0} completed at {1}", Id, DateTime.Now);
		}

		public DateTime ArrivalTime { get; set; }
		public TaskPriority Priority { get; set; }
		public TaskExecutionResult ExecutionResult { get; set; }
		public TaskPriority OriginalPriority { get; set; }
		public DateTime PriorityBoostTime { get; set; }
		public TimeSpan ThreadAbortTimeout { get; set; }
		public Guid Id { get; set; }
	}
}
