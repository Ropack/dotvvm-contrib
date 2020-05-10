﻿using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Contrib.Samples.ViewModels
{
    public class Benchmark1ViewModel : DotvvmViewModelBase
    {
        public List<Person> People => Enumerable.Range(0, Count).Select(x => new Person() { Id = 1, Name = $"Person #{x}" }).ToList();

        [FromRoute("Count")] 
        public int Count { get; set; }
    }
}