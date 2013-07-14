//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using log4net.Config;

namespace Logging
{
    public class Logger
    {       
        private static ILog logger = null;

        protected Logger()
        {            
        }

        public static ILog Instance
        {
            get 
            { 
                if(logger == null)
                {
                    XmlConfigurator.Configure();
                    logger = LogManager.GetLogger(typeof(Logger));
                }

                return logger; 
            }
        }                 
    }
}
