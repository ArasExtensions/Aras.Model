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

namespace Aras.Model.Actions
{
    internal class Relationship : Action
    {
        private IO.Item DBItem;
        internal override IO.Item Commit()
        {
            if (!this.Completed)
            {
                this.DBItem = this.BuildItem();

                if (((Model.Relationship)this.Item).Related != null)
                {
                    Action relatedaction = this.Transaction.Get(((Model.Relationship)this.Item).Related.ID);

                    if (relatedaction != null)
                    {
                        this.DBItem.SetProperty("related_id", relatedaction.Commit().ID);
                    }
                    else
                    {
                        this.DBItem.SetProperty("related_id", ((Model.Relationship)this.Item).Related.ID);
                    }
                }

                // Watch for Source Item Versioning
                ((Model.Relationship)this.Item).Source.Superceded += Source_Superceded;

                this.Completed = true;
            }

            return this.DBItem;
        }

        void Source_Superceded(object sender, SupercededEventArgs e)
        {
            ((Model.Relationship)this.Item).Source.Superceded -= Source_Superceded;
            ((Model.Relationship)this.Item).Source = e.NewGeneration;
        }

        internal override void Rollback()
        {
            if (!this.Completed)
            {
                switch(this.Item.Action)
                {
                    case Model.Item.Actions.Create:

                        // Remove from Parent Cache
                        //((Model.Relationship)this.Item).Source.Store(((Model.Relationship)this.Item).RelationshipType).Delete((Model.Relationship)this.Item);

                        break;
         
                    case Model.Item.Actions.Update:
                    case Model.Item.Actions.Delete:

                        // Unlock
                        this.Item.UnLock();

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
            }
        }

        internal Relationship(Transaction Transaction, String Name, Model.Relationship Relationship)
            : base(Transaction, Name, Relationship)
        {
            this.DBItem = null;
        }
    }
}
