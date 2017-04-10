using Microsoft.EntityFrameworkCore;
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
            using (var context = new PlaygroundContext(new DbContextOptions<PlaygroundContext>())) 
            { 
            }
        }
    }
}
