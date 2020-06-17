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

        public DirectoryInfo AssemblyDirectory { get; set; }

        private readonly object LoadAssemblyLock = new object();
        public void LoadAssembly(String AssemblyFile)
        {
            lock (this.LoadAssemblyLock)
            {
                this.LoadAssembly(new FileInfo(this.AssemblyDirectory.FullName + "\\" + AssemblyFile + ".dll"));
            }
        }

        private List<Assembly> AssmeblyCache;
        internal IEnumerable<Assembly> Assemblies
        {
            get
            {
                return this.AssmeblyCache;
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


        private void LoadAssembly(FileInfo AssemblyFile)
        {
            if (AssemblyFile.Exists)
            {
                Assembly assembly = Assembly.LoadFrom(AssemblyFile.FullName);

                if (!this.AssmeblyCache.Contains(assembly))
                {
                    this.AssmeblyCache.Add(assembly);

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

        private readonly object _databasesCacheLock = new object();
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

                        foreach (IO.Database iodb in this.IO.Databases)
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
            : base()
        {
            // Initialise Assembly Cache
            this.AssmeblyCache = new List<Assembly>();

            // Initialise ItemType Cache
            this.ItemTypeClassCache = new Dictionary<String, Type>();

            // Create IO Server
            this.IO = new IO.Server(URL);

            // Default Assembly Directory
            this.AssemblyDirectory = new DirectoryInfo(Environment.CurrentDirectory);

            // Load this assembly
            this.LoadAssembly(new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location));
        }
    }
}