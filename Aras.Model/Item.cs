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
    public class Item
    {
        public Boolean IsNew { get; private set; }

        public String ID { get; private set; }

        public String ConfigID { get; private set; }

        public ItemType Type { get; private set; }

        private Dictionary<PropertyType, Property> _propertyCache;
        private Dictionary<PropertyType, Property> PropertyCache
        {
            get
            {
                if (this._propertyCache == null)
                {
                    this._propertyCache = new Dictionary<PropertyType, Property>();

                    foreach(PropertyType proptype in this.Type.PropertyTypes)
                    {
                        switch(proptype.GetType().Name)
                        {
                            case "String":
                                this._propertyCache[proptype] = new Properties.String(this, (PropertyTypes.String)proptype);
                                break;
                            case "Integer":
                                this._propertyCache[proptype] = new Properties.Integer(this, (PropertyTypes.Integer)proptype);
                                break;
                            default:
                                break;
                        }
                    }
                }

                return this._propertyCache;
            }
        }

        public Property Property(PropertyType PropertType)
        {
            return this.PropertyCache[PropertType];
        }

        public Property Property(String Name)
        {
            return this.PropertyCache[this.Type.PropertyType(Name)];
        }

        public IEnumerable<Property> Properties
        {
            get
            {
                return this.PropertyCache.Values;
            }
        }

        internal String Select
        {
            get
            {
                List<String> names = new List<String>();

                foreach(Property prop in this.PropertyCache.Values)
                {
                    if (prop.Select)
                    {
                        names.Add(prop.Type.Name);
                    }
                }

                return String.Join(",", names);
            }
            set
            {
                if (value != null)
                {
                    foreach (String name in value.Split(','))
                    {
                        this.PropertyCache[this.Type.PropertyType(name)].Select = true;
                    }
                }
            }
        }

        public Item(String ID, String ConfigID, ItemType Type)
        {
            this.ID = ID;
            this.ConfigID = ConfigID;
            this.Type = Type;
            this.IsNew = false;
        }

        public Item(ItemType Type)
        {
            this.Type = Type;
            this.ID = Server.NewID();
            this.ConfigID = this.ID;
            this.IsNew = true;
        }
    }
}
