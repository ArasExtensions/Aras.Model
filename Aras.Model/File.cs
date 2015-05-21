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
using System.IO;

namespace Aras.Model
{
    public class File : Item
    {
        public String Filename
        {
            get
            {
                return ((Properties.String)this["filename"]).Value;
            }
            set
            {
                ((Properties.String)this["filename"]).Value = value;
            }
        }

        private FileInfo AttachedFile { get; set; }

        public void Checkin(FileInfo Filename)
        {
            this.AttachedFile = Filename;
            this.Filename = Filename.Name;
        }

        public FileInfo Checkout()
        {
            IO.Item file = new IO.Item(this.ItemType.Name, "get");
            file.ID = this.ID;

            IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Session, file);
            IO.SOAPResponse response = request.Execute();

            if (this.Session.Workspace != null)
            {
                throw new NotImplementedException();
                /*Aras.IOM.Item checkoutresponse = response.checkout(this.Session.Workspace.FullName);

                if (!checkoutresponse.isError())
                {
                    return new FileInfo(this.Session.Workspace.FullName + "\\" + this.Filename);
                }
                else
                {
                    throw new Exceptions.ServerException("Failed to Checkout File");
                }*/
            }
            else
            {
                throw new Exceptions.ArgumentException("Session workspace not set");
            }

        }

        internal File(ItemType ItemType)
            : base(ItemType)
        {

        }

    }
}
