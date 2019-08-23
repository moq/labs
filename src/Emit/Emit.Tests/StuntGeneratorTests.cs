using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Stunts;
using Stunts.Emit.Static;
using Xunit;
using Xunit.Abstractions;

namespace Emit.Tests
{
    public class Foo : BaseClass, IStunt, INotifyPropertyChanged, IServiceProvider, IEnumerable<string>
    {
        readonly BehaviorPipeline pipeline = new BehaviorPipeline();

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        public string Bar
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override void Public()
        {
            base.Public();
        }

        protected override void Protected()
        {
            base.Protected();
        }

        ObservableCollection<IStuntBehavior> IStunt.Behaviors => pipeline.Behaviors;

        public string this[int index]
        {
            get => "";
            set => Console.WriteLine(value);
        }

        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<string> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class BaseClass
    {
        public virtual void Public() { }

        protected virtual void Protected() { }
    }

    public class StuntGeneratorTests
    {
        private ITestOutputHelper output;
        private static readonly string assemblyFile;

        static StuntGeneratorTests()
        {
            using (var generator = new StuntsGenerator(Assembly.GetExecutingAssembly().Location))
            {
                assemblyFile = generator.Emit();
            }
        }

        public StuntGeneratorTests(ITestOutputHelper output) => this.output = output;

        [MemberData(nameof(StuntTypes))]
        [Theory]
        public void CanInstantiateAllStunts(Type type)
        {
            try
            {
                using (Activator.CreateInstance(type) as IDisposable) { }
            }
            catch (NotImplementedException) { }
            catch (TargetInvocationException tie) when (tie.InnerException is NotImplementedException) { }
            catch (TargetInvocationException tie)
            {
                Assert.True(false, $"Failed to instantiate {type.Name}: {tie.InnerException}");
            }
        }

        public static IEnumerable<object[]> StuntTypes
            => Assembly.LoadFrom(assemblyFile).GetTypes().Where(t => typeof(IStunt).IsAssignableFrom(t)).Select(t => new object[] { t });

        [Fact]
        public void BasePublic()
        {
            Of<BaseClass>();

            var asm = Assembly.LoadFrom(assemblyFile);
            var name = Stunts.StuntNaming.GetFullName(typeof(BaseClass));
            var type = asm.GetType(name, true);

            var stunt = (IStunt)Activator.CreateInstance(type);
        }

        [Fact]
        public void Do()
        {
            Of<IServiceProvider, INotifyPropertyChanged, IComponent>();
            Of<IList<string>>();

            var asm = Assembly.LoadFrom(assemblyFile);
            var name = Stunts.StuntNaming.GetFullName(typeof(IList<string>));
            var type = asm.GetType(name, true);

            var stunt = (IStunt)Activator.CreateInstance(type);

            Assert.Equal(0, stunt.Behaviors.Count);

            stunt.AddBehavior((m, n) =>
            {
                output.WriteLine(m.ToString());
                return n()(m, n);
            });
            
            Assert.Equal(1, stunt.Behaviors.Count);
        }

        public void ThrowNotImplemented() => throw new NotImplementedException();

        [StuntGenerator]
        public T Of<T>() => default;

        [StuntGenerator]
        public T Of<T, T1>() => default;

        [StuntGenerator]
        public T Of<T, T1, T2>() => default;
    }
}
