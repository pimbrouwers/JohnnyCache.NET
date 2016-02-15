using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CacheIO.Helpers
{
    public static class ObjectHelper
    {
        public static string SerializeObject (object objToSerialize)
        {
            Type type = objToSerialize.GetType();
            string objStr = null;

            if (type == typeof(DataTable))
                objStr = JsonConvert.SerializeObject(objToSerialize, new DataTableConverter());
            else if (type == typeof(DataSet))
                objStr = JsonConvert.SerializeObject(objToSerialize, new DataSetConverter());
            else
                objStr = JsonConvert.SerializeObject(objToSerialize);

            return objStr;
        }
    }
}
