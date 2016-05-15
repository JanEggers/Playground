using System.Data.Entity.Migrations;

using Playground.Models;

namespace Playground.Migrations
{

    internal sealed class Configuration : DbMigrationsConfiguration<PlaygroundContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(PlaygroundContext context)
        {
        }
    }
}
