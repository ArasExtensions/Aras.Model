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
    [Model.Attributes.ItemType("Part Variants")]
    public class PartVariant : Model.Relationship
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

        private List<PartVariantRule> _partVariantRules;
        public IEnumerable<PartVariantRule> PartVariantRules
        {
            get
            {
                if (this._partVariantRules == null)
                {
                    this._partVariantRules = new List<PartVariantRule>();

                    Queries.Relationship pvrquery = (Queries.Relationship)this.Store("Part Variant Rule").Query();
                    pvrquery.Refresh();

                    foreach(PartVariantRule pvr in pvrquery)
                    {
                        this._partVariantRules.Add(pvr);
                    }
                }

                return this._partVariantRules;
            }
        }

        private Dictionary<Order, PartBOM> _configuredPartBOM;
        public PartBOM ConfiguredPartBOM(Order Order, Part Source)
        {
            OrderContext ordercontext = null;

            foreach (PartVariantRule partvariantrule in this.PartVariantRules)
            {
                ordercontext = partvariantrule.Selected(Order);

                if (ordercontext == null)
                {
                    break;
                }
            }

            if (ordercontext != null)
            {
                if (!this._configuredPartBOM.ContainsKey(Order))
                {
                    this._configuredPartBOM[Order] = (PartBOM)Source.Store("Part BOM").Create(this.Related);
                }

                // Update Properties
                this._configuredPartBOM[Order].Quantity = this.Quantity * ordercontext.Quantity;

                return this._configuredPartBOM[Order];
            }
            else
            {
                return null;
            }
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();
        
            // Reset PartVariantRules
            this._partVariantRules = null;
        }

        public PartVariant(Model.RelationshipType RelationshipType, Model.Item Source, Model.Item Related)
            : base(RelationshipType, Source, Related)
        {
            this._configuredPartBOM = new Dictionary<Order, PartBOM>();
        }

        public PartVariant(Model.RelationshipType RelationshipType, Model.Item Source, IO.Item DBItem)
            : base(RelationshipType, Source, DBItem)
        {
            this._configuredPartBOM = new Dictionary<Order, PartBOM>();
        }
    }
}
