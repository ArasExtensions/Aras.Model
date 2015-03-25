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
using System.Threading.Tasks;

namespace Aras.Model
{
    public abstract class Condition
    {
        public Request.Item Item { get; private set; }

        public ItemType ItemType
        {
            get
            {
                return this.Item.ItemType;
            }
        }

        public abstract IEnumerable<Condition> Children { get; }

        protected abstract void AddChild(Condition Condition);

        public Model.Conditions.Properties.String AddProperty(PropertyTypes.String PropertyType, Conditions.Operator Operator,  System.String Value)
        {
            Model.Conditions.Properties.String condition = new Conditions.Properties.String(this.Item, PropertyType, Operator, Value);
            this.AddChild(condition);
            return condition;
        }

        public Model.Conditions.Properties.String AddProperty(String Name, Conditions.Operator Operator, System.String Value)
        {
            PropertyType propertytype = this.ItemType.PropertyType(Name);

            if (propertytype != null)
            {
                if (propertytype is PropertyTypes.String)
                {
                    return this.AddProperty((PropertyTypes.String)propertytype, Operator, Value);
                }
                else
                {
                    throw new Exceptions.ArgumentException("PropetyType is not of type String");
                }
            }
            else
            {
                throw new Exceptions.ArgumentException("PropertyType does not exist");
            }
        }

        public Conditions.OR AddOR()
        {
            Conditions.OR condition = new Conditions.OR(this.Item);
            this.AddChild(condition);
            return condition;
        }

        public Conditions.AND AddAND()
        {
            Conditions.AND condition = new Conditions.AND(this.Item);
            this.AddChild(condition);
            return condition;
        }

        internal abstract String WhereClause { get; }

        internal Condition(Request.Item Item)
        {
            this.Item = Item;
        }
    }
}
