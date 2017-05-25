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

namespace Aras.Model
{
    public class File : Item
    {
        const int bufferlength = 4096;

        public String CheckedOutPath
        {
            get
            {
                return (String)this.Property("checkedout_path").Value;
            }
            set
            {
                this.Property("checkedout_path").Value = value;
            }
        }

        public String Checksum
        {
            get
            {
                return (String)this.Property("checksum").Value;
            }
        }

        public String Filename
        {
            get
            {
                return (String)this.Property("filename").Value;
            }
        }

        public Int32 FileSize
        {
            get
            {
                return (Int32)this.Property("file_size").Value;
            }
        }

        public Items.FileType FileType
        {
            get
            {
                return (Items.FileType)this.Property("file_type").Value;
            }
            set
            {
                this.Property("file_type").Value = value;
            }
        }

        /*
        private Vault _userVault;
        private Vault UserVault
        {
            get
            {
                if (this._userVault == null)
                {
                    foreach (Located located in this.Store("Located"))
                    {
                        if (this.ItemType.Session.User.Vault.Equals(located.Vault))
                        {
                            this._userVault = located.Vault;
                            break;
                        }
                    }

                    if (this._userVault == null)
                    {
                        throw new Exceptions.ServerException("File is not located in Users Vault");
                    }
                }

                return this._userVault;
            }
        }
        */

        public void Read(Stream Output)
        {
            this.ItemType.Session.IO.VaultRead(this.ID, this.VaultFilename, Output);
        }

        private FileInfo _cacheFilename;
        internal FileInfo CacheFilename
        {
            get
            {
                if (this._cacheFilename == null)
                {
                    this._cacheFilename = new FileInfo(this.ItemType.Session.CacheDirectory.FullName + "\\" + this.ID + ".dat");
                }

                return this._cacheFilename;
            }
        }

        internal String VaultFilename { get; private set; }

        public void Write(Stream Input, String Filename)
        {
            byte[] buffer = new byte[bufferlength];
            int read = 0;

            using (FileStream cache = new FileStream(this.CacheFilename.FullName, FileMode.Create))
            {
                while ((read = Input.Read(buffer, 0, bufferlength)) > 0)
                {
                    cache.Write(buffer, 0, read);
                }
            }

            // Store Filename
            this.VaultFilename = Path.GetFileName(Filename);
        }

        public File(Store Store)
            : base(Store)
        {

        }

        public File(Store Store, IO.Item DBItem)
            : base(Store, DBItem)
        {

        }
    }
}
