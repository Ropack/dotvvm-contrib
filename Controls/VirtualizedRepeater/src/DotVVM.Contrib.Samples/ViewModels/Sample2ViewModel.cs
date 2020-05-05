using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Contrib.Samples.ViewModels
{
    public class Sample2ViewModel : MasterViewModel
    {
        public List<Person> People { get; set; }

        public Sample2ViewModel()
        {
            People = new List<Person>();
            for (int i = 0; i < 100; i++)
            {
                People.Add(new Person()
                {
                    Id = i + 1,
                    Name = $"Person #{i}"
                });
            }
        }
    }
}

