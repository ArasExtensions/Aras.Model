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
using System.Net;
using System.Net.Http;
using System.IO;
using System.Xml;

namespace Aras.Model.Actions
{
    internal class File : Action
    {
        private IO.Item Result;
        
        internal override IO.Item Commit()
        {
            if (!this.Completed)
            {
                // Write File to Cache
                using (FileStream cachefile = new FileStream(((Model.File)this.Item).CacheFilename.FullName, FileMode.Open))
                {
                    IO.Response response = this.Transaction.Session.IO.VaultWrite(cachefile, ((Model.File)this.Item).VaultFilename);

                    if (!response.IsError)
                    {
                        if (response.Items.Count() == 1)
                        {
                            this.Result = response.Items.First();

                            if (this.Result.ConfigID.Equals(this.Item.ConfigID))
                            {
                                if (!this.Result.ID.Equals(this.Item.ID))
                                {
                                    // New Version of Item
                                    //Model.Item newversion = this.Item.Session.Store(this.Item.ItemType).Get(this.Result);
                                    //Model.Item oldversion = this.Item;
                                    //this.Item = newversion;
                                    //this.UpdateItem(this.Result);
                                    //oldversion.OnSuperceded(newversion);
                                }
                                else
                                {
                                    this.UpdateItem(this.Result);
                                }

                                this.Item.UpdateProperties(this.Result);
                            }
                            else
                            {
                                // Result does not match Item
                                throw new Exceptions.ServerException("Server response does not match original Item");
                            }
                        }
                    }

                }

                this.Completed = true;
            }

            return this.Result;
        }

        internal override void Rollback()
        {
            if (!this.Completed)
            {
                switch (this.Item.Action)
                {
                    case Model.Item.Actions.Create:

                        // Remove from Cache
                        //this.Item.Session.Store(this.Item.ItemType).Delete(this.Item);

                        break;
             
                    default:
                        break;
                }

                this.Completed = true;
            }
        }

        internal override void UpdateStore()
        {
            if (this.Item.Action == Model.Item.Actions.Delete)
            {
                // Trigger Deleted Event
                this.Item.OnDeleted();

                // Remove from Cache
                //this.Item.Session.Store(this.Item.ItemType).Delete(this.Item);
            }
        }

        internal File(Transaction Transaction, String Name, Model.Item Item)
            : base(Transaction, Name, Item)
        {
            this.Result = null;
        }
    }
}
