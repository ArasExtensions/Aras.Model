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
using System.Threading.Tasks;

namespace Aras.Model
{
    public class Session
    {
        public Database Database { get; private set; }

        public Guid GUID { get; private set; }

        public User User { get; private set; }

        internal Aras.IOM.Innovator Innovator { get; private set; }

        private Dictionary<String, Item> ItemCache;

        internal Item ItemFromCache(Type Type, String id, String config_id, String keyed_name)
        {
            if (this.ItemCache.ContainsKey(id))
            {
                return this.ItemCache[id];
            }
            else
            {
                Item item = (Item)Activator.CreateInstance(Type, new object[] { this });
                item.id = id;
                item.config_id = config_id;
                item.keyed_name = keyed_name;
                this.ItemCache[id] = item;
                return item;
            }
        }

        internal Session(Database Database, Aras.IOM.Item User, Aras.IOM.Innovator Innovator)
        {
            this.Database = Database;
            this.GUID = Guid.NewGuid();
            this.Innovator = Innovator;
            this.ItemCache = new Dictionary<String, Item>();

            Aras.IOM.Item userrequest = this.Innovator.newItem("User", "get");
            userrequest.setID(User.getID());
            userrequest.setAttribute("select", "id,config_id,keyed_name");
            Aras.IOM.Item userresponse = userrequest.apply();

            if (!userresponse.isError())
            {
                this.User = (User)this.ItemFromCache(typeof(User), userresponse.getID(), userresponse.getProperty("config_id"), userresponse.getProperty("keyed_name"));
            }
        }
    }
}
