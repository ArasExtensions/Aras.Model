﻿/*  
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

                    case "Item":

                        if (this.Value != null)
                        {
                            return "(" + proptype.ColumnName + OperatorString(this.Operator) + "'" + this.Value.ToString() + "')";
                        }
                        else
                        {
                            return null;
                        }

                    case "Integer":
                    case "Decimal":
                    case "Boolean":

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
