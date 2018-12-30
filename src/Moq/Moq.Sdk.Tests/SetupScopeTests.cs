using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class SetupScopeTests
    {
        [Fact]
        public void LifeCycle()
        {
            using (new SetupScope())
            {
                Assert.True(SetupScope.IsActive);
            }

            Assert.False(SetupScope.IsActive);
        }
    }
}
