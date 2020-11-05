using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Avatars;

namespace Moq.Sdk.Tests
{
    public class Mocked : IMocked, IAvatar
    {
        IMock mock;
        readonly IList<IAvatarBehavior> behaviors = new ObservableCollection<IAvatarBehavior>();

        public IMock Mock => LazyInitializer.EnsureInitialized(ref mock, () => new DefaultMock(this));

        public IList<IAvatarBehavior> Behaviors => behaviors;
    }
}
