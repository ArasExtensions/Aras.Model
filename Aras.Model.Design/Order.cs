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

        private Dictionary<VariantContext, OrderContext> _orderContextCache;
        private Dictionary<VariantContext, OrderContext> OrderContextCache
        {
            get
            {
                if (this._orderContextCache == null)
                {
                    this._orderContextCache = new Dictionary<VariantContext, OrderContext>();

                    // Load Order Contexts already in database
                    foreach (OrderContext ordercontext in this.Store("v_Order Context").Query())
                    {
                        if (!this._orderContextCache.ContainsKey(ordercontext.VariantContext))
                        {
                            this._orderContextCache[ordercontext.VariantContext] = ordercontext;
                        }
                    }
                }

                return this._orderContextCache;
            }
        }

        public IEnumerable<OrderContext> OrderContexts
        {
            get
            {
                return this.OrderContextCache.Values;
            }
        }

        internal OrderContext OrderContext(VariantContext VariantContext)
        {
            if (this.Transaction != null)
            {
                if (!this.OrderContextCache.ContainsKey(VariantContext))
                {
                    this.OrderContextCache[VariantContext] = (OrderContext)this.Store("v_Order Context").Create(VariantContext, this.Transaction);

                    // Default Value to first value in List
                    this.OrderContextCache[VariantContext].Value = this.OrderContextCache[VariantContext].ValueList.Values.Values.First().Value;

                    // Default Quantity to 1.0
                    this.OrderContextCache[VariantContext].Quantity = 1.0;

                    // Watch for changes
                    this.OrderContextCache[VariantContext].ValueList.PropertyChanged += ValueList_PropertyChanged;
                    this.OrderContextCache[VariantContext].Property("quantity").PropertyChanged += Quantity_PropertyChanged;
                }
                else
                {
                    if (this.OrderContextCache[VariantContext].Value == null)
                    {
                        // Default Value to first value in List
                        this.OrderContextCache[VariantContext].Value = this.OrderContextCache[VariantContext].ValueList.Values.Values.First().Value;

                        // Default Quantity to 1.0
                        this.OrderContextCache[VariantContext].Quantity = 1.0;
                    }
                }

                if (this.OrderContextCache[VariantContext].VariantContext.IsMethod)
                {
                    if (this.OrderContextCache[VariantContext].VariantContext.Method != null)
                    {
                        Model.IO.Item dborder = new Model.IO.Item(this.ItemType.Name, this.OrderContextCache[VariantContext].VariantContext.Method);
                        dborder.ID = this.ID;

                        // Add this Order Context
                        Model.IO.Item dbordercontext = this.OrderContextCache[VariantContext].GetIOItem();
                        dborder.AddRelationship(dbordercontext);

                        // Add all other order Context
                        foreach (OrderContext otherordercontext in this.OrderContexts)
                        {
                            if (!otherordercontext.Equals(this.OrderContextCache[VariantContext]))
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
                                this.OrderContextCache[VariantContext].Value = response.Items.First().GetProperty("value");
                                this.OrderContextCache[VariantContext].Quantity = Double.Parse(response.Items.First().GetProperty("quantity"));
                            }
                            else
                            {
                                this.OrderContextCache[VariantContext].Value = "0";
                                this.OrderContextCache[VariantContext].Quantity = 0.0;
                            }
                        }
                        else
                        {
                            throw new Model.Exceptions.ServerException(response);
                        }
                    }
                    else
                    {
                        this.OrderContextCache[VariantContext].Value = "0";
                        this.OrderContextCache[VariantContext].Quantity = 0.0;
                    }


                }

                return this.OrderContextCache[VariantContext];
            }
            else
            {
                throw new Exceptions.ReadOnlyException();
            }
        }

        void Quantity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                this.Process();
            }
        }

        void ValueList_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                this.Process();
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            
            // Update Order Contexts
            foreach (OrderContext ordercontext in this.OrderContextCache.Values)
            {
                ordercontext.Update(this.Transaction, true);

                // Watch for changes
                ordercontext.ValueList.PropertyChanged += ValueList_PropertyChanged;
                ordercontext.Property("quantity").PropertyChanged += Quantity_PropertyChanged;
            }

            // Update Configured Part
            if (this.ConfiguredPart == null)
            {
                IEnumerable<Model.Item> parts = this.ItemType.Session.Store("Part").Query(Aras.Conditions.Eq("item_number", this.ItemNumber));

                if (parts.Count() == 0)
                {
                    // Create Part
                    this.ConfiguredPart = (Part)this.ItemType.Session.Store("Part").Create(this.Transaction);
                    this.ConfiguredPart.ItemNumber = this.ItemNumber;
                }
                else
                {
                    this.ConfiguredPart = (Part)parts.First();
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

        private Boolean Processing = false;
        private void Process()
        {
            if (!this.Processing)
            {
                this.Processing = true;


                if (this.Transaction != null)
                {
                    // Update Properties of Configured Part
                    this.ConfiguredPart.Class = this.ConfiguredPart.ItemType.GetClassName("NoTemplate");
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
                    foreach (PartBOM partbom in this.ConfiguredPart.Store("Part BOM").Copy())
                    {
                        if ((partbom.Related != null) && !flatbom.ContainsKey((Part)partbom.Related))
                        {
                            partbom.Delete(this.Transaction);
                        }
                    }

                    // Add any Part BOM that not current in Configured Part
                    foreach (Part flatpart in flatbom.Keys)
                    {
                        Boolean found = false;

                        foreach (PartBOM partbom in this.ConfiguredPart.Store("Part BOM").Copy())
                        {
                            if ((partbom.Related != null) && partbom.Related.Equals(flatpart))
                            {
                                found = true;

                                //Update
                                partbom.Update(this.Transaction);
                            
                                // Update Quantity
                                partbom.Quantity = flatbom[flatpart];

                                break;
                            }
                        }

                        if (!found)
                        {
                            PartBOM newpartbom = (PartBOM)this.ConfiguredPart.Store("Part BOM").Create(flatpart, this.Transaction);
                            newpartbom.Quantity = flatbom[flatpart];
                        }
                    }
                }

                this.OnPropertyChanged("ConfiguredPart");

                this.Processing = false;
            }

        }

        protected override void OnRefresh()
        {
            base.OnRefresh();
            this.Process();
        }

        public Order(Model.ItemType ItemType)
            : base(ItemType)
        {
            this.Processing = false;
        }

        public Order(Model.ItemType ItemType, IO.Item DBItem)
            : base(ItemType, DBItem)
        {
            this.Processing = false;
        }
    }
}
