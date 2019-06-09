using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Definit.QData.ChangeSets
{
    public static class ChangeSetHelper
    {
        public static object ConvertToType(Type targetType, object value)
        {
            if (value == null)
                return null;

            if (value.GetType() == targetType)
                return value;

            //Returns defaultvalue of targetype (eg 0 for int) if value is empty string ("")
            if(value is string strValue && targetType!=typeof(string) && strValue=="")
                return Activator.CreateInstance(targetType);

            var serialized = JsonConvert.SerializeObject(value);
            return JsonConvert.DeserializeObject(serialized, targetType);
        }


    }
}

