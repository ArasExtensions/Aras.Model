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
using System.Net;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Security.Cryptography;

namespace Aras.Model
{
    public class Server
    {
        public IO.Server IO { get; private set; }

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

        private void LoadAssemblies()
        {
            this.ItemTypeClassCache = new Dictionary<String, Type>();

            // Ensure all assemblies in execting directory are loaded and search for Item classes
            FileInfo thisdlllocation = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);

            foreach(FileInfo dllfile in thisdlllocation.Directory.GetFiles("*.dll"))
            {
                Assembly thisassembly = Assembly.LoadFrom(dllfile.FullName);

                foreach (Type type in thisassembly.GetTypes())
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

        private object _databasesCacheLock = new object();
        private Dictionary<String, Database> _databasesCache;
        private Dictionary<String, Database> DatabaseCache
        {
            get
            {
                lock (this._databasesCacheLock)
                {
                    if (this._databasesCache == null)
                    {
                        this._databasesCache = new Dictionary<String, Database>();

                        foreach(IO.Database iodb in this.IO.Databases)
                        {
                            this._databasesCache[iodb.ID] = new Database(this, iodb);
                        }
                    }
                }

                return this._databasesCache;
            }
        }

        public IEnumerable<Database> Databases
        {
            get
            {
                return this.DatabaseCache.Values;
            }
        }

        public Database Database(String ID)
        {
            if (this.DatabaseCache.ContainsKey(ID))
            {
                return this.DatabaseCache[ID];
            }
            else
            {
                throw new Exceptions.ArgumentException("Invalid Database ID: " + ID);
            }
        }

        public override string ToString()
        {
            return this.IO.URL;
        }



        public Server(String URL)
            :base()
        {
            // Initialise Assembly Cache
            this.LoadAssemblies();

            // Create IO Server
            this.IO = new IO.Server(URL);
        }
    }
}