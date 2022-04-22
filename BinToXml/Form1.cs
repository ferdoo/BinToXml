using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace BinToXml
{

    public partial class Form1 : Form
    {

        private byte[] BinData = new byte[32000];
        private OpenFileDialog openFileDialog = new OpenFileDialog();
        private FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
        private string filePath;
        private string fileName;

        public Form1()
        {
            InitializeComponent();

            textBox1.Text = Properties.Settings.Default.DefaultFilePath;

        }

        private void button1_Click(object sender, EventArgs e)
        {
                        
            openFileDialog.Filter = "Binary File (*.bin)|*.bin";
            openFileDialog.FilterIndex = 1;
            openFileDialog.Title = "Select Binary File";
            openFileDialog.RestoreDirectory = true;


            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {

                fileName = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);

                
                if (textBox1.Text.Substring(textBox1.Text.Length - 1, 1) != @"\")
                {
                    filePath = textBox1.Text + @"\";
                }
                else
                {
                    filePath = textBox1.Text;
                }


                Array.Clear(BinData, 0, BinData.Length);
                                
                ReadBinary();

                ExportXml(filePath, fileName);
                                
            }
                       

        }


        private void ReadBinary()
        {
            FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open);

            using (BinaryReader binaryReader = new BinaryReader(fileStream))
            {
                BinData = new byte[binaryReader.BaseStream.Length];
                for (int pos = 0; pos < binaryReader.BaseStream.Length; pos++)
                {
                    BinData[pos] = (byte)binaryReader.BaseStream.ReadByte();
                }

                binaryReader.Close();
            }

        }

        private void ExportXml(string filePath, string fileName)
        {

            var pathWithFilename = filePath + fileName;

            StringBuilder stringBuilder = new StringBuilder();

            try
            {
                using (XmlWriter writer = XmlWriter.Create($"{pathWithFilename}.xml"))
                {
                    writer.WriteStartElement("BISCockpitUserData");
                    writer.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
                    writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");

                    writer.WriteElementString("Start_Address", "32768");
                    writer.WriteElementString("Byte_Length", "0");

                    foreach (var item in BinData)
                    {
                        if (item < 16)
                        {
                            stringBuilder.Append(" 0" + string.Format("{0:X}", item));
                        }
                        else if (item >= 16)
                        {
                            stringBuilder.Append(" " + string.Format("{0:X}", item));
                        }
                        else if (item == 0)
                        {
                            stringBuilder.Append(" 00");
                        }

                    }


                    writer.WriteElementString("Data", stringBuilder.ToString());
                    //writer.WriteElementString("Data", " FF FF AA"); //TEST

                    writer.WriteEndElement();
                    writer.Flush();

                    MessageBox.Show($"{fileName} > OK", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show("Directory Not Found", "Error", MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog.SelectedPath;
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.DefaultFilePath = textBox1.Text;
            Properties.Settings.Default.Save();
        }

        
    }
}
