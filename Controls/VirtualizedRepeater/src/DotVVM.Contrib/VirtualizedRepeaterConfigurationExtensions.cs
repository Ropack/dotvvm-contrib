using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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

            // register additional resources for the control and set up dependencies
            config.Resources.Register("dotvvm.contrib.VirtualizedRepeater", new ScriptResource()
            {
                Location = new EmbeddedResourceLocation(typeof(VirtualizedRepeater).GetTypeInfo().Assembly, "DotVVM.Contrib.Scripts.DotVVM.Contrib.VirtualizedRepeater.js"),
                Dependencies = new [] { "dotvvm", "dotvvm.contrib.VirtualizedRepeater.css" }
            });
            config.Resources.Register("virtualizedForeach", new ScriptResource()
            {
                Location = new EmbeddedResourceLocation(typeof(VirtualizedRepeater).GetTypeInfo().Assembly, "DotVVM.Contrib.Scripts.virtualized-foreach.js"),
                Dependencies = new[] { "dotvvm", "dotvvm.contrib.VirtualizedRepeater.css" }
            });
            config.Resources.Register("dotvvm.contrib.VirtualizedRepeater.css", new StylesheetResource()
            {
                Location = new EmbeddedResourceLocation(typeof(VirtualizedRepeater).GetTypeInfo().Assembly, "DotVVM.Contrib.Styles.DotVVM.Contrib.VirtualizedRepeater.css")
            });

            // NOTE: all resource names should start with "dotvvm.contrib.VirtualizedRepeater"
        }

    }
}
