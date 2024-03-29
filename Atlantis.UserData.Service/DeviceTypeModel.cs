﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Atlantis.UserData.Service
{
    [DataContract]
    public class DeviceTypeModel
    {

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "unit")]
        public string Unit { get; set; }
    }
}
