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

namespace Aras.Model.Conditions
{
    public enum Operator { Equals, LessThan, GreaterThan }

    public abstract class Property : Condition
    {
        public override IEnumerable<Condition> Children
        {
            get 
            {
                throw new Exceptions.ArgumentException("Property Conditions do not allow Children");
            }
        }

        protected override void AddChild(Condition Condition)
        {
            throw new Exceptions.ArgumentException("Property Conditions do not allow Children");
        }

        private PropertyType _propertyType;
        public PropertyType PropertyType
        {
            get
            {
                return this._propertyType;
            }
            private set
            {
                if (value != null)
                {
                    if (this.ItemType.Equals(value.ItemType))
                    {
                        this._propertyType = value;
                    }
                    else
                    {
                        throw new Exceptions.ArgumentException("PropertyType is not associated with Condition ItemType");
                    }
                }
                else
                {
                    throw new Exceptions.ArgumentException("PropertyType must not be null");
                }
            }
        }

        private System.String _operatorString;
        protected System.String OperatorString
        {
            get
            {
                if (this._operatorString == null)
                {
                    switch(this.Operator)
                    {
                        case Conditions.Operator.Equals:
                            this._operatorString = "=";
                            break;
                        case Conditions.Operator.GreaterThan:
                            this._operatorString = ">";
                            break;
                        case Conditions.Operator.LessThan:
                            this._operatorString = "<";
                            break;
                        default:
                            throw new NotImplementedException("Property Condition Operator not implemented");
                    }
                }

                return this._operatorString;
            }
        }

        public Operator Operator { get; private set; }

        internal Property(Requests.Item Item, PropertyType PropertyType, Operator Operator)
            :base(Item)
        {
            this.PropertyType = PropertyType;
            this.Operator = Operator;
        }
    }
}
