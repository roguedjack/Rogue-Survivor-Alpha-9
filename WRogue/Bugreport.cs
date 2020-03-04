using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace djack.RogueSurvivor
{
    public partial class Bugreport : Form
    {
        Exception m_Exception;
        string NL = Environment.NewLine;

        public Bugreport(Exception e)
        {
            InitializeComponent();

            m_Exception = e;
        }

        private void m_OkButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Bugreport_Load(object sender, EventArgs e)
        {
            m_HeaderTextBox.Text = "Rogue Survivor encoutered a fatal error."+NL+
                "Please report all the text in the textbox below to the author (copypaste it, remember to scroll all the way down from start to end)."+NL+
                "Press OK to exit.";

            m_LogTextBox.Clear();
            m_LogTextBox.AppendText("Start of report."+NL);

            m_LogTextBox.AppendText("-----------------------------------------------" + NL);
            m_LogTextBox.AppendText("EXCEPTION"+NL);
            m_LogTextBox.AppendText(m_Exception.ToString() + NL);

            m_LogTextBox.AppendText("-----------------------------------------------" + NL);
            m_LogTextBox.AppendText("LOG"+NL);
            foreach (string line in Logger.Lines)
                m_LogTextBox.AppendText(line+NL);
            m_LogTextBox.AppendText("-----------------------------------------------"+NL);

            m_LogTextBox.AppendText("End of report."+NL);
        }
    }
}
