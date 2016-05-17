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
using Aras.Model;
using System.IO;

namespace Aras.Model.Design.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            // Connect to Server
            Model.Server server = new Model.Server("http://192.168.1.23/11SP1");
            server.ProxyURL = "http://127.0.0.1:8888";
            server.LoadAssembly("Aras.Model.Design");
            server.LoadAssembly("Aras.ViewModel.Design");
            Model.Database database = server.Database("Development11SP1");
            Model.Session session = database.Login("admin", Model.Server.PasswordHash("innovator"));
            session.ItemType("CAD").AddToSelect("native_file,viewable_file");


            Model.Stores.Item<Model.Design.CAD> store = new Model.Stores.Item<Model.Design.CAD>(session, "CAD", Aras.Conditions.Eq("item_number", "123456"));
            Model.Design.CAD cad = store.First();
            Model.File viewable = cad.ViewableFile;

            using (FileStream outfile = new FileStream("c:\\temp\\sample1.pdf", FileMode.Create))
            {
                viewable.Read(outfile);
            }

            using (Transaction trans = session.BeginTransaction())
            {
                cad.Update(trans);
                File newviewable = (File)session.Cache("File").Create(trans);

                using (FileStream infile = new FileStream("c:\\work\\sample2.pdf", FileMode.Open))
                {
                    newviewable.Write(infile);
                }

                cad.ViewableFile = newviewable;

                trans.Commit();
            }
        }
    }
}
