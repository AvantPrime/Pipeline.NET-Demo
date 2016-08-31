using System;
using System.Collections.Concurrent;
using System.Linq;
using AvantPrime.PipelineNET;

namespace AvantPrime.PipelineNET.OnComplete
{
	class Program
	{
		private static ConcurrentBag<DateTime> _finishedDates;

		static void Main(string[] args)
		{
			const int size = 10000;

			var scheduler = new PipelineScheduler
				(
					onComplete: OnComplete,
					threadingMechanism: ThreadingMechanism.Standard,
					taskBufferSize: size,
					useTaskSchedulingOffloading: false,
					scheduler: new FcfsScheduler()
				);
			
			_finishedDates = new ConcurrentBag<DateTime>();
			var startDate = DateTime.Now;

			scheduler.Start();

			for (int i = 0; i < size; i++)
			{
				scheduler.Push(() => { });
			}

			Console.WriteLine("Press any key to get duration...\n");
			Console.ReadKey();
			scheduler.Dispose();

			var duration = _finishedDates.Max() - startDate;
			Console.WriteLine("Duration | h:{0} m:{1} s:{2} ms: {3}", duration.Hours, duration.Minutes, duration.Seconds, duration.Milliseconds);
			Console.WriteLine("Total: {0}\n", _finishedDates.Count);

			Console.WriteLine("Press any key to exit...\n");
			Console.ReadKey();
		}

		static void OnComplete(IPipelineTask task)
		{
			_finishedDates.Add(DateTime.Now);
			//Console.WriteLine("Task {0} finished at {1}.", task.Id, DateTime.Now);
		}
	}
}
