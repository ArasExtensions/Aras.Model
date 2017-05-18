/*  
  Copyright 2017 Processwall Limited

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
  Web:     http://www.processwall.com
  Email:   support@processwall.com
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Aras.Model.Items
{
    [Attributes.ItemType("Vault")]
    public class Vault : Item
    {

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

        public String URL
        {
            get
            {
                return (String)this.Property("vault_url").Value;
            }
            set
            {
                this.Property("vault_url").Value = value;
            }
        }

        public Vault(Store Store, Transaction Transaction)
            : base(Store, Transaction)
        {

        }

        public Vault(Store Store, IO.Item DBItem)
            : base(Store, DBItem)
        {

        }
    }
}
