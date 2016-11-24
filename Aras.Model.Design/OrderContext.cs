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
    [Model.Attributes.ItemType("v_Order Context")]
    public class OrderContext : Model.Relationship
    {

        public Double Quantity
        {
            get
            {
                Double? quanity = (Double?)this.Property("quantity").Value;

                if (quanity == null)
                {
                    return 0.0;
                }
                else
                {
                    return (Double)quanity;
                }
            }
            set
            {
                this.Property("quantity").Value = value;
            }
        }

        public String Value
        {
            get
            {
                return (String)this.Property("value").Value;
            }
            set
            {
                this.Property("value").Value = value;
            }
        }

        public Order Order
        {
            get
            {
                return (Order)this.Source;
            }
        }

        public VariantContext VariantContext
        {
            get
            {
                return (VariantContext)this.Related;
            }
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();
        }

        internal IO.Item GetIOItem()
        {
            IO.Item dbitem = new IO.Item(this.ItemType.Name, "get");
            dbitem.ID = this.ID;
            dbitem.SetProperty("value", this.Value);
            dbitem.SetProperty("quantity", this.Quantity.ToString());
            dbitem.SetProperty("related_id", this.Related.ID);
            return dbitem;
        }

        public OrderContext(Model.RelationshipType RelationshipType, Transaction Transaction, Model.Item Source, Model.Item Related)
            : base(RelationshipType, Transaction, Source, Related)
        {
         
        }

        public OrderContext(Model.RelationshipType RelationshipType, Model.Item Source, IO.Item DBItem)
            : base(RelationshipType, Source, DBItem)
        {
           
        }
    }
}
