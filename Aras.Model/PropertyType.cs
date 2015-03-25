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

namespace Aras.Model
{
    public abstract class PropertyType
    {
        public ItemType ItemType { get; private set; }

        public String Name { get; private set; }

        private String _columnName;
        internal String ColumnName
        {
            get
            {
                if (this._columnName == null)
                {
                    this._columnName = this.ItemType.TableName + "." + this.Name; 
                }

                return this._columnName;
            }
        }

        public String Label { get; private set; }

        public Boolean ReadOnly { get; private set; }

        public override string ToString()
        {
            return this.Name;
        }

        internal PropertyType(ItemType ItemType, String Name, String Label, Boolean ReadOnly)
            :base()
        {
            this.ItemType = ItemType;
            this.Name = Name;
            this.Label = Label;
            this.ReadOnly = ReadOnly;
        }
    }
}
