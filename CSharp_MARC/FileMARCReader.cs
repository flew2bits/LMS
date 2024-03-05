﻿/**
 * Parser for MARC records
 *
 * This project is based on the File_MARC package
 * (http://pear.php.net/package/File_MARC) by Dan Scott , which was based on PHP
 * MARC package, originally called "php-marc", that is part of the Emilda
 * Project (http://www.emilda.org). Both projects were released under the LGPL
 * which allowed me to port the project to C# for use with the .NET Framework.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * @author    Mattie Schraeder <mattie@csharpmarc.net>
 * @copyright 2009-2022 Mattie Schraeder and Bound to Stay Bound Books <http://www.btsb.com>
 * @license   http://www.gnu.org/copyleft/lesser.html  LGPL License 3
 */


using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace MARC
{
	/// <summary>
	/// This is a wrapper for FileMARC that allows for reading large files without loading the entire file into memory.
	/// </summary>
	public class FileMARCReader : IEnumerable, IDisposable, IEnumerable<Record>
	{
		//Member Variables and Properties
		#region Member Variables and Properties

		private string filename = null;
        private readonly Stream reader = null;
        private readonly bool forceUTF8 = false;

        private readonly string byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

		#endregion

		//Constructors
		#region Constructors

		public FileMARCReader(string filename)
		{
			this.filename = filename;
			reader = new FileStream(this.filename, FileMode.Open, FileAccess.Read, FileShare.Read);
		}

        public FileMARCReader(string filename, bool forceUTF8)
        {
            this.forceUTF8 = forceUTF8;
            this.filename = filename;
            reader = new FileStream(this.filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public FileMARCReader(Stream stream)
        {
	        reader = stream;
        }

		#endregion

		//Interface functions
		#region IEnumerator Members

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns></returns>
		public IEnumerator<Record> GetEnumerator()
		{
			var bufferSize = 10 * 1024 * 1024; // Read 10 MB at a time
			if (bufferSize > reader.Length)
				bufferSize = Convert.ToInt32(reader.Length);

			while (reader.Position < reader.Length)
            {
                var byteArray = new byte[bufferSize];
                int delPosition, realReadSize;

				do
				{
					realReadSize = reader.Read(byteArray, 0, bufferSize);

					if (realReadSize != bufferSize)
						Array.Resize(ref byteArray, realReadSize);

					delPosition = Array.LastIndexOf(byteArray, Convert.ToByte(FileMARC.END_OF_RECORD)) + 1;

					if (!(delPosition == 0 & realReadSize == bufferSize)) continue;
					
					bufferSize *= 2;
					byteArray = new byte[bufferSize];
				} while (delPosition == 0 & realReadSize == bufferSize);

				//Some files will have trailer characters, usually a hex code 1A. 
				//The record has to at least be longer than the leader length of a MARC record, so it's a good place to make sure we have enough to at least try and make a record
				//Otherwise we will relying error checking in the FileMARC class
				if (byteArray.Length > FileMARC.LEADER_LEN)
                {
                    string encoded;

                    reader.Position = reader.Position - (realReadSize - delPosition);
					char marc8utf8Flag = Convert.ToChar(byteArray[9]);

                    if (marc8utf8Flag == ' ' && !forceUTF8)
                    {
                        Encoding encoding = new MARC8();
                        encoded = encoding.GetString(byteArray, 0, delPosition);
                    }
                    else
                    {
                        encoded = Encoding.UTF8.GetString(byteArray, 0, delPosition);

                        if (encoded.StartsWith(byteOrderMarkUtf8))
                            encoded = encoded.Remove(0, byteOrderMarkUtf8.Length); //remove UTF8 Byte Order Mark
                    }

					FileMARC marc = new FileMARC(encoded);
					
					if (forceUTF8)
						marc.ForceUTF8 = true;

					foreach (Record marcRecord in marc)
					{
						yield return marcRecord;
					}
				}
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			reader.Dispose();
		}

		#endregion
	}
}