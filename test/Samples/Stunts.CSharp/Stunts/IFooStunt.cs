using System;
using System.Collections.ObjectModel;
using System.Reflection;
using Stunts;
using System.Runtime.CompilerServices;
using Sample;

namespace Stunts
{
    public partial class IFooStunt : IFoo, IStunt
    {
        readonly BehaviorPipeline pipeline = new BehaviorPipeline();

        ObservableCollection<IStuntBehavior> IStunt.Behaviors => pipeline.Behaviors;

        public int Bar() => pipeline.Execute<int>(new MethodInvocation(this, MethodBase.GetCurrentMethod()));
        public void Do() => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod()));
        public override bool Equals(object obj) => pipeline.Execute<bool>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), obj));
        public override int GetHashCode() => pipeline.Execute<int>(new MethodInvocation(this, MethodBase.GetCurrentMethod()));
        public override string ToString() => pipeline.Execute<string>(new MethodInvocation(this, MethodBase.GetCurrentMethod()));
    }
}