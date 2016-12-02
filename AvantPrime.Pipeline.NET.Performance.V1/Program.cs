using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Amazon;
using AvantPrime.PipelineNET;

namespace AvantPrime.Pipeline.NET.Performance.V1
{
	class Program
	{
		public static string GetProductVersion()
		{
			var attribute = (AssemblyVersionAttribute)Assembly
			  .GetExecutingAssembly()
			  .GetCustomAttributes(typeof(AssemblyVersionAttribute), true)
			  .SingleOrDefault();

			return attribute?.Version;
		}

		static void Main(string[] args)
		{
			for (int pass = 0; pass < 2; pass++)
			{
				Console.WriteLine("Pass {0}", pass+1);

				var sw = new Stopwatch();
				const int totalMessages = 1000;
				var signal = new AutoResetEvent(false);
				var syncObj = new object();
				const int pipelines = 1;

				List<string> pushQueue = new List<string>(totalMessages);
				List<string> pullQueue = new List<string>(totalMessages);
				long queueDuration = 0;
				long dequeueDuration = 0;
				int pushCounter = 0;
				int pullCounter = 0;
				int pullFailureCounter = 0;
				int pushFailureCounter = 0;
				var queueName = "PipelineQueue-V1-" + Guid.NewGuid();

				// Create queue
				using (AmazonSqs.ObjectQueue queue = new AmazonSqs.ObjectQueue(
					ConfigurationManager.AppSettings["AWSAccessKey"], // Or your AWS access key
					ConfigurationManager.AppSettings["AWSSecretKey"], // Or your AWS secret key
					RegionEndpoint.EUWest1,
					queueName
				))
				{
					// Push messages
					using (var scheduler = new PipelineScheduler(useThreadPool: true,
						threadAbortExceptionSubscriber: e =>
						{
							pushCounter++;
							pushFailureCounter++;
						}))
					{
						sw.Start();

						for (int i = 0; i < totalMessages; i++)
						{
							scheduler.Push(() =>
							{
								var s = Guid.NewGuid().ToString();
								pushQueue.Add(s);
								queue.Enqueue(new List<string>(new[] {s}));

								lock (syncObj)
								{
									pushCounter++;

									if (pushCounter == totalMessages)
									{
										sw.Stop();
										queueDuration = sw.ElapsedMilliseconds;
										signal.Set();
									}
								}
							});
						}

						Console.WriteLine("Starting pushing to the queue.");
						signal.WaitOne();
						Thread.Sleep(1000);
					}
				}

				using (AmazonSqs.ObjectQueue queue = new AmazonSqs.ObjectQueue(
					ConfigurationManager.AppSettings["AWSAccessKey"], // Or your AWS access key
					ConfigurationManager.AppSettings["AWSSecretKey"], // Or your AWS secret key
					RegionEndpoint.EUWest1,
					queueName
				))
				{
					using (var scheduler = new PipelineScheduler(useThreadPool: true,
						threadAbortExceptionSubscriber: (e) =>
						{
							pullCounter++;
							pullFailureCounter++;
						}))
					{
						sw.Restart();

						// Dequeue messages
						for (int i = 0; i < totalMessages; i++)
						{
							scheduler.Push(() =>
							{
								var response = queue.Dequeue<List<string>>();
								pullQueue.Add(response.FirstOrDefault()?.FirstOrDefault());

								lock (syncObj)
								{
									pullCounter++;

									if (pullCounter == pushCounter)
									{
										sw.Stop();
										dequeueDuration = sw.ElapsedMilliseconds;
										signal.Set();
									}
								}
							});
						}

						Console.WriteLine("Starting pulling from the queue.");
						signal.WaitOne();
						Thread.Sleep(1000);
					}

					// Remove queue
					queue.DeleteQueue();
				}

				// Show stats
				Console.WriteLine("Total messages: {0}", totalMessages);
				Console.WriteLine("Push duration: {0}", queueDuration);
				Console.WriteLine("Pull duration: {0}", dequeueDuration);
				Console.WriteLine("Push Counter: {0}", pushCounter);
				Console.WriteLine("Pull Counter: {0}", pullCounter);
				Console.WriteLine("Push Failure Counter: {0}", pushFailureCounter);
				Console.WriteLine("Pull Failure Counter: {0}", pullFailureCounter);
				Console.WriteLine("Push/Pull Intersect: {0}", pushQueue.Intersect(pullQueue).Count());
				Console.WriteLine();
			}

			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();
		}
	}
}
