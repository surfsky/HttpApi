using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Web.Script.Serialization;
using System.Xml.Serialization;
//using App.Core;
using System.ComponentModel;
using App.Utils;

namespace App
{
    /// <summary>
    /// Person sex
    /// </summary>
    public enum Sex
    {
        [UI("男")]            Male = 0,
        [UI("女")]            Female = 1,
        [Description("不详")] Unknown = 2,
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
        public List<Person> Children { get; set; } = new List<Person>();

        [JsonIgnore]
        [XmlIgnore]
        //[ScriptIgnore]
        //[NonSerialized]
        public Person Mather { get; set; }
    }
}