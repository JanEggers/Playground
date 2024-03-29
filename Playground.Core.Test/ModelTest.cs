﻿namespace Playground.Core.Test;

public class ModelTest
{
    [Fact]
    public void Test()
    {
        using var services = new ServiceCollection()
        .AddEntityFrameworkSqlServer()
        .AddDbContext<PlaygroundContext>(o =>
        {
            //o.UseInMemoryDatabase();
            o.UseSqlServer("Server=(localdb)\v11.0;Integrated Security=true;");
        }).BuildServiceProvider();

        var context = services.GetService<PlaygroundContext>();

        var query = context.Sites.Select(s => s.Id).ToQueryString();
    }
}
