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
    public class Date : Property
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
                    if (value is System.DateTime)
                    {
                        base.Object = value;
                    }
                    else
                    {
                        throw new ArgumentException("Object must be type System.DateTime");
                    }
                }
            }
        }

        public System.DateTime? Value
        {
            get
            {
                if (this.Object == null)
                {
                    return null;
                }
                else
                {
                    return (System.DateTime)this.Object;
                }
            }
            set
            {
                this.Object = value;
            }
        }

        internal override System.String ValueString
        {
            get
            {
                if (this.Object == null)
                {
                    return null;
                }
                else
                {
                    return ((DateTime)this.Object).ToString("yyyy-MM-ddThh:mm:ss");
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
                    this.SetObject(DateTime.Parse(value));
                }
            }
        }

        internal Date(Model.Item Item, PropertyTypes.Date PropertyType)
            : base(Item, PropertyType)
        {

        }
    }
}
