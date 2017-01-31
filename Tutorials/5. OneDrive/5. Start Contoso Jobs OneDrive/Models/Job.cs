using Contoso_Jobs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.UI.Input.Inking;
using System.Xml;
using System.Xml.Schema;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Contoso_Jobs.Models
{
    public class Job : IXmlSerializable
    {
        public string Title { get; set; }

        public string Description { get; set; }
        
        public JobStatus Status { get; set; }
                
        public IReadOnlyList<InkStroke> Strokes { get; set; }

        public SoftwareBitmap Photo { get; set; }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public async void ReadXml(XmlReader reader)
        {
            reader.Read();
            Title = reader.ReadElementContentAsString("Title", "");
            Description = reader.ReadElementContentAsString("Description", "");
            string s = reader.ReadElementContentAsString("Status", "");
            Status = (JobStatus)Enum.Parse(typeof(JobStatus), s);

            if (!reader.IsEmptyElement)
            {
                InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream();
                DataWriter dw = new DataWriter(ms.GetOutputStreamAt(0));
                byte[] tempBytes;
                int bytesRead = 0;
                int totalBytes = 0;
                do
                {
                    tempBytes = new byte[1024];
                    bytesRead = reader.ReadElementContentAsBinHex(tempBytes, 0, 1024);
                    if (bytesRead > 0)
                    {
                        dw.WriteBytes(tempBytes);
                        totalBytes += bytesRead;
                    }

                } while (bytesRead == 1024);

                await dw.StoreAsync();
                if (totalBytes > 1)
                {
                    InkStrokeContainer inkCont = new InkStrokeContainer();
                    await inkCont.LoadAsync(ms);
                    Strokes = inkCont.GetStrokes().ToList();
                }
                reader.ReadEndElement();
            }
            else
            {
                reader.Read();
            }

            if (!reader.IsEmptyElement)
            {
                InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream();
                DataWriter dw = new DataWriter(ms.GetOutputStreamAt(0));
                byte[] tempBytes;
                int bytesRead = 0;
                int totalBytesRead = 0;
                do
                {
                    tempBytes = new byte[1024];
                    bytesRead = reader.ReadContentAsBinHex(tempBytes, 0, 1024);
                    if (bytesRead > 0)
                    {
                        dw.WriteBytes(tempBytes);
                        totalBytesRead += bytesRead;
                    }

                } while (bytesRead == 1024);

                await dw.StoreAsync();
                if (totalBytesRead > 1)
                {
                    //load bytes as image
                    byte[] bytes = new byte[ms.Size];
                    //var dataWriter = new DataWriter(ms);
                    var dataReader = new DataReader(ms.GetInputStreamAt(0));

                    await dataReader.LoadAsync((uint)ms.Size);
                    dataReader.ReadBytes(bytes);
                    //TODO: this should change based on the resolution you store the photos at
                    Photo = new SoftwareBitmap(BitmapPixelFormat.Bgra8, 640, 360);
                    Photo.CopyFromBuffer(bytes.AsBuffer());
                }
                reader.ReadEndElement();
            }
            else
            {
                reader.Read();
            }


            reader.Skip();

        }

        public async void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("Title", Title);
            writer.WriteElementString("Description", Description);
            writer.WriteElementString("Status", Status.ToString());
            writer.WriteStartElement("Stokes");
            if (Strokes != null && Strokes.Count > 0)
            {
                using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
                {
                    InkStrokeContainer inkCont = new InkStrokeContainer();

                    foreach (InkStroke stroke in Strokes)
                    {
                        inkCont.AddStroke(stroke.Clone());
                    }

                    await inkCont.SaveAsync(ms);
                    await ms.FlushAsync();

                    byte[] bytes = new byte[ms.Size];
                    //var dataWriter = new DataWriter(ms);
                    var reader = new DataReader(ms.GetInputStreamAt(0));
                    await reader.LoadAsync((uint)ms.Size);
                    reader.ReadBytes(bytes);

                    writer.WriteBinHex(bytes, 0, (int)ms.Size);
                }
            }
            else
            {
                byte[] bytes = new byte[1];
                bytes[0] = 0xF;
                writer.WriteBinHex(bytes, 0, 1);
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Photo");

            if (Photo != null)
            {

                InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream();
                if (null != ms)
                {
                    try
                    {

                        byte[] buffer = new byte[4 * Photo.PixelWidth * Photo.PixelHeight];
                        Photo.CopyToBuffer(buffer.AsBuffer());

                        writer.WriteBinHex(buffer, 0, (int)buffer.Length);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }

            }
            else

            {
                byte[] bytes = new byte[1];
                bytes[0] = 0;
                writer.WriteBinHex(bytes, 0, 1);
            }
            writer.WriteEndElement();


        }
    }
}
