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

namespace Aras.Model.Design
{
    [Model.Attributes.ItemType("Variant Context")]
    public class VariantContext : Model.Item
    {
        const System.Int32 DefaultMinQuantity = 0;
        const System.Int32 DefaultMaxQuantity = 10000;
        const System.Int32 DefaultSortOrder = 10000;

        public Boolean IsQuantity
        {
            get
            {
                return this.ContextType.Value.Equals("Quantity");
            }
        }

        public Boolean IsMethod
        {
            get
            {
                return this.ContextType.Value.Equals("Method");
            }
        }

        public String Method
        {
            get
            {
                if (this.Property("method").Value == null)
                {
                    return null;
                }
                else
                {
                    return (String)((Model.Item)this.Property("method").Value).Property("name").Value;
                }
            }
        }

        public Model.ListValue ContextType
        {
            get
            {
                return (Model.ListValue)this.Property("context_type").Value;
            }
        }

        public Model.List List
        {
            get
            {
                return (Model.List)this.Property("list").Value;
            }
        }

        public System.String Question
        {
            get
            {
                return (System.String)this.Property("question").Value;
            }
        }

        public System.Int32 MinQuantity
        {
            get
            {
                System.Int32? value = (System.Int32?)this.Property("min_quantity").Value;

                if (value == null)
                {
                    return DefaultMinQuantity;
                }
                else
                {
                    return (System.Int32)value;
                }
            }
        }

        public System.Int32 MaxQuantity
        {
            get
            {
                System.Int32? value = (System.Int32?)this.Property("max_quantity").Value;

                if (value == null)
                {
                    return DefaultMaxQuantity;
                }
                else
                {
                    return (System.Int32)value;
                }
            }
        }

        public System.Int32 SortOrder
        {
            get
            {
                System.Int32? value = (System.Int32?)this.Property("sort_order").Value;

                if (value == null)
                {
                    return DefaultSortOrder;
                }
                else
                {
                    return (System.Int32)value;
                }
            }
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();
        }

        public VariantContext(Model.ItemType ItemType, Transaction Transaction)
            : base(ItemType, Transaction)
        {

        }

        public VariantContext(Model.ItemType ItemType, IO.Item DBItem)
            : base(ItemType, DBItem)
        {

        }
    }
}
