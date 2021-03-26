using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace CygnusAutomation.Cygnus.Models
{
    public class Value
    {

        public string InputValue { get; set; }

        public string PauValue { get; set; }
    }

    public class Feature
    {

        public string FeatureName { get; set; }

        public string PauKey { get; set; }

        public List<Value> Values { get; set; }
    }

    public class UserCategory
    {
        public string CategoryName { get; set; }

        public List<Feature> Features { get; set; }
    }

    public class ExtendedProperty
    {

        public string IntegerValue { get; set; }

        public string StringValue { get; set; }
    }
}
