using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvantPrime.PipelineNET.General
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Starting main application.");
			Console.WriteLine("Thread Id: {0}", Thread.CurrentThread.ManagedThreadId);

			var pipe = new PipelineScheduler(ThreadingMechanism.ThreadPool);

			for (int i = 0; i < 10; i++)
			{
				var task = new SendMessageTask(i.ToString());
				pipe.Push(task);
			}

			pipe.Start();
			Console.WriteLine("Press any key to quit...");
			Console.ReadKey();
			pipe.Stop();
			pipe.Dispose();
		}

		class SendMessageTask : IPipelineTask
		{
			private readonly string _message;

			public SendMessageTask(string message)
			{
				_message = message;
			}

			public void Execute()
			{
				Console.WriteLine("Launched by Pipeline.NET on a background thread {0}, sending message {1}.", Thread.CurrentThread.ManagedThreadId, _message);
			}

			#region IPipelineTask Properties

			public DateTime ArrivalTime { get; set; }
			public TaskPriority Priority { get; set; }
			public TaskExecutionResult ExecutionResult { get; set; }
			public TaskPriority OriginalPriority { get; set; }
			public DateTime PriorityBoostTime { get; set; }
			public TimeSpan ThreadAbortTimeout { get; set; }
			public Guid Id { get; set; }

			#endregion
		}

		private static void DoSomething()
		{
			Console.WriteLine("Launched by Pipeline.NET on a background thread.");
			Console.WriteLine("Thread Id: {0}", Thread.CurrentThread.ManagedThreadId);
		}
	}
}
