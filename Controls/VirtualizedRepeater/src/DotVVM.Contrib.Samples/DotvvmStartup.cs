using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Contrib.Samples
{
    public class DotvvmStartup : IDotvvmStartup, IDotvvmServiceConfigurator
    {
        // For more information about this class, visit https://dotvvm.com/docs/tutorials/basics-project-structure
        public void Configure(DotvvmConfiguration config, string applicationPath)
        {
            config.AddContribVirtualizedRepeaterConfiguration();

            ConfigureRoutes(config, applicationPath);
            ConfigureControls(config, applicationPath);
            ConfigureResources(config, applicationPath);
        }

        private void ConfigureRoutes(DotvvmConfiguration config, string applicationPath)
        {
            config.RouteTable.Add("_Default", "", "Views/_default.dothtml");
            config.RouteTable.AutoDiscoverRoutes(new SamplesRouteStrategy(config));

            RegisterBenchmarkRoutes(config);
        }

        private static void RegisterBenchmarkRoutes(DotvvmConfiguration config)
        {
            config.RouteTable.Add("Benchmark1GridView", "Benchmark1GridView/{Count}", "Views/Benchmark1GridView.dothtml", new {Count = 100});
            config.RouteTable.Add("Benchmark1VirtualizedGridView", "Benchmark1VirtualizedGridView/{Count}", "Views/Benchmark1VirtualizedGridView.dothtml", new {Count = 100});
            config.RouteTable.Add("Benchmark2GridView", "Benchmark2GridView/{Count}", "Views/Benchmark2GridView.dothtml", new {Count = 100});
            config.RouteTable.Add("Benchmark2VirtualizedGridView", "Benchmark2VirtualizedGridView/{Count}", "Views/Benchmark2VirtualizedGridView.dothtml", new {Count = 100});
            config.RouteTable.Add("Benchmark3GridView", "Benchmark3GridView/{Count}", "Views/Benchmark3GridView.dothtml", new {Count = 100});
            config.RouteTable.Add("Benchmark3VirtualizedGridView", "Benchmark3VirtualizedGridView/{Count}", "Views/Benchmark3VirtualizedGridView.dothtml", new {Count = 100});
        }

        private void ConfigureControls(DotvvmConfiguration config, string applicationPath)
        {
            // register code-only controls and markup controls
        }

        private void ConfigureResources(DotvvmConfiguration config, string applicationPath)
        {
            // register custom resources and adjust paths to the built-in resources
        }

        public void ConfigureServices(IDotvvmServiceCollection services)
        {
            services.AddDefaultTempStorages("Temp");

        }
    }

    internal class SamplesRouteStrategy : DefaultRouteStrategy
    {
        public SamplesRouteStrategy(DotvvmConfiguration config) : base(config)
        {
        }

        protected override IEnumerable<RouteStrategyMarkupFileInfo> DiscoverMarkupFiles()
        {
            return base.DiscoverMarkupFiles().Where(r => !r.ViewsFolderRelativePath.StartsWith("_")).Where(x=>!x.ViewsFolderRelativePath.StartsWith("Benchmark"));
        }
    }
}
