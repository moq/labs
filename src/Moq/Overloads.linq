<Query Kind="Statements">
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <NuGetReference>System.Net.Http</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Net.Http.Headers</Namespace>
</Query>

var comment = @"        /// <summary>
        /// Specifies a callback to invoke when the method is called that receives the original arguments.
        /// </summary>";

for (int i = 2; i < 16; i++)
{
	var typeargs = string.Join(", ", Enumerable.Range(1, i + 1).Select(t => "T" + t));
	var args = string.Join(", ", Enumerable.Range(1, i + 1).Select((t, a) => $"(T{t})args[{a}]"));
	var method =
$@"{comment}
    public static TResult Callback<{typeargs}, TResult>(this TResult target, Action<{typeargs}> callback)
        => Callback(target, args => callback({args}));
"
.Dump();

}