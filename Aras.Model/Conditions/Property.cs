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
    public enum Operators { eq, ne, lt, gt, le, ge, like };

    public class Property : Condition
    {
        public String Name { get; private set; }

        public Operators Operator { get; private set; }

        public Object Value { get; private set; }

        private static System.String OperatorString(Conditions.Operators Operator)
        {
            switch (Operator)
            {
                case Conditions.Operators.eq:
                    return "=";
                case Conditions.Operators.ne:
                    return "<>";
                case Conditions.Operators.gt:
                    return ">";
                case Conditions.Operators.lt:
                    return "<";
                case Conditions.Operators.le:
                    return "<=";
                case Conditions.Operators.ge:
                    return ">=";
                case Conditions.Operators.like:
                    return "like";
                default:
                    throw new NotImplementedException("Property Condition Operator not implemented");
            }
        }

        internal override string Where(ItemType ItemType)
        {
            if (this.Name == "id")
            {
                if (this.Value == null)
                {
                    return "(" + ItemType.TableName + ".[id] is null)";
                }
                else
                {
                    return "(" + ItemType.TableName + ".[id]" + OperatorString(this.Operator) + "'" + this.Value.ToString() + "')";
                }
            }
            else
            {
                PropertyType proptype = ItemType.PropertyType(this.Name);

                switch (proptype.GetType().Name)
                {
                    case "String":
                    case "Sequence":
                    case "Text":
                    case "List":

                        if (this.Value == null)
                        {
                            return "(" + proptype.ColumnName + " is null)";
                        }
                        else
                        {
                            return "(" + proptype.ColumnName + OperatorString(this.Operator) + "'" + this.Value.ToString().Replace('*', '%') + "')";
                        }

                    case "Integer":
                    case "Decimal":

                        if (this.Value == null)
                        {
                            return "(" + proptype.ColumnName + " is null)";
                        }
                        else
                        {
                            return "(" + proptype.ColumnName + OperatorString(this.Operator) + this.Value.ToString() + ")";
                        }

                    default:
                        throw new Exceptions.ArgumentException("Property Type not implemented: " + proptype.GetType().Name);
                }
            }
        }

        public override bool Equals(Condition other)
        {
            if (other != null && other is Property)
            {
                return this.Name.Equals(((Property)other).Name) && this.Operator.Equals(((Property)other).Operator) && this.Value.Equals(((Property)other).Value);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode() ^ this.Operator.GetHashCode() ^ this.Value.GetHashCode();
        }

        internal Property(String Name, Operators Operator, Object Value)
            : base()
        {
            this.Name = Name;
            this.Operator = Operator;
            this.Value = Value;
        }
    }
}
