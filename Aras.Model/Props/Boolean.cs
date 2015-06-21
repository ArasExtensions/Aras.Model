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
    public class Boolean : Property
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
                    if (value is System.Boolean)
                    {
                        base.Object = value;
                    }
                    else
                    {
                        throw new ArgumentException("Object must be type System.Boolean");
                    }
                }
            }
        }

        public System.Boolean? Value
        {
            get
            {
                if (this.Object == null)
                {
                    return null;
                }
                else
                {
                    return (System.Boolean)this.Object;
                }
            }
            set
            {
                this.Object = value;
            }
        }

        public static implicit operator System.Boolean?(Model.Properties.Boolean Property)
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
                    if (((System.Boolean)this.Object).Equals(true))
                    {
                        return "1";
                    }
                    else
                    {
                        return "0";
                    }
                }
            }
            set
            {
                if (value == null)
                {
                    this.SetObject(null);
                }
                else
                {
                    this.SetObject(value.Equals("1"));
                }
            }
        }

        internal Boolean(Model.Item Item, PropertyTypes.Boolean PropertyType)
            : base(Item, PropertyType)
        {

        }
    }
}
