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
    [Model.Attributes.ItemType("v_Order")]
    public class Order : Model.Item
    {
        public String ItemNumber
        {
            get
            {
                return (String)this.Property("item_number").Value;
            }
            set
            {
                this.Property("item_number").Value = value;
            }
        }

        public Part Part
        {
            get
            {
                return (Part)this.Property("part").Value;
            }
            set
            {
                this.Property("part").Value = value;
            }
        }

        public Part ConfiguredPart
        {
            get
            {
                return (Part)this.Property("configured_part").Value;
            }
            set
            {
                this.Property("configured_part").Value = value;
            }
        }

        private Dictionary<String, OrderContext> OrderContextCache;

        private void AddOrderContext(OrderContext OrderContext)
        {
            if (!this.OrderContextCache.ContainsKey(OrderContext.ID))
            {
                this.OrderContextCache[OrderContext.ID] = OrderContext;
                OrderContext.ValueList.PropertyChanged += ValueList_PropertyChanged;
            }
        }

        void ValueList_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                this.Process();
            }
        }

        private void AddVariantContext(VariantContext VariantContext)
        {

        }

        private Part GetConfiguredPart()
        {
            Model.Queries.Item query = this.ItemType.Session.Query("Part", Conditions.Eq("item_number", this.ItemNumber));
    
            if (query.Count() == 0)
            {
                // Create Part
                Part ret = (Part)this.ItemType.Session.Create("Part", this.Transaction);
                ret.ItemNumber = this.ItemNumber;
                return ret;
            }
            else
            {
                return (Part)query.First();
            }
        }

        private Boolean Processing;
        private void Process()
        {
            if (!this.Processing)
            {
                this.Processing = true;

                if (this.Status == States.Create || this.Status == States.Update)
                {
                    // Check Configured Part - same Item Number as Order
                    if (this.ConfiguredPart == null)
                    {
                        this.ConfiguredPart = this.GetConfiguredPart();
                    }
                    else
                    {
                        if (this.ItemNumber != null)
                        {
                            if (!this.ItemNumber.Equals(this.ConfiguredPart.ItemNumber))
                            {
                                this.ConfiguredPart = this.GetConfiguredPart();
                            }
                        }
                        else
                        {
                            this.ConfiguredPart = null;
                        }
                    }
                }

                this.Processing = false;
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            this.Process();
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();
        }

        public Order(String ID, Model.ItemType Type)
            :base(ID, Type)
        {
            this.OrderContextCache = new Dictionary<String, OrderContext>();
            this.Processing = false;

            // Load Order Contect already in database
            foreach (OrderContext ordercontext in this.Relationships("v_Order Context"))
            {
                this.AddOrderContext(ordercontext);
            }
        }
    }
}
