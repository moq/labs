using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit.Abstractions;

namespace Microsoft.Build.Framework
{
	/// <summary>
	/// Fake build engine for unit testing Tasks without spinning up 
	/// MSBuild.
	/// </summary>
	public class MockBuildEngine : IBuildEngine
	{
		bool trace = false;
		ITestOutputHelper output;

		public MockBuildEngine(bool trace = true)
		{
			this.trace = trace;
			LoggedCustomEvents = new List<CustomBuildEventArgs>();
			LoggedErrorEvents = new List<BuildErrorEventArgs>();
			LoggedMessageEvents = new List<BuildMessageEventArgs>();
			LoggedWarningEvents = new List<BuildWarningEventArgs>();
		}

		public MockBuildEngine(ITestOutputHelper output, bool trace = false)
			: this(trace)
		{
			this.output = output;
		}

		public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
		{
			throw new NotSupportedException();
		}

		public int ColumnNumberOfTaskNode { get; set; }

		public bool ContinueOnError { get; set; }

		public int LineNumberOfTaskNode { get; set; }

		public string ProjectFileOfTaskNode { get; set; }

		public List<CustomBuildEventArgs> LoggedCustomEvents { get; private set; }
		public List<BuildErrorEventArgs> LoggedErrorEvents { get; private set; }
		public List<BuildMessageEventArgs> LoggedMessageEvents { get; private set; }
		public List<BuildWarningEventArgs> LoggedWarningEvents { get; private set; }

		public void LogCustomEvent(CustomBuildEventArgs e)
		{
			if (trace)
				TraceMessage(e.Message);

			LoggedCustomEvents.Add(e);
		}

		public void LogErrorEvent(BuildErrorEventArgs e)
		{
			if (trace)
				TraceMessage(e.Message);

			LoggedErrorEvents.Add(e);
		}

		public void LogMessageEvent(BuildMessageEventArgs e)
		{
			if (trace)
				TraceMessage(e.Message);

			LoggedMessageEvents.Add(e);
		}

		public void LogWarningEvent(BuildWarningEventArgs e)
		{
			if (trace)
				TraceMessage(e.Message);

			LoggedWarningEvents.Add(e);
		}

		void TraceMessage(string message)
		{
			Console.WriteLine(message);
			Trace.WriteLine(message);
			Debug.WriteLine(message);
			Debugger.Log(0, "", message);
			if (output != null)
				output.WriteLine(message);
		}
	}
}