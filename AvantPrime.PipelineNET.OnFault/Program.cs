using System;
using AvantPrime.PipelineNET;

namespace AvantPrime.PipelineNET.OnFault
{
	class Program
	{
		static void Main(string[] args)
		{
			var scheduler = new PipelineScheduler
				(
					onException: OnFault
				);

			using (scheduler)
			{
				scheduler.Push(new SolveProblem());

				scheduler.Start();

				Console.WriteLine("Press any key to continue...\n");
				Console.ReadKey();
			}
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
		public void Execute()
		{
			// Do Something
			Console.WriteLine("Task {0} started at {1}", Id, DateTime.Now);

			throw new Exception();
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