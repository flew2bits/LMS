/**
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MARC
{
    /// <summary>
    /// The MARC DataField class represents a single field in a MARC record.
    /// A MARC data field consists of a tag name, two indicators which may be
    /// null, and zero or more subfields represented by MARC Subfield objects.
    /// Subfields are held within a List structure.
    /// </summary>
    public class DataField : Field
    {
        //Private member variables and properties
        #region Private member variables and properties

        private char ind1;
        private char ind2;

        /// <summary>
        /// Gets or sets the first indicator.
        /// </summary>
        /// <value>The first indicator.</value>
        public char Indicator1
        {
            get => ind1;
            set
            {
                if (ValidateIndicator(value))
                    ind1 = value;
                else
                    throw new ArgumentException("Invalid indicator.");
            }
        }

        /// <summary>
        /// Gets or sets the second indicator.
        /// </summary>
        /// <value>The second indicator.</value>
        public char Indicator2
        {
            get => ind2;
            set
            {
                if (ValidateIndicator(value))
                    ind2 = value;
                else
                    throw new ArgumentException("Invalid indicator.");
            }
        }

        /// <summary>
        /// Gets or sets the subfields.
        /// </summary>
        /// <value>The subfields.</value>
        public List<Subfield> Subfields { get; set; }

        /// <summary>
        /// Gets the first <see cref="MARC.Subfield"/> with the specified code.
        /// </summary>
        /// <value>The first matching subfield or null if not found</value>
        public Subfield this[char code] => Subfields.FirstOrDefault(s => s.Code == code);

        #endregion

        //Constructors
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DataField"/> class.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="subfields">The subfields.</param>
        /// <param name="ind1">The first indicator.</param>
        /// <param name="ind2">The second indicator.</param>
        public DataField(string tag, List<Subfield> subfields, char ind1, char ind2) : base(tag)
        {
            this.Subfields = subfields;

            if (ValidateIndicator(ind1))
                this.ind1 = ind1;
            else
                throw new ArgumentException("Invalid indicator.");

            if (ValidateIndicator(ind2))
                this.ind2 = ind2;
            else
                throw new ArgumentException("Invalid indicator.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataField"/> class.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="subfields">The subfields.</param>
        /// <param name="ind1">The first indicator.</param>
        public DataField(string tag, List<Subfield> subfields, char ind1) : this(tag, subfields, ind1, ' ') { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataField"/> class.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="subfields">The subfields.</param>
        public DataField(string tag, List<Subfield> subfields) : this(tag, subfields, ' ') { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataField"/> class.
        /// </summary>
        /// <param name="tag">The tag.</param>
        public DataField(string tag) : this(tag, new List<Subfield>()) { }

        #endregion

        /// <summary>
        /// Returns a List of subfield objects that match a requested code,
        /// or a cloned List that contains all the subfield objects if the
        /// requested code is null.
        /// </summary>
        /// <param name="code">The code, or null.</param>
        /// <returns></returns>
        public IEnumerable<Subfield> GetSubfields(char? code = null)
            => code == null ? Subfields : Subfields.Where(s => s.Code == code);
        

        /// <summary>
        /// Inserts the subfield into position before the first subfield found with a higher code.
        /// Numbers always get sorted after letters
        /// This assumes the subfield has already been sorted.
        /// </summary>
        /// <param name="newSubfield">The new subfield.</param>
        public void InsertSubfield(Subfield newSubfield)
		{
			int rowNum = 0;
			foreach (Subfield subfield in Subfields)
			{
				int x;
				if (!Int32.TryParse(subfield.Code.ToString(), out x) && !Int32.TryParse(newSubfield.Code.ToString(), out x) && subfield.Code.CompareTo(newSubfield.Code) > 0)
				{
					Subfields.Insert(rowNum, newSubfield);
					return;
				}
				else if (Int32.TryParse(subfield.Code.ToString(), out x) && !Int32.TryParse(newSubfield.Code.ToString(), out x))
				{
					Subfields.Insert(rowNum, newSubfield);
					return;
				}
				else if (Int32.TryParse(subfield.Code.ToString(), out x) && subfield.Code.CompareTo(newSubfield.Code) > 0)
				{
					Subfields.Insert(rowNum, newSubfield);
					return;
				}

				rowNum++;
			}

			//Insert at the end
			Subfields.Add(newSubfield);
		}

        /// <summary>
        /// Determines whether this instance is empty.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsEmpty() => Subfields == null || Subfields.Count == 0;

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            var lines = new StringBuilder();
            var prefix = tag.PadLeft(3) + " " + ind1 + ind2 + " ";

            foreach (var subfield in Subfields)
            {
                if (lines.Length != 0)
                    lines.Append(Environment.NewLine).Append("       ");
                else
                    lines.Append(prefix);

                lines.Append(subfield.ToString());
            }

            return lines.ToString();
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>
        /// in raw USMARC format.
        /// </summary>
        /// <returns></returns>
        public override string ToRaw()
        {
            string raw = string.Empty;

            foreach (Subfield subfield in Subfields)
            {
                if (!subfield.IsEmpty())
                    raw += subfield.ToRaw();
            }

            return ind1.ToString() + ind2.ToString() + raw + FileMARC.END_OF_FIELD.ToString();
        }



        /// <summary>
        /// Print a MARC Field object without tags, indicators, etc
        /// </summary>
        /// <returns></returns>
        public string FormatField(params char[] excludeCodes)
        {
            var result = new StringBuilder();

            foreach (var subfield in Subfields)
            {
                if (Tag.Substring(0, 1) == "6" && (subfield.Code == 'v' || subfield.Code == 'x' || subfield.Code == 'y' || subfield.Code == 'v' || subfield.Code == 'z'))
                    result.Append(" -- " + subfield.Data);
                else
                {
                    var exclude = excludeCodes.Any(code => subfield.Code == code);

                    if (!exclude)
                        result.Append(" " + subfield.Data);
                }
            }

            return result.ToString().Trim();
        }

        public override string FormatField() => FormatField(); 

        /// <summary>
        /// Validates the indicator.
        /// </summary>
        /// <param name="ind">The indicator</param>
        /// <returns>
        ///     <c>true</c> if the indicator is valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool ValidateIndicator(char ind)
        {
            var match = Regex.Match(ind.ToString(), "^[0-9a-z]{1}$");
            return (match.Captures.Count > 0 || ind == ' ');
        }

		/// <summary>
		/// Makes a deep clone of this instance.
		/// </summary>
		/// <returns></returns>
		public override Field Clone() => new DataField(tag, Subfields.Select(s => s.Clone()).ToList(), ind1, ind2);
    }
}
