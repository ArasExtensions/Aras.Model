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

namespace Aras.Design
{
    [Model.Attributes.ItemType("Part Variant Rule")]
    public class PartVariantRule : Model.Relationship
    {
        public String Value
        {
            get
            {
                return (String)this.Property("value").Value;
            }
        }

        public VariantContext VariantContext
        {
            get
            {
                return (VariantContext)this.Related;
            }
        }

        public OrderContext Selected(Order Order)
        {
            OrderContext ret = null;

            foreach (OrderContext ordercontext in Order.Relationships("v_Order Context"))
            {
                if (ordercontext.VariantContext.Equals(this.VariantContext))
                {
                    if (String.Compare(ordercontext.Value, this.Value) == 0)
                    {
                        ret = ordercontext;
                        break;
                    }
                }
            }

            return ret;
        }

        public PartVariantRule(String ID, Model.RelationshipType Type, Model.Item Source, Model.Item Related)
            :base(ID, Type, Source, Related)
        {

        }
    }
}
