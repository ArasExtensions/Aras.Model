/*  
  Copyright 2017 Processwall Limited

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
  Web:     http://www.processwall.com
  Email:   support@processwall.com
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Reflection;
using System.IO;

namespace Aras.Model
{
    public class Database
    {
        public Server Server { get; private set; }

        public IO.Database IO { get; private set; }

        public String ID
        {
            get
            {
                return this.IO.ID;
            }
        }

        private object SessionCacheLock = new object();
        private Dictionary<String, Session> SessionCache;

        public Session Login(String Username, String Password)
        {
            lock (this.SessionCacheLock)
            {
                if (!this.SessionCache.ContainsKey(Username))
                {
                    IO.Session iosession = this.IO.Login(Username, Password);
                    this.SessionCache[Username] = new Session(this, iosession);
                }

                return this.SessionCache[Username];
            }
        }

        public override string ToString()
        {
            return this.IO.ToString();
        }

        internal Database(Server Server, IO.Database IO)
            : base()
        {
            this.SessionCache = new Dictionary<String, Session>();
            this.Server = Server;
            this.IO = IO;
        }
    }
}
