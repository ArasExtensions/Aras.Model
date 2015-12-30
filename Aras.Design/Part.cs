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

        private List<PartBOM> _partBOM;
        public IEnumerable<PartBOM> PartBOM
        {
            get
            {
                if (this._partBOM == null)
                {
                    this._partBOM = new List<PartBOM>();
   
                    foreach(PartBOM partbom in this.Relationships("Part BOM", "quantity"))
                    {
                        this._partBOM.Add(partbom);
                    }
                }

                return this._partBOM;
            }
        }

        private List<PartVariant> _partVariant;
        public IEnumerable<PartVariant> PartVariant
        {
            get
            {
                if (this._partVariant == null)
                {
                    this._partVariant = new List<PartVariant>();

                    foreach (PartVariant partvariant in this.Relationships("Part Variants", "quantity"))
                    {
                        this._partVariant.Add(partvariant);
                    }
                }

                return this._partVariant;
            }
        }

        public IEnumerable<PartBOM> ConfiguredPartBOM(Order Order)
        {
            return null;
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();
            this._partBOM = null;
            this._partVariant = null;
        }

        public Part(String ID, Model.ItemType Type)
            :base(ID, Type)
        {

        }
    }
}
