/*  
  Aras.Model provides a .NET cient library for Aras Innovator

  Copyright (C) 2015 Processwall Limited.

  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU Affero General Public License as published
  by the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU Affero General Public License for more details.

  You should have received a copy of the GNU Affero General Public License
  along with this program.  If not, see http://opensource.org/licenses/AGPL-3.0.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
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

        private Dictionary<String, Type> ItemTypeClassCache;

        internal Type ItemTypeClass(String Name)
        {
            if (this.ItemTypeClassCache.ContainsKey(Name))
            {
                return this.ItemTypeClassCache[Name];
            }
            else
            {
                return null;
            }
        }

        public override string ToString()
        {
            return this.IO.ID;
        }

        internal Database(Server Server, IO.Database IO)
            : base()
        {
            this.SessionCache = new Dictionary<String, Session>();
            this.ItemTypeClassCache = new Dictionary<String, Type>();
            this.Server = Server;
            this.IO = IO;

            foreach(Assembly assembly in this.Server.Assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(Item)))
                    {
                        // Get Atttribute
                        Model.Attributes.ItemType itemtypeatt = (Model.Attributes.ItemType)type.GetCustomAttribute(typeof(Model.Attributes.ItemType));

                        if (itemtypeatt != null)
                        {
                            if (!this.ItemTypeClassCache.ContainsKey(itemtypeatt.Name))
                            {
                                this.ItemTypeClassCache[itemtypeatt.Name] = type;
                            }
                            else
                            {
                                if (type.IsSubclassOf(this.ItemTypeClassCache[itemtypeatt.Name]))
                                {
                                    this.ItemTypeClassCache[itemtypeatt.Name] = type;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
