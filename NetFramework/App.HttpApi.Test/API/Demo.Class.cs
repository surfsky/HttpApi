using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Drawing;
using App.HttpApi;
using System.ComponentModel;
using App.Core;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.Web.Script.Serialization;
using System.Collections;

namespace App
{
    public partial class Demo
    {

        //---------------------------------------------
        // 自定义类
        //---------------------------------------------
        [HttpApi("Complex type parameter")]
        [HttpParam("father", "Father，如：{Name:'Kevin', Birth:'1979-12-01', Sex:0}")]
        public Person CreateGirl(Person father)
        {
            return new Person()
            {
                Name = father?.Name + "'s dear daughter",
                Birth = System.DateTime.Now,
                Sex = Sex.Female,
                Father = father
            };
        }

        [HttpApi("null值处理")]
        public static Person CreateNull()
        {
            return null;
        }

        [HttpApi("返回复杂对象")]
        public static Person GetPerson()
        {
            var father = new Person() { Name = "Father" };
            var mother = new Person() { Name = "Mother" };
            var son = new Person() { Name = "Son", Father = father, Mather=mother};
            var grandson = new Person() { Name = "GrandSon", Father = son };
            son.Children.Add(grandson);
            father.Children.Add(son);
            return father;
        }


        [HttpApi("返回Xml对象", Type = ResponseType.XML)]
        public static Person GetPersonXml()
        {
            return new Person() { Name = "Cherry" };
        }

        [HttpApi("返回复杂对象，并用APIResult进行封装", Wrap = true)]
        public static Person GetPersonData()
        {
            return new Person() { Name = "Kevin" };
        }

        [HttpApi("返回APIResult对象")]
        public static APIResult GetPersons()
        {
            var persons = new List<Person>(){
                new Person(){ Name="Kevin", Sex=Sex.Male, Birth=new DateTime(2000, 01, 01)},
                new Person(){ Name="Cherry", Sex=Sex.Female, Birth=new DateTime(2010, 01, 01)}
            };
            return new APIResult(true, "", persons);
        }
    }
}
