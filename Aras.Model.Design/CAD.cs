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
    [Model.Attributes.ItemType("CAD")]
    public class CAD : Model.Item
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

        public String Name
        {
            get
            {
                return (String)this.Property("name").Value;
            }
            set
            {
                this.Property("name").Value = value;
            }
        }

        public File NativeFile
        {
            get
            {
                return (File)this.Property("native_file").Value;
            }
            set
            {
                this.Property("native_file").Value = value;
            }
        }

        public File ViewableFile
        {
            get
            {
                return (File)this.Property("viewable_file").Value;
            }
            set
            {
                this.Property("viewable_file").Value = value;
            }
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();


        }

        public CAD(Model.ItemType ItemType)
            : base(ItemType)
        {
 
        }

        public CAD(Model.ItemType ItemType, IO.Item DBItem)
            : base(ItemType, DBItem)
        {

        }
    }
}
