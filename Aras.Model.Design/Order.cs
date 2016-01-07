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
            Model.Queries.Item query = this.ItemType.Session.Query("Part", Aras.Conditions.Eq("item_number", this.ItemNumber));
    
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

        private IEnumerable<PartBOM> AllConfiguredPartBOM(Order Order, Part Part)
        {
            List<PartBOM> ret = new List<PartBOM>();

            foreach (PartBOM partbom in Part.ConfiguredPartBOM(Order, false))
            {
                ret.Add(partbom);

                if (partbom.Related != null)
                {
                    foreach (PartBOM childpartbom in this.AllConfiguredPartBOM(Order, (Part)partbom.Related))
                    {
                        ret.Add(childpartbom);
                    }
                }
            }

            return ret;
        }

        private Boolean Processing;
        private void Process()
        {
            if (!this.Processing)
            {
                this.Processing = true;

                if (((this.Status == States.Create) || (this.Status == States.Update)) && (this.Part != null))
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

                    // Update Properties of Configured Part
                    this.ConfiguredPart.Update(this.Transaction);
                    this.ConfiguredPart.Class = this.ConfiguredPart.ItemType.GetClassName("Assembly");
                    this.ConfiguredPart.Property("name").Value = this.Property("name").Value;
                    this.ConfiguredPart.Property("description").Value = this.Property("description").Value;

                    // Add any missing Order Context
                    foreach(VariantContext variantcontext in this.Part.VariantContext(this))
                    {
                        Boolean exists = false;

                        foreach (OrderContext ordercontext in this.Relationships("v_Order Context"))
                        {
                            if (ordercontext.Related.Equals(variantcontext))
                            {
                                exists = true;
                                break;
                            }
                        }

                        if (!exists)
                        {
                            this.Relationships("v_Order Context").Create(variantcontext, this.Transaction);
                        }
                    }

                    // Evaluate any Method Variant Contexts
                    foreach (OrderContext ordercontext in this.Relationships("v_Order Context"))
                    {
                        if (ordercontext.VariantContext.IsMethod && ordercontext.VariantContext.Method != null)
                        {
                            Model.IO.Item dborder = new Model.IO.Item(this.ItemType.Name, ordercontext.VariantContext.Method);
                            dborder.ID = this.ID;

                            // Add this Order Context
                            Model.IO.Item dbordercontext = ordercontext.GetIOItem();
                            dborder.AddRelationship(dbordercontext);

                            // Add all other order Context
                            foreach (OrderContext otherordercontext in this.Relationships("v_Order Context"))
                            {
                                if (!otherordercontext.Equals(ordercontext))
                                {
                                    Model.IO.Item dbotherordercontext = otherordercontext.GetIOItem();
                                    dborder.AddRelationship(dbotherordercontext);
                                }
                            }

                            Model.IO.SOAPRequest request = new Model.IO.SOAPRequest(Model.IO.SOAPOperation.ApplyItem, this.ItemType.Session, dborder);
                            Model.IO.SOAPResponse response = request.Execute();

                            if (!response.IsError)
                            {
                                if (response.Items.Count() == 1)
                                {
                                    ordercontext.Value = response.Items.First().GetProperty("value");
                                    ordercontext.Quantity = Double.Parse(response.Items.First().GetProperty("quantity"));
                                }
                                else
                                {
                                    ordercontext.Value = "0";
                                    ordercontext.Quantity = 0.0;
                                }
                            }
                            else
                            {
                                throw new Model.Exceptions.ServerException(response);
                            }
                        }
                    }

                    // Build Flat BOM
                    Dictionary<Part, Double> flatbom = new Dictionary<Part, Double>();
                    flatbom[this.Part] = 1.0;

                    foreach(PartBOM partbom in this.AllConfiguredPartBOM(this, this.Part))
                    {

                        if (partbom.Related != null)
                        {
                            if (flatbom.ContainsKey((Part)partbom.Related))
                            {
                                flatbom[(Part)partbom.Related] = flatbom[(Part)partbom.Related] + partbom.Quantity;
                            }
                            else
                            {
                                flatbom[(Part)partbom.Related] = partbom.Quantity;
                            }
                        }
                    }

                    // Remove any Part BOM no longer required in Configured Part
                    foreach (PartBOM partbom in this.ConfiguredPart.Relationships("Part BOM"))
                    {
                        if ((partbom.Related != null) && !flatbom.ContainsKey((Part)partbom.Related))
                        {
                            partbom.Delete(this.Transaction);
                        }
                    }

                    // Add any Part BOM that no current in Configured Part

                    foreach(Part flatpart in flatbom.Keys)
                    {
                        Boolean found = false;

                        foreach (PartBOM partbom in this.ConfiguredPart.Relationships("Part BOM"))
                        {
                            if ((partbom.Related != null) && partbom.Related.Equals(flatpart))
                            {
                                found = true;

                                if (partbom.Status == States.Deleted)
                                {
                                    partbom.Update(this.Transaction);
                                    partbom.Quantity = flatbom[flatpart];
                                }

                                break;
                            }
                        }

                        if (!found)
                        {
                            PartBOM newpartbom = (PartBOM)this.ConfiguredPart.Relationships("Part BOM").Create(flatpart, this.Transaction);
                            newpartbom.Quantity = flatbom[flatpart];
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
