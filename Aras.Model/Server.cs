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

        public DirectoryInfo AssemblyDirectory { get; set; }

        public void LoadAssembly(String AssemblyFile)
        {
            this.LoadAssembly(new FileInfo(this.AssemblyDirectory.FullName + "\\" + AssemblyFile + ".dll"));
        }

        private List<Assembly> AssmeblyCache;
        internal IEnumerable<Assembly> Assemblies
        {
            get
            {
                return this.AssmeblyCache;
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
            // Initialise Assebly Cache
            this.AssmeblyCache = new List<Assembly>();

            // Create IO Server
            this.IO = new IO.Server(URL);

            // Default Assembly Directory
            this.AssemblyDirectory = new DirectoryInfo(Environment.CurrentDirectory);

            // Load this assembly
            this.LoadAssembly(new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location));
        }
    }
}