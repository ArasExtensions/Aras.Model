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
    public abstract class Item
    {
        public Session Session { get; private set; }

        public Guid GUID { get; private set; }

        public String id { get; internal set; }

        public String config_id { get; internal set; }

        public String keyed_name { get; internal set; }

        private String _major_rev;
        [Attributes.PropertyType("major_rev")]
        public String major_rev
        {
            get
            {
                return this._major_rev;
            }
            set
            {
                this._major_rev = value;
            }
        }

        private String _itemType;
        internal String ItemType
        {
            get
            {
                if (this._itemType == null)
                {
                    object[] attributes = this.GetType().GetCustomAttributes(typeof(Attributes.ItemType), true);

                    if (attributes.Length == 1)
                    {
                        this._itemType = ((Attributes.ItemType)attributes[0]).Name;
                    }
                    else
                    {
                        throw new NotImplementedException("Class must have ItemType Attribute: " + this.GetType().FullName);
                    }
                }

                return this._itemType;
            }
        }

        public override string ToString()
        {
            return this.keyed_name;
        }

        public Item(Session Session)
        {
            this.Session = Session;
            this.GUID = Guid.NewGuid();
        }
    }
}
