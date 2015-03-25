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
  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
  Email:   support@processwall.com
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Aras.Model.Cache
{
    public class Item : IEquatable<Item>
    {
        public ItemType ItemType { get; private set; }

        public Session Session
        {
            get
            {
                return this.ItemType.Session;
            }
        }

        public String ID
        {
            get
            {
                Property idproperty = this.Property("id");

                if (idproperty != null)
                {
                    return ((Model.Cache.Properties.String)idproperty).Value;
                }
                else
                {
                    return null;
                }
            }
        }

        public String KeyedName
        {
            get
            {
                Property knproperty = this.Property("keyed_name");

                if (knproperty != null)
                {
                    return ((Model.Cache.Properties.String)knproperty).Value;
                }
                else
                {
                    return null;
                }
            }
        }

        private Class _class;
        public Class Class 
        {
            get
            {
                return this._class;
            }
            set
            {
                if (value != null)
                {
                    if (value.ItemType.Equals(this.ItemType))
                    {
                        this._class = value;
                    }
                    else
                    {
                        throw new Exceptions.ArgumentException("Class is from another ItemType");
                    }
                }
                else
                {
                    throw new Exceptions.ArgumentException("Class cannot be null");
                }
            }
        }

        private Dictionary<PropertyType, Property> PropertiesCache;

        public Property AddProperty(PropertyType PropertyType, object Value)
        {
            if (this.ItemType.Equals(PropertyType.ItemType))
            {
                if (!this.PropertiesCache.ContainsKey(PropertyType))
                {
                    switch (PropertyType.GetType().Name)
                    {
                        case "String":
                            this.PropertiesCache[PropertyType] = new Properties.String(this, (PropertyTypes.String)PropertyType);
                            break;
                        case "Item":
                            this.PropertiesCache[PropertyType] = new Properties.Item(this, (PropertyTypes.Item)PropertyType);
                            break;
                        case "Date":
                            this.PropertiesCache[PropertyType] = new Properties.Date(this, (PropertyTypes.Date)PropertyType);
                            break;
                        case "Text":
                            this.PropertiesCache[PropertyType] = new Properties.Text(this, (PropertyTypes.Text)PropertyType);
                            break;
                        case "Integer":
                            this.PropertiesCache[PropertyType] = new Properties.Integer(this, (PropertyTypes.Integer)PropertyType);
                            break;
                        case "Boolean":
                            this.PropertiesCache[PropertyType] = new Properties.Boolean(this, (PropertyTypes.Boolean)PropertyType);
                            break;
                        case "List":
                            this.PropertiesCache[PropertyType] = new Properties.List(this, (PropertyTypes.List)PropertyType);
                            break;
                        case "Sequence":
                            this.PropertiesCache[PropertyType] = new Properties.Sequence(this, (PropertyTypes.Sequence)PropertyType);
                            break;
                        case "Foreign":
                            this.PropertiesCache[PropertyType] = new Properties.Foreign(this, (PropertyTypes.Foreign)PropertyType);
                            break;
                        case "Image":
                            this.PropertiesCache[PropertyType] = new Properties.Image(this, (PropertyTypes.Image)PropertyType);
                            break;
                        case "Decimal":
                            this.PropertiesCache[PropertyType] = new Properties.Decimal(this, (PropertyTypes.Decimal)PropertyType);
                            break;
                        case "Float":
                            this.PropertiesCache[PropertyType] = new Properties.Float(this, (PropertyTypes.Float)PropertyType);
                            break;
                        case "ColorList":
                            this.PropertiesCache[PropertyType] = new Properties.ColorList(this, (PropertyTypes.ColorList)PropertyType);
                            break;
                        default:
                            throw new NotImplementedException("PropertyType not implemented :" + PropertyType.GetType().FullName);
                    }
                }

                this.PropertiesCache[PropertyType].SetObject(Value);

                return this.PropertiesCache[PropertyType];
            }
            else
            {
                throw new Exceptions.ArgumentException("PropertyType is not associated with the ItemType of the Item");
            }
        }

        public Property AddProperty(String Name, object Value)
        {
            PropertyType proptype = this.ItemType.PropertyType(Name);

            if (proptype != null)
            {
                return this.AddProperty(proptype, Value);
            }
            else
            {
                throw new Exceptions.ArgumentException("Property does not exist");
            }
        }

        public IEnumerable<Property> Properties
        {
            get
            {
                return this.PropertiesCache.Values;
            }
        }

        public Property Property(PropertyType PropertyType)
        {
            if (this.ItemType.Equals(PropertyType.ItemType))
            {
                if (this.PropertiesCache.ContainsKey(PropertyType))
                {
                    return this.PropertiesCache[PropertyType];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                throw new Exceptions.ArgumentException("PropertyType is not associated with the Item ItmeType");
            }
        }

        public Property Property(String Name)
        {
            PropertyType propertytype = this.ItemType.PropertyType(Name);

            if (propertytype != null)
            {
                return this.Property(propertytype);
            }
            else
            {
                throw new ArgumentException("Invalid property name: " + Name);
            }
        }

        [System.Runtime.CompilerServices.IndexerName("PropertyValue")]
        public Property this[String Name]
        {
            get
            {
                return this.Property(Name);
            }
        }

        /*
        private List<Item> _promotions;
        public IEnumerable<Item> Promotions
        {
            get
            {
                if (this._promotions == null)
                {
                    this._promotions = new List<Item>();

                    Aras.IOM.Item request = this.Session.Innovator.newItem(this.ItemType.Name, "getItemNextStates");
                    request.setID(this.ID);
                    Aras.IOM.Item response = request.apply();

                    if (!response.isError())
                    {
                        for (int i = 0; i < response.getItemCount(); i++)
                        {
                            Aras.IOM.Item lct = response.getItemByIndex(i);
                            Aras.IOM.Item lcs = lct.getPropertyItem("to_state");

                            Item lifecyclestate = this.Session.GetItemFromCache(this.Session.AnyItemType("Life Cycle State"), lcs.getID());

                            if (lifecyclestate == null)
                            {
                                lifecyclestate = this.Session.CreateItem(this.Session.AnyItemType("Life Cycle State"), lcs.getID(), lcs.getProperty("confid_id"), lcs.getProperty("keyed_name"), null, "1", "1", Operations.Read);
                                this.Session.AddItemToCache(lifecyclestate);
                            }

                            this._promotions.Add(lifecyclestate);
                        }
                    }
                    else
                    {
                        throw new Exceptions.ServerException(response);
                    }
                }

                return this._promotions;
            }
        }

        public void Promote(Item State)
        {
            if (State != null)
            {
                Aras.IOM.Item request = this.Session.Innovator.newItem(this.ItemType.Name, "promoteItem");
                request.setID(this.ID);
                request.setProperty("state", State.KeyedName);
                Aras.IOM.Item response = request.apply();

                if (!response.isError())
                {
                    ((Properties.Item)this["current_state"]).Value = State;
                    ((Properties.String)this["state"]).Value = State.KeyedName;
                    this._promotions = null;
                }
                else
                {
                    throw new Exceptions.ServerException(response);
                }
            }
            else
            {
                throw new Exceptions.ArgumentException("State must not be null");
            }
        }
        */

        public bool Equals(Item other)
        {
            if (other == null)
            {
                return false;
            }
            else
            {
                return this.ID.Equals(other.ID) && this.ItemType.Equals(other.ItemType);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else
            {
                if (obj is Item)
                {
                    return this.Equals((Item)obj);
                }
                else
                {
                    return false;
                }
            }
        }

        public override int GetHashCode()
        {
            if (this.ID != null)
            {
                return this.ID.GetHashCode() ^ this.ItemType.GetHashCode();
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public override string ToString()
        {
            if (this.KeyedName != null)
            {
                return this.KeyedName;
            }
            else
            {
                if (this.ID != null)
                {
                    return this.ID;
                }
                else
                {
                    return base.ToString();
                }
            }
        }

        internal Item(ItemType ItemType)
        {
            this.PropertiesCache = new Dictionary<PropertyType, Property>();
            this.ItemType = ItemType;
        }
    }
}
