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


namespace Contoso_Jobs.Models
{
    public class Job : IXmlSerializable
    {
        public string Title { get; set; }

        public string Description { get; set; }
        
        public JobStatus Status { get; set; }

        [XmlIgnore]
        public IReadOnlyList<InkStroke> Strokes { get; set; }

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

            InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream();
            DataWriter dw = new DataWriter(ms.GetOutputStreamAt(0));
            byte[] tempBytes = new byte[1024];
            int bytesRead = 0;
            do
            {
                bytesRead = reader.ReadElementContentAsBinHex(tempBytes, 0, 1024);
                if (bytesRead > 0)
                {
                    dw.WriteBytes(tempBytes);
                }
            } while (bytesRead == 1024);

            await dw.StoreAsync();
            if (ms.Size > 0)
            {
                InkStrokeContainer inkCont = new InkStrokeContainer();
                await inkCont.LoadAsync(ms);
                Strokes = inkCont.GetStrokes().ToList();
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
            writer.WriteEndElement();

        }
    }
}
