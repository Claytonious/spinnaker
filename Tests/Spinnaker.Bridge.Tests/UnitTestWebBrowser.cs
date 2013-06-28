using System;
using System.Net;
using System.Collections.Generic;

namespace Spinnaker.Bridge.Tests
{
	/// <summary>
	/// This class pretends to be a web browser for unit testing purposes.
	/// </summary>
	public class UnitTestWebBrowser
	{
		private string url;
		private string content;
		private Dictionary<string,int> scriptFunctionsInvokedCount = new Dictionary<string, int>();

		public UnitTestWebBrowser()
		{
		}

		public string Url
		{
			get { return url; }
			set
			{
				scriptFunctionsInvokedCount.Clear();
				url = value;
				using (WebClient client = new WebClient())
				{
					content = client.DownloadString(new Uri(url));
				}
			}
		}

		public string Content { get { return content; } }

		public void ExecuteScriptFunction(string functionName, params object[] args)
		{
            int count = 0;
            scriptFunctionsInvokedCount.TryGetValue(functionName, out count);
            scriptFunctionsInvokedCount[functionName] = ++count;
		}

		public int GetNumScriptFunctionInvocations(string functionName)
		{
			int result = 0;
			scriptFunctionsInvokedCount.TryGetValue(functionName, out result);
			return result;
		}
	}
}

