using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace djack.RogueSurvivor.Engine
{
    public class CSVField
    {
        #region Fields
        string m_RawString;
        #endregion

        #region Init
        public CSVField(string rawString)
        {
            m_RawString = rawString;
        }
        #endregion

        #region Parsing
        public int ParseInt()
        {
            return int.Parse(m_RawString);
        }

        public float ParseFloat()
        {
            return float.Parse(m_RawString);
        }

        public string ParseText()
        {
            return m_RawString.Trim(new char[] { '"' });
        }

        public bool ParseBool()
        {
            return ParseInt() > 0;
        }
        #endregion
    }

    public class CSVLine
    {
        #region Fields
        CSVField[] m_Fields;
        #endregion

        #region Operators
        public CSVField this[int field]
        {
            get { return m_Fields[field]; }
            set { m_Fields[field] = value; }
        }
        #endregion

        #region Properties
        public int FieldsCount
        {
            get { return m_Fields.Length; }
        }
        #endregion

        #region Init
        public CSVLine(int nbFields)
        {
            m_Fields = new CSVField[nbFields];
        }
        #endregion
    }

    public class CSVTable
    {
        #region Fields
        int m_nbFields;
        List<CSVLine> m_Lines;
        #endregion

        #region Operators
        public CSVField this[int field, int line]
        {
            get { return m_Lines[line][field]; }
        }
        #endregion

        #region Properties
        public IEnumerable<CSVLine> Lines
        {
            get { return m_Lines; }
        }

        public int CountLines
        {
            get { return m_Lines.Count; }
        }
        #endregion

        #region Init
        public CSVTable(int nbFields)
        {
            m_nbFields = nbFields;
            m_Lines = new List<CSVLine>();
        }

        public void AddLine(CSVLine line)
        {
            if (line.FieldsCount != m_nbFields)
                throw new ArgumentException(String.Format("line fields count {0} does not match with table fields count {1}", line.FieldsCount, m_nbFields));

            m_Lines.Add(line);

        }
        #endregion
    }

    public class CSVParser
    {
        #region Fields
        char m_Delimiter;
        #endregion

        #region Properties
        public char Delimiter
        {
            get { return m_Delimiter; }
            set { m_Delimiter = value; }
        }
        #endregion

        #region Init
        public CSVParser()
        {
            m_Delimiter = ',';
        }
        #endregion

        #region Parsing
        public string[] Parse(string line)
        {
            if (line == null)
                return new string[] { };

            // split with delimiter.
            line = line.TrimEnd();
            string[] splitted = line.Split(m_Delimiter);

            // delimite wrongly splitted quoted string that contained a delimiter.
            // glue back quotted strings that were wrongly splitted.
            List<string> fields = new List<string>(splitted);
            int iField = 0;
            do
            {
                // if this fields starts with a quote but doesn't end with a quote, we need to glue it back.
                string f = fields[iField];
                if (f[0] == '"' && f[f.Length - 1] != '"')
                {
                    // find next field that ends with a quote.
                    string quotedField = f;
                    int iGlue = iField + 1;
                    for(;;)
                    {
                        // if end, stop.
                        if (iGlue >= fields.Count)
                            break;

                        // glue to quoted string.
                        string glueMe = fields[iGlue];
                        quotedField += "," + glueMe;

                        // remove.
                        fields.RemoveAt(iGlue);

                        // if ends with quote, break.
                        if (glueMe[glueMe.Length - 1] == '"')
                            break;
                    }

                    // update field with correct quoted string.
                    fields[iField] = quotedField;
                }
                else
                {
                    // good field, keep it and check next one.
                    ++iField;
                }
            }
            while (iField < fields.Count - 1);

            // done.
            return fields.ToArray();
        }

        public List<string[]> Parse(string[] lines)
        {
            List<string[]> list = new List<string[]>(1);

            if (lines == null)
                return list;

            foreach (string l in lines)
                list.Add(Parse(l));

            return list;
        }

        public CSVTable ParseToTable(string[] lines, int nbFields)
        {
            CSVTable table = new CSVTable(nbFields);

            List<string[]> parsedRawLines = Parse(lines);
            foreach (string[] l in parsedRawLines)
            {
                CSVLine newTableLine = new CSVLine(l.Length);
                for (int iField = 0; iField < newTableLine.FieldsCount; iField++)
                    newTableLine[iField] = new CSVField(l[iField]);

                table.AddLine(newTableLine);
            }

            return table;
        }
        #endregion

        #region Formatting
        public string Format(string[] fields)
        {
            if (fields == null)
                return String.Format("{0}", m_Delimiter);

            StringBuilder sb = new StringBuilder();
            foreach (string f in fields)
            {
                sb.Append(f);
                sb.Append(m_Delimiter);
            }
            return sb.ToString();
        }
        #endregion
    }
}
