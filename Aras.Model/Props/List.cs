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

namespace Aras.Model.Properties
{
    public class List : Property
    {
        public override object Object
        {
            get
            {
                return base.Object;
            }
            set
            {
                if (value == null)
                {
                    base.Object = value;
                }
                else
                {
                    if (value is ListValue)
                    {
                        if (((Model.PropertyTypes.List)this.PropertyType).Values.Equals(((ListValue)value).List))
                        {
                            base.Object = value;
                        }
                        else
                        {
                            throw new ArgumentException("ListValue is not from the List specified on the PropertyType");
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Object must be type Aras.Model.ListValue");
                    }
                }
            }
        }

        public ListValue Value
        {
            get
            {
                if (this.Object == null)
                {
                    return null;
                }
                else
                {
                    return (ListValue)this.Object;
                }
            }
            set
            {
                this.Object = value;
            }
        }

        public static implicit operator ListValue(Model.Properties.List Property)
        {
            return Property.Value;
        }

        public override System.String ValueString
        {
            get
            {
                if (this.Object == null)
                {
                    return null;
                }
                else
                {
                    return this.Value.Value;
                }
            }
            set
            {
                if (value == null)
                {
                    this.Object = null;
                }
                else
                {
                    this.Object = ((Model.PropertyTypes.List)this.PropertyType).Values.Value(value);
                }
            }
        }

        internal List(Model.Item Item, PropertyTypes.List PropertyType)
            : base(Item, PropertyType)
        {

        }
    }
}
