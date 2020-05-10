using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Contrib.Samples.Model;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Contrib.Samples.ViewModels
{
    public class Benchmark3ViewModel : DotvvmViewModelBase
    {
        public List<RequestForm> RequestForms => Enumerable.Range(0, Count).Select(GetRequestForm).ToList();

        private RequestForm GetRequestForm(int x)
        {
            return new RequestForm()
            {
                Id = 1, 
                FirstName = $"First name #{x}",
                LastName = $"Last name #{x}",
                Company = $"Company #{x}",
                Email = $"mail{x}@mail.com",
                ProfileUrl = "/someUrl",
                RequestCreatedDate = DateTime.Now,
                State = 1,
                UserId = 1
            };
        }

        [FromRoute("Count")]
        public int Count { get; set; }

        public void Approve(RequestForm requestForm)
        {
            // do something
        }
        public void Deny(RequestForm requestForm)
        {
            // do something
        }
    }
}