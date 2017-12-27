<Query Kind="Statements">
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <NuGetReference>System.Net.Http</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Net.Http.Headers</Namespace>
</Query>

var comment = @"        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>";

for (int i = 6; i < 16; i++)
{
	var typeargs = string.Join(", ", Enumerable.Range(1, i + 1).Select(t => "T" + t)) + ", TResult";
	var args = string.Join(", ", Enumerable.Range(1, i + 1).Select((t, a) => $"(T{t})mi.Arguments[{a}]"));
	var method =
$@"{comment}
    public static TResult Returns<{typeargs}>(this TResult target, Func<{typeargs}> value)
        => Returns<TResult>(value, (mi, next)
            => mi.CreateValueReturn(value({args}), mi.Arguments));"
.Dump();

}