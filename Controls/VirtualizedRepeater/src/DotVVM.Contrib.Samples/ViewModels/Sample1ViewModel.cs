using System.Collections.Generic;

namespace DotVVM.Contrib.Samples.ViewModels
{
    public class Sample1ViewModel : MasterViewModel
    {
        public List<Person> People { get; set; }

        public Sample1ViewModel()
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

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}