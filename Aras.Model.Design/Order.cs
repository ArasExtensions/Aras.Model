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

        private Stores.Relationship<OrderContext> _orderContexts;
        public Stores.Relationship<OrderContext> OrderContexts
        {
            get
            {
                if (this._orderContexts == null)
                {
                    this._orderContexts = new Stores.Relationship<OrderContext>(this, "v_Order Context");
                }

                return this._orderContexts;
            }
        }

        private void RunMethodVariantContext(VariantContext VariantContext)
        {
            if (VariantContext.IsMethod)
            {
                if (VariantContext.Method != null)
                {
                    Model.IO.Item dborder = new Model.IO.Item(this.ItemType.Name, VariantContext.Method);
                    dborder.ID = this.ID;

                    // Add this Order Context
                    Model.IO.Item dbordercontext = this.OrderContexts.FirstByRelated(VariantContext).GetIOItem();
                    dborder.AddRelationship(dbordercontext);

                    // Add all other order Context
                    foreach (OrderContext otherordercontext in this.OrderContexts)
                    {
                        if (!otherordercontext.Equals(this.OrderContexts.FirstByRelated(VariantContext)))
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
                            this.OrderContexts.FirstByRelated(VariantContext).Value = response.Items.First().GetProperty("value");
                            this.OrderContexts.FirstByRelated(VariantContext).Quantity = Double.Parse(response.Items.First().GetProperty("quantity"));
                        }
                        else
                        {
                            this.OrderContexts.FirstByRelated(VariantContext).Value = "0";
                            this.OrderContexts.FirstByRelated(VariantContext).Quantity = 0.0;
                        }
                    }
                    else
                    {
                        throw new Model.Exceptions.ServerException(response);
                    }
                }
                else
                {
                    this.OrderContexts.FirstByRelated(VariantContext).Value = "0";
                    this.OrderContexts.FirstByRelated(VariantContext).Quantity = 0.0;
                }
            }
        }

        internal OrderContext OrderContext(VariantContext VariantContext)
        {
            if (this.Transaction != null)
            {
                OrderContext ordercontext = this.OrderContexts.FirstByRelated(VariantContext);

                if (ordercontext == null)
                {
                    // Create new Variant Context
                    ordercontext = this.OrderContexts.Create(VariantContext, this.Transaction);

                    // Default Value to first value in List
                    ordercontext.Value = ordercontext.ValueList.Values.Values.First().Value;

                    // Default Quantity to 1.0
                    ordercontext.Quantity = 1.0;
                }
                else
                {
                    if (ordercontext.Value == null)
                    {
                        // Default Value to first value in List
                        ordercontext.Value = ordercontext.ValueList.Values.Values.First().Value;

                        // Default Quantity to 1.0
                        ordercontext.Quantity = 1.0;
                    }
                }

                // Run Method Variant Context
                this.RunMethodVariantContext(VariantContext);

                return ordercontext;
            }
            else
            {
                throw new Exceptions.ReadOnlyException();
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            
            // Update Order Contexts
            this.OrderContexts.Refresh();

            foreach (OrderContext ordercontext in this.OrderContexts)
            {
                ordercontext.Update(this.Transaction, true);
            }

            // Update Configured Part
            if (this.ConfiguredPart == null)
            {
                Stores.Item<Part> partsstore = new Stores.Item<Part>(this.Session, "Part", Aras.Conditions.Eq("item_number", this.ItemNumber));

                if (partsstore.Count() == 0)
                {
                    // Create Part
                    this.ConfiguredPart = (Part)partsstore.Create(this.Transaction);
                    this.ConfiguredPart.ItemNumber = this.ItemNumber;
                }
                else
                {
                    this.ConfiguredPart = (Part)partsstore.First();
                    this.ConfiguredPart.Update(this.Transaction);
                }
            }
            else
            {
                this.ConfiguredPart.Update(this.Transaction, true);
            }
        }

        private Dictionary<Part, Double> AllConfiguredParts(Part Part, Double Quantity)
        {
            Dictionary<Part, Double> ret = new Dictionary<Part, Double>();

            if (ret.ContainsKey(Part))
            {
                ret[Part] += Quantity;
            }
            else
            {
                ret[Part] = Quantity;
            }

            foreach (PartBOM partbom in Part.ConfiguredPartBOM(this))
            {
                if (partbom.Related != null)
                {
                    Dictionary<Part, Double> childparts = this.AllConfiguredParts((Part)partbom.Related, partbom.Quantity * Quantity);
                   
                    foreach(Part childpart in childparts.Keys)
                    {
                        if (ret.ContainsKey(childpart))
                        {
                            ret[childpart] += childparts[childpart];
                        }
                        else
                        {
                            ret[childpart] = childparts[childpart];
                        }
                    }
                }
            }

            return ret;
        }

        private Boolean UpdatingBOM = false;

        public void UpdateBOM()
        {
            if (!this.UpdatingBOM)
            {
                this.UpdatingBOM = true;

                if (this.Transaction != null)
                {
                    this.Refresh();

                    if (this.Part != null)
                    {
                        // Update Properties of Configured Part
                        this.ConfiguredPart.Class = this.ConfiguredPart.ItemType.GetClassName("TopLevel");
                        this.ConfiguredPart.Property("name").Value = this.Property("name").Value;
                        this.ConfiguredPart.Property("description").Value = this.Property("description").Value;

                        // Build Flat BOM
                        Dictionary<Part, Double> flatbom = this.AllConfiguredParts(this.Part, 1.0);

                        // Remove any Parts that are not class BOM
                        List<Part> partstoremove = new List<Part>();

                        foreach (Part part in flatbom.Keys)
                        {
                            if ((part.Class == null) || (part.Class.Name != "BOM"))
                            {
                                partstoremove.Add(part);
                            }
                        }

                        foreach (Part part in partstoremove)
                        {
                            flatbom.Remove(part);
                        }

                        // Remove any Part BOM no longer required in Configured Part
                        foreach (PartBOM partbom in this.ConfiguredPart.PartBOMS)
                        {
                            if ((partbom.Related != null) && !flatbom.ContainsKey((Part)partbom.Related))
                            {
                                partbom.Delete(this.Transaction, true);
                            }
                        }

                        // Add any Part BOM that not current in Configured Part
                        foreach (Part flatpart in flatbom.Keys)
                        {
                            Boolean found = false;

                            foreach (PartBOM partbom in this.ConfiguredPart.PartBOMS)
                            {
                                if ((partbom.Related != null) && partbom.Related.Equals(flatpart))
                                {
                                    found = true;

                                    //Update
                                    partbom.Update(this.Transaction, true);

                                    // Update Quantity
                                    partbom.Quantity = flatbom[flatpart];

                                    break;
                                }
                            }

                            if (!found)
                            {
                                PartBOM newpartbom = this.ConfiguredPart.PartBOMS.Create(flatpart, this.Transaction);
                                newpartbom.Quantity = flatbom[flatpart];
                            }
                        }
                    }
                }

                this.OnPropertyChanged("ConfiguredPart");

                this.UpdatingBOM = false;
            }

        }

        protected override void OnRefresh()
        {
            base.OnRefresh();

            // Refresh Order Contexts
            if (this._orderContexts != null)
            {
                this._orderContexts.Refresh();
            }
        }

        private void Initialiase()
        {
            this.UpdatingBOM = false;

            // Ensure Required Properties are Selected
            this.Session.ItemType("v_Order Context").AddToSelect("quantity,value,locked_by_id");
            this.Session.ItemType("Variant Context").AddToSelect("context_type,min_quantity,max_quantity,sort_order");
            this.Session.ItemType("Part").AddToSelect("item_number,locked_by_id");
            this.Session.ItemType("Part Variants").AddToSelect("quantity");
            this.Session.ItemType("Part BOM").AddToSelect("quantity,locked_by_id");
            this.Session.ItemType("User").AddToSelect("keyed_name");
        }

        public Order(Model.ItemType ItemType)
            : base(ItemType)
        {
            this.Initialiase();
        }

        public Order(Model.ItemType ItemType, IO.Item DBItem)
            : base(ItemType, DBItem)
        {
            this.Initialiase();
        }
    }
}
