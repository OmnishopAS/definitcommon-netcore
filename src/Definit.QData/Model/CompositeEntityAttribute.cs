using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Definit.QData.Model
{
    public class CompositeEntityAttribute : Attribute
    {
        private readonly string[] _navPropertyNames;

        public CompositeEntityAttribute(params string[] navPropertyNames)
        {
            _navPropertyNames = navPropertyNames;
        }

        public string[] NavPropertyNames {
            get { return _navPropertyNames; }
        }

    }
}
