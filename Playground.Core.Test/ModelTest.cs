using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Playground.core.Models;
using Xunit;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Playground.Core.Test
{
    public class ModelTest
    {
        [Fact]
        public void Test()
        {
            var conventions = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventions);

            //var c = modelBuilder.Entity<Company>().HasKey(p => p.Id);
            //var c2 = modelBuilder.Entity<CompanySub>();
            //var c3 = modelBuilder.Entity<CompanySub2>();

            var model = modelBuilder.Model;
             
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddEntityFrameworkSqlServer();
            serviceCollection.AddDbContext<PlaygroundContext>(o=> {
                //o.UseInMemoryDatabase();
                o.UseSqlServer("Server=(localdb)\v11.0;Integrated Security=true;");
            });

            var services = serviceCollection.BuildServiceProvider();

            var  context = services.GetService<PlaygroundContext>();

            var nodeTypeProvider = context.GetService<MethodInfoBasedNodeTypeRegistry>();
            //var infos = context.GetService<IRelationalAnnotationProvider>();

            //var companies = context.Companies.OfType<CompanySub>().Where( c => c.Name == "wtf" );
            //var cc = (EntityQueryProvider)companies.Provider;
            //var compiler = (QueryCompiler)cc.GetType().GetTypeInfo().GetField("_queryCompiler", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(cc);
            //var db = (IDatabase)compiler.GetType().GetTypeInfo().GetProperty("Database", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(compiler);
            //var qccf = (IQueryCompilationContextFactory)db.GetType().GetTypeInfo().BaseType.GetField("_queryCompilationContextFactory", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(db);
            //var qcf = (IQueryContextFactory)compiler.GetType().GetTypeInfo().GetField("_queryContextFactory", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(compiler);
                        
            //var qcc = qccf.Create(false);
            //var qc = qcf.Create();

            //var queryParser = new QueryParser(new ExpressionTreeParser(
            //nodeTypeProvider: nodeTypeProvider,
            //processor: new CompoundExpressionTreeProcessor(new IExpressionTreeProcessor[]
            //{
            //   // new PartialEvaluatingExpressionTreeProcessor(new ApiCompilationFilter()),
            //    new TransformingExpressionTreeProcessor(ExpressionTransformerRegistry.CreateDefault())
            //})));


            //var queryModel = queryParser.GetParsedQuery(companies.Expression);

            //var companyEntity = qcc.Model.FindEntityType(typeof(Company));
            //var info = infos.For(companyEntity);

            //var eqmv = (RelationalQueryModelVisitor)qcc.CreateQueryModelVisitor();  // <- best guess
            
            //var executor = eqmv.CreateQueryExecutor<Company>(queryModel);

            //var select = eqmv.TryGetQuery(queryModel.MainFromClause);

            //var result = executor(qc).ToList();

            //db.CompileQuery(qc,)

            //var loggerFactory = new LoggerFactory();
            //var queryModelVisitorFactory = new EntityResultFindingExpressionVisitorFactory(model, );
            //var queryMaterializerFactory = new RequiresMaterializationExpressionVisitorFactory(model)
            //var queryCompilationContext = new QueryCompilationContext(
            //    model,
            //    loggerFactory.CreateLogger(""), 
            //    queryModelVisitorFactory,
            //    queryMaterializerFactory,
            //    null,
            //    typeof(PlaygroundContext), false);
        }
    }
}
