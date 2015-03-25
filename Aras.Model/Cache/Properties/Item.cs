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

namespace Aras.Model.Cache.Properties
{
    public class Item : Property
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
                    if (value is Aras.Model.Cache.Item)
                    {
                        base.Object = value;
                    }
                    else
                    {
                        throw new ArgumentException("Object must be type Aras.Model.Cache.Item");
                    }
                }
            }
        }

        public Model.Cache.Item Value
        {
            get
            {
                if (this.Object == null)
                {
                    return null;
                }
                else
                {
                    return (Model.Cache.Item)this.Object;
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
                    return ((Model.Cache.Item)this.Object).ID;
                }
            }
            set
            {
                if (System.String.IsNullOrEmpty(value))
                {
                    this.SetObject(null);
                }
                else
                {
                    throw new NotImplementedException();
                    //this.Object = this.Session.Get(((PropertyTypes.Item)this.PropertyType).PropertyItemType, value);
                }
            }
        }

        public override string ToString()
        {
            if (this.Object == null)
            {
                return null;
            }
            else
            {
                return ((Model.Cache.Item)this.Object).ToString();
            }
        }

        internal Item(Model.Cache.Item Item, PropertyTypes.Item PropertyType)
            : base(Item, PropertyType)
        {

        }
    }
}
