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
    [Model.Attributes.ItemType("Part")]
    public class Part : Model.Item
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

        public Boolean Variant
        {
            get
            {
                if ((this.Class != null) && (this.Class.Name == "Variant"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private static void AddPartBOM(List<PartBOM> List, PartBOM PartBOM)
        {
            Boolean found = false;

            foreach(PartBOM partbom in List)
            {
                if (partbom.Related != null && PartBOM.Related != null && partbom.Related.Equals(PartBOM.Related))
                {
                    found = true;
                    partbom.Quantity = partbom.Quantity + PartBOM.Quantity;
                    break;
                }
            }

            if (!found)
            {
                List.Add(PartBOM);
            }
        }

        public IEnumerable<PartBOM> FlatConfiguredPartBOM(Order Order)
        {
            List<PartBOM> ret = new List<PartBOM>();

            foreach(PartBOM partbom in this.ConfiguredPartBOM(Order))
            {
                AddPartBOM(ret, partbom);

                foreach(PartBOM childpartbom in ((Part)partbom.Related).FlatConfiguredPartBOM(Order))
                {
                    AddPartBOM(ret, childpartbom);
                }
            }

            return ret;
        }

        public IEnumerable<PartBOM> ConfiguredPartBOM(Order Order)
        {
            List<PartBOM> ret = new List<PartBOM>();

            // Add PartBOM
            foreach(PartBOM partbom in this.Relationships("Part BOM"))
            {
                if (!partbom.Runtime)
                {
                    ret.Add(partbom);
                }
            }

            // Add Configured Variants
            foreach (PartVariant partvariant in this.Relationships("Part Variants"))
            {
                PartBOM configurepartbom = partvariant.ConfiguredPartBOM(Order);

                if (configurepartbom != null)
                {
                    ret.Add(configurepartbom);
                }
            }

            return ret;
        }

        public IEnumerable<VariantContext> VariantContext(Order Order)
        {
            List<VariantContext> ret = new List<VariantContext>();

            // Check Variants on this Part
            foreach (PartVariant partvariant in this.Relationships("Part Variants"))
            {
                foreach (PartVariantRule partvariantrule in partvariant.Relationships("Part Variant Rule"))
                {
                    VariantContext variantcontext = (VariantContext)partvariantrule.Related;

                    if (!ret.Contains(variantcontext))
                    {
                        ret.Add(variantcontext);
                    }
                }
            }

            // Check Configured Part BOM
            foreach(PartBOM partbom in this.ConfiguredPartBOM(Order))
            {
                if (partbom.Related != null)
                {
                    foreach (VariantContext variantcontext in ((Part)partbom.Related).VariantContext(Order))
                    {
                        if (!ret.Contains(variantcontext))
                        {
                            ret.Add(variantcontext);
                        }
                    }
                }
            }

            return ret;
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();
        }

        public Part(String ID, Model.ItemType Type)
            :base(ID, Type)
        {
 
        }
    }
}
