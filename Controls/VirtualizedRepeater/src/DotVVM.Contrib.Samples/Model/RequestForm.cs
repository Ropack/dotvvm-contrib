using System;

namespace DotVVM.Contrib.Samples.Model
{
    public class RequestForm
    {
        public int Id { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Company { get; set; }

        public string Email { get; set; }

        public string ProfileUrl { get; set; }

        public int State { get; set; }

        public DateTime RequestCreatedDate { get; set; }

        public int? UserId { get; set; }
    }
}