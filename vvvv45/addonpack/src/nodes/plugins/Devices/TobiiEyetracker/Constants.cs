using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tobii.Eyetracking.Sdk;

namespace TobiiEyetracker
{
    public class Constants
    {
        // self defined for init state
        public const string STATUS_OFF = "off";

        // unit is working as expected:
        public const string STATUS_OK = "ok";

        // unit is up and running but the eye tracker firmware is not working as expected
        public const string STATUS_NOT_WORKING = "not-working";

        // unit is installing a firmware upgrade
        public const string STATUS_UPGRADING = "upgrading";

        // indicates a serious error. The unit is malfunctioning in some way
        public const string STATUS_ERROR = "error";

    }
}
