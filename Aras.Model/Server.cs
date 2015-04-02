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
using System.Xml;

namespace Aras.Model
{
    public class Server
    {
        public String URL { get; private set; }

        private object _databasesCacheLock = new object();
        private Dictionary<String, Database> _databasesCache;
        private Dictionary<String, Database> DatabaseCache
        {
            get
            {
                if (this._databasesCache == null)
                {
                    lock (this._databasesCacheLock)
                    {
                        if (this._databasesCache == null)
                        {
                            this._databasesCache = new Dictionary<String, Database>();

                            try
                            {
                                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.URL + "/Server/dblist.aspx");

                                using (WebResponse response = request.GetResponse())
                                {
                                    using (Stream result = response.GetResponseStream())
                                    {
                                        XmlDocument doc = new XmlDocument();
                                        doc.Load(result);
                                        XmlNode dblist = doc.SelectSingleNode("DBList");

                                        foreach (XmlNode db in dblist.ChildNodes)
                                        {
                                            String name = db.Attributes["id"].Value;
                                            this._databasesCache[name] = new Database(this, name);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exceptions.ServerException("Unable to connect to Server", ex);
                            }
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

        public Database Database(String Name)
        {
            return this.DatabaseCache[Name];
        }

        public override string ToString()
        {
            return this.URL;
        }

        public Server(String URL)
            :base()
        {
            this.URL = URL;
        }
    }
}
