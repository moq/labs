using Microsoft.Build.Framework;
using System.Collections.Generic;
using Xunit.Abstractions;

/// <summary>
/// xunit logger > MSBuild logger
/// </summary>
class TestOutputLogger : ILogger
{
	ITestOutputHelper output;

	public TestOutputLogger(ITestOutputHelper output, LoggerVerbosity? verbosity = LoggerVerbosity.Quiet)
	{
		this.output = output;
		this.Verbosity = verbosity.GetValueOrDefault();
	}

	public void Reset()
	{
		Messages = new List<BuildMessageEventArgs>();
		Warnings = new List<BuildWarningEventArgs>();
		Errors = new List<BuildErrorEventArgs>();
		FinishedTargets = new List<TargetFinishedEventArgs>();
		FinishedTasks = new List<TaskFinishedEventArgs>();
	}

	public LoggerVerbosity Verbosity { get; set; }

	public List<BuildMessageEventArgs> Messages { get; private set; } = new List<BuildMessageEventArgs>();

	public List<BuildWarningEventArgs> Warnings { get; private set; } = new List<BuildWarningEventArgs>();

	public List<BuildErrorEventArgs> Errors { get; private set; } = new List<BuildErrorEventArgs>();

	public List<TargetFinishedEventArgs> FinishedTargets { get; private set; } = new List<TargetFinishedEventArgs>();

	public List<TaskFinishedEventArgs> FinishedTasks { get; private set; } = new List<TaskFinishedEventArgs>();

	public void Initialize(IEventSource eventSource)
	{
		eventSource.TargetFinished += (sender, e) => FinishedTargets.Add(e);
		eventSource.TaskFinished += (sender, e) => FinishedTasks.Add(e);

		eventSource.AnyEventRaised += (sender, e) =>
		{
			if (!(e is BuildMessageEventArgs) && Verbosity > LoggerVerbosity.Normal)
				output?.WriteLine(e.Message);
		};

		eventSource.MessageRaised += (sender, e) =>
		{
			var shouldLog = (e.Importance == MessageImportance.High);
			switch (e.Importance)
			{
				case MessageImportance.Normal:
					shouldLog = (Verbosity >= LoggerVerbosity.Normal);
					break;
				case MessageImportance.Low:
					shouldLog = (Verbosity >= LoggerVerbosity.Detailed);
					break;
				default:
					break;
			}

			if (Verbosity != LoggerVerbosity.Quiet && shouldLog)
				output?.WriteLine(e.Message);

			Messages.Add(e);
		};

		eventSource.ErrorRaised += (sender, e) =>
		{
			output?.WriteLine(e.Message);
			Errors.Add(e);
		};

		eventSource.WarningRaised += (sender, e) =>
		{
			if (Verbosity != LoggerVerbosity.Quiet)
				output?.WriteLine(e.Message);

			Warnings.Add(e);
		};
	}

	public string Parameters { get; set; }

	public void Shutdown()
	{
	}
}