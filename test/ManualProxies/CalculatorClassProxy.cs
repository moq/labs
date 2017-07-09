using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Moq.Proxy
{
    public class CalculatorClassProxy : Calculator, IProxy
    {
        BehaviorPipeline pipeline = new BehaviorPipeline();

        public ObservableCollection<IProxyBehavior> Behaviors => pipeline.Behaviors;

        public override event EventHandler TurnedOn
        {
            add => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value), (m, n) => { base.TurnedOn += value; return m.CreateValueReturn(null, value); });
            remove => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value), (m, n) => { base.TurnedOn -= value; return m.CreateValueReturn(null, value); });
        }

        public override CalculatorMode Mode
        {
            get => pipeline.Execute<CalculatorMode>(new MethodInvocation(this, MethodBase.GetCurrentMethod()), (m, n) => m.CreateValueReturn(base.Mode));
            set => pipeline.Invoke(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value), (m, n) => { base.Mode = value; return m.CreateValueReturn(null, value); });
        }

        public override int? this[string name]
        {
            get => pipeline.Execute<int?>(new MethodInvocation(this, MethodBase.GetCurrentMethod()), (m, n) => m.CreateValueReturn(base[name]));
            set => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), name, value), (m, n) => { base[name] = value; return m.CreateValueReturn(null, value); });
        }

        public override int Add(int x, int y) => 
            pipeline.Execute<int>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), x, y), (m, n) => m.CreateValueReturn(base.Add(x, y), x, y));

        public override int Add(int x, int y, int z) => 
            pipeline.Execute<int>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), x, y, z), (m, n) => m.CreateValueReturn(base.Add(x, y, z), x, y, z));

        public override bool TryAdd(ref int x, ref int y, out int z)
        {
            z = default(int);
            var local_x = x;
            var local_y = y;
            var local_z = z;

            var result = pipeline.Invoke(new MethodInvocation(this, MethodBase.GetCurrentMethod(), x, y, z),
                (m, n) => m.CreateValueReturn(base.TryAdd(ref local_x, ref local_y, out local_z), local_x, local_y, local_z), true);

            x = (int)result.Outputs["x"];
            y = (int)result.Outputs["y"];
            z = (int)result.Outputs["z"];

            return (bool)result.ReturnValue;
        }

        public override void TurnOn() =>
            pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod()), (m, n) => { base.TurnOn(); return m.CreateValueReturn(null); });

        public override void Store(string name, int value) =>
            pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), name, value), (m, n) => { base.Store(name, value); return m.CreateValueReturn(null, name, value); });

        public override int? Recall(string name) =>
            pipeline.Execute<int?>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), name), (m, n) => m.CreateValueReturn(base.Recall(name), name));


        public override void Clear(string name) =>
            pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), name), (m, n) => { base.Clear(name); return m.CreateValueReturn(null, name); });
        
    }
}