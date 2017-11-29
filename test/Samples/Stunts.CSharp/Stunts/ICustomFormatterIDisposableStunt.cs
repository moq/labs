using System;
using System.Collections.ObjectModel;
using System.Reflection;
using Stunts;
using System.Runtime.CompilerServices;

namespace Stunts
{
    public partial class ICustomFormatterIDisposableStunt : ICustomFormatter, IDisposable, IStunt
    {
        readonly BehaviorPipeline pipeline = new BehaviorPipeline();

        ObservableCollection<IStuntBehavior> IStunt.Behaviors => pipeline.Behaviors;

        public void Dispose() => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod()));
        public override bool Equals(object obj) => pipeline.Execute<bool>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), obj));
        public string Format(string format, object arg, IFormatProvider formatProvider) => pipeline.Execute<string>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), format, arg, formatProvider));
        public override int GetHashCode() => pipeline.Execute<int>(new MethodInvocation(this, MethodBase.GetCurrentMethod()));
        public override string ToString() => pipeline.Execute<string>(new MethodInvocation(this, MethodBase.GetCurrentMethod()));
    }
}