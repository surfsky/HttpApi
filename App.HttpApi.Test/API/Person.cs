using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace App
{
    /// <summary>
    /// Person sex
    /// </summary>
    public enum Sex
    {
        Male = 0,
        Female = 1
    }

    /// <summary>
    /// Person
    /// </summary>
    public class Person
    {
        public string Name { get; set; }
        public DateTime? Birth { get; set; }
        public Sex? Sex { get; set; }
        public Person Father { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        [ScriptIgnore]
        //[NonSerialized]
        public Person Mather { get; set; }
    }
}