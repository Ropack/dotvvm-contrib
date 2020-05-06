using System.Reflection;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ResourceManagement;

namespace DotVVM.Contrib
{
    public static class VirtualizedRepeaterConfigurationExtensions
    {
        public static void AddContribVirtualizedRepeaterConfiguration(this DotvvmConfiguration config)
        {
            config.Markup.Controls.Add(new DotvvmControlConfiguration()
            {
                Assembly = typeof(VirtualizedRepeater).Assembly.GetName().Name,
                Namespace = typeof(VirtualizedRepeater).Namespace,
                TagPrefix = "dc"
            });

            config.Markup.Controls.Add(new DotvvmControlConfiguration()
            {
                Assembly = typeof(VirtualizedGridView).Assembly.GetName().Name,
                Namespace = typeof(VirtualizedGridView).Namespace,
                TagPrefix = "dc"
            });

            // register additional resources for the control and set up dependencies
            config.Resources.Register("virtualizedForeach", new ScriptResource()
            {
                Location = new EmbeddedResourceLocation(typeof(VirtualizedRepeater).GetTypeInfo().Assembly, "DotVVM.Contrib.Scripts.virtualized-foreach.js")
            });
            config.Resources.Register("dotvvm.contrib.VirtualizedRepeater", new ScriptResource()
            {
                Location = new EmbeddedResourceLocation(typeof(VirtualizedRepeater).GetTypeInfo().Assembly, "DotVVM.Contrib.Scripts.DotVVM.Contrib.VirtualizedRepeater.js"),
                Dependencies = new[] {"dotvvm", "dotvvm.contrib.VirtualizedRepeater.css"}
            });
            config.Resources.Register("dotvvm.contrib.VirtualizedRepeater.css", new StylesheetResource()
            {
                Location = new EmbeddedResourceLocation(typeof(VirtualizedRepeater).GetTypeInfo().Assembly, "DotVVM.Contrib.Styles.DotVVM.Contrib.VirtualizedRepeater.css")
            });
            config.Resources.Register("dotvvm.contrib.VirtualizedGridView", new ScriptResource()
            {
                Location = new EmbeddedResourceLocation(typeof(VirtualizedGridView).GetTypeInfo().Assembly, "DotVVM.Contrib.Scripts.DotVVM.Contrib.VirtualizedGridView.js"),
                Dependencies = new[] {"dotvvm", "dotvvm.contrib.VirtualizedGridView.css"}
            });
            config.Resources.Register("dotvvm.contrib.VirtualizedGridView.css", new StylesheetResource()
            {
                Location = new EmbeddedResourceLocation(typeof(VirtualizedGridView).GetTypeInfo().Assembly, "DotVVM.Contrib.Styles.DotVVM.Contrib.VirtualizedGridView.css")
            });
        }
    }
}