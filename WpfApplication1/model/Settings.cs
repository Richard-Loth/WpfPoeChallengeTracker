﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Poe_Challenge_Tracker.model
{
    public class Settings
    {
        private bool autoSortCompleted;

        public bool AutoSortCompleted
        {
            get { return autoSortCompleted; }
            set { autoSortCompleted = value; }
        }

        public Settings()
        {

        }

    }
}
