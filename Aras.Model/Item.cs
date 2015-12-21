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

        private Dictionary<PropertyType, Property> PropertyCache;

        public Property Property(PropertyType PropertyType)
        {
            if (!this.PropertyCache.ContainsKey(PropertyType))
            {
                switch (PropertyType.GetType().Name)
                {
                    case "String":
                        this.PropertyCache[PropertyType] = new Properties.String(this, (PropertyTypes.String)PropertyType);
                        break;
                    case "Text":
                        this.PropertyCache[PropertyType] = new Properties.Text(this, (PropertyTypes.Text)PropertyType);
                        break;
                    case "Integer":
                        this.PropertyCache[PropertyType] = new Properties.Integer(this, (PropertyTypes.Integer)PropertyType);
                        break;
                    case "Item":
                        this.PropertyCache[PropertyType] = new Properties.Item(this, (PropertyTypes.Item)PropertyType);
                        break;
                    case "Date":
                        this.PropertyCache[PropertyType] = new Properties.Date(this, (PropertyTypes.Date)PropertyType);
                        break;
                    case "List":
                        this.PropertyCache[PropertyType] = new Properties.List(this, (PropertyTypes.List)PropertyType);
                        break;
                    case "Decimal":
                        this.PropertyCache[PropertyType] = new Properties.Decimal(this, (PropertyTypes.Decimal)PropertyType);
                        break;
                    case "Boolean":
                        this.PropertyCache[PropertyType] = new Properties.Boolean(this, (PropertyTypes.Boolean)PropertyType);
                        break;
                    default:
                        throw new NotImplementedException("Property Type not implmented: " + PropertyType.GetType().Name);
                }
            }

            return this.PropertyCache[PropertyType];
        }

        public Property Property(String Name)
        {
            return this.Property(this.Type.PropertyType(Name));
        }

        public IEnumerable<Property> Properties
        {
            get
            {
                return this.PropertyCache.Values;
            }
        }

        public Item(String ID, String ConfigID, ItemType Type)
        {
            this.PropertyCache = new Dictionary<PropertyType, Property>();
            this.Type = Type;

            if (ID == null)
            {
                this.ID = Server.NewID();
                this.ConfigID = this.ID;
                this.IsNew = true;
            }
            else
            {
                this.ID = ID;
                this.ConfigID = ConfigID;
                this.IsNew = false;
            }

        }
    }
}
