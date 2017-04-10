using Microsoft.VisualStudio.TestTools.UnitTesting;

using Playground.core.Models;

namespace Playground.Core.Test
{
    [TestClass]
    public class AutoMapperTests
    {
        [TestMethod]
        public void Test()
        {
            var context = new PlaygroundContext(new Microsoft.EntityFrameworkCore.DbContextOptions<PlaygroundContext>());
        }
    }
}
