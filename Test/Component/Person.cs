using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace App
{
    public enum Sex
    {
        Male,
        Female
    }

    public class Person
    {
        public string Name { get; set; }
        public DateTime Birth { get; set; }
        public Sex Sex { get; set; }
        public Person Father { get; set; }
    }
}