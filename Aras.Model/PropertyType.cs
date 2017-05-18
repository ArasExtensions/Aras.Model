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
using System.Threading.Tasks;

namespace Aras.Model
{
    public abstract class PropertyType : IComparable<PropertyType>
    {
        public ItemType Type { get; private set; }

        public System.String Name { get; private set; }

        public System.String Label { get; private set; }

        public System.Int32 SortOrder { get; private set; }

        public System.Boolean InSearch { get; private set; }

        public System.Boolean InRelationshipGrid { get; private set; }

        public System.Int32 ColumnWidth { get; private set; }

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

        public System.Boolean Required { get; private set; }

        public Object Default { get; private set; }

        public int CompareTo(PropertyType other)
        {
            if (other != null)
            {
                return this.SortOrder.CompareTo(other.SortOrder);
            }
            else
            {
                return -1;
            }
        }

        public override System.String ToString()
        {
            return this.Name;
        }

        internal PropertyType(ItemType Type, System.String Name, System.String Label, System.Boolean ReadOnly, System.Boolean Required, System.Int32 SortOrder, System.Boolean InSearch, System.Boolean InRelationshipGrid, System.Int32 ColumnWidth, Object Default)
        {
            this.Type = Type;
            this.Name = Name;
            this.Label = Label;
            this.ReadOnly = ReadOnly;
            this.Required = Required;
            this.SortOrder = SortOrder;
            this.InSearch = InSearch;
            this.InRelationshipGrid = InRelationshipGrid;
            this.ColumnWidth = ColumnWidth;
            this.Default = Default;
        }
    }
}
