//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pame
{
    public delegate void NotifyColleagueEventHandler<T>(T msg, object args) /*where T : enum*/;

    public class Mediator<T>
    {        
        public event NotifyColleagueEventHandler<T> NotifyColleagueEvent;
        protected static Mediator<T> instance = null;

        public static Mediator<T> Instance
        {
            get
            {
                if (instance == null)
                    instance = new Mediator<T>();
                return instance;
            }
        }

        protected Mediator()
        {
        }

        public virtual void NotifyColleague(T msg, object args)
        {
            if (NotifyColleagueEvent != null)
                NotifyColleagueEvent(msg, args);
        }                       
    }
}
