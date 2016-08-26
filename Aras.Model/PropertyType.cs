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
    public abstract class PropertyType
    {
        public ItemType Type { get; private set; }

        public System.String Name { get; private set; }

        public System.String Label { get; private set; }

        private String _columnName;
        internal String ColumnName
        {
            get
            {
                if (this._columnName == null)
                {
                    this._columnName = this.Type.TableName + ".[" + this.Name + "]";
                }

                return this._columnName;
            }
        }

        public System.Boolean ReadOnly { get; private set; }

        public System.Boolean Runtime { get; private set; }

        public System.Boolean Required { get; private set; }

        public Object Default { get; private set; }

        public override System.String ToString()
        {
            return this.Name;
        }

        internal PropertyType(ItemType Type, System.String Name, System.String Label, System.Boolean ReadOnly, System.Boolean Runtime, System.Boolean Required, Object Default)
        {
            this.Type = Type;
            this.Name = Name;
            this.Label = Label;
            this.ReadOnly = ReadOnly;
            this.Runtime = Runtime;
            this.Required = Required;
            this.Default = Default;
        }
    }
}
