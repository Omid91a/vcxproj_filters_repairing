using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace VSXFilterRepair
{
    public class VSXFilterRepair
    {
        VCXProj content = new VCXProj();

        void AddFiltersItemGroup(XmlWriter writer)
        {
            var filters = content.GetFilters();
            writer.WriteStartElement("ItemGroup");
            foreach (var item in filters)
            {
                var filter = new Filter();
                filter.IncludeName = item.IncludeName;
                if (item.BuildParent.IncludeName.Contains(".cpp"))
                {
                    filter.NodeExtensions = new Extensions("cpp;c;cc;cxx;def;odl;idl;hpj;bat;asm;asmx");
                }
                else if (item.BuildParent.IncludeName.Contains(".h"))
                {
                    filter.NodeExtensions = new Extensions("h;hh;hpp;hxx;hm;inl;inc;xsd");
                }
                filter.GenerateXmlNode(writer);
            }
            writer.WriteEndElement();
        }

        public bool CheckFileValidity(object p)
        {
            throw new NotImplementedException();
        }

        void AddResourcesItemGroup(XmlWriter writer)
        {
            var resources = content.GetResources();
            writer.WriteStartElement("ItemGroup");
            foreach (var item in resources)
            {
                item.GenerateXmlNode(writer);
            }
            writer.WriteEndElement();
        }
        void AddHeadersItemGroup(XmlWriter writer)
        {
            var headers = content.GetHeaders();
            writer.WriteStartElement("ItemGroup");
            foreach (var item in headers)
            {
                item.GenerateXmlNode(writer);
            }
            writer.WriteEndElement();
        }
        void AddCustomBuildsItemGroup(XmlWriter writer)
        {
            var sources = content.GetCustomBuilds();
            writer.WriteStartElement("ItemGroup");
            foreach (var item in sources)
            {
                item.GenerateXmlNode(writer);
            }
            writer.WriteEndElement();
        }
        void AddSourcesItemGroup(XmlWriter writer)
        {
            var sources = content.GetSources();
            writer.WriteStartElement("ItemGroup");
            foreach (var item in sources)
            {
                item.GenerateXmlNode(writer);
            }
            writer.WriteEndElement();
        }
        void AddImagesItemGroup(XmlWriter writer)
        {
            var sources = content.GetImages();
            writer.WriteStartElement("ItemGroup");
            foreach (var item in sources)
            {
                item.GenerateXmlNode(writer);
            }
            writer.WriteEndElement();
        }
        void AddNoneItemGroup(XmlWriter writer)
        {
            var sources = content.GetNones();
            writer.WriteStartElement("ItemGroup");
            foreach (var item in sources)
            {
                item.GenerateXmlNode(writer);
            }
            writer.WriteEndElement();
        }

        public void GetAllNodeChildren(XmlNodeList root)
        {
            foreach (XmlNode item in root)
            {
                if (item.Name == "ItemGroup")
                {
                    content.ItemGroups.Add(new ItemGroup());
                }
                else if (item.Name == "CustomBuild")
                {
                    var child = new CustomBuild();
                    child.IncludeName = item.Attributes["Include"].Value;
                    content.ItemGroups.Last().Children.Add(child);
                }
                else if (item.Name == "ClCompile")
                {
                    var child = new ClCompile();
                    child.IncludeName = item.Attributes["Include"].Value;
                    content.ItemGroups.Last().Children.Add(child);
                }
                else if (item.Name == "ClInclude")
                {
                    var child = new ClInclude();
                    child.IncludeName = item.Attributes["Include"].Value;
                    content.ItemGroups.Last().Children.Add(child);
                }
                else if (item.Name == "ResourceCompile")
                {
                    var child = new ResourceCompile();
                    child.IncludeName = item.Attributes["Include"].Value;
                    content.ItemGroups.Last().Children.Add(child);
                }
                else if (item.Name == "Image")
                {
                    var child = new ImageNode();
                    child.IncludeName = item.Attributes["Include"].Value;
                    content.ItemGroups.Last().Children.Add(child);
                }
                else if (item.Name == "None")
                {
                    var child = new None();
                    child.IncludeName = item.Attributes["Include"].Value;
                    content.ItemGroups.Last().Children.Add(child);
                }
                else if (item.Name == "Filter")
                {
                    var child = new Filter();
                    var attr = item.Attributes["Include"];
                    if (attr == null)
                    {
                        child.IncludeName = item.InnerText;
                        if (content.ItemGroups.Last().Children.Count > 0)
                        {
                            ((BuildNode)(content.ItemGroups.Last().Children.Last())).FilterNode = child;
                            child.BuildParent = (BuildNode)(content.ItemGroups.Last().Children.Last());
                        }
                    }
                    else
                    {
                        child.IncludeName = attr.Value;
                        content.ItemGroups.Last().Children.Add(child);
                    }
                }
                else if (item.Name == "UniqueIdentifier")
                {
                    var child = new UniqueIdentifier();
                    child.Value = item.InnerText;
                    if (content.ItemGroups.Last().Children.Count > 0)
                        ((Filter)(content.ItemGroups.Last().Children.Last())).NodeUniqueIdentifier = child;
                }
                else if (item.Name == "Extensions")
                {
                    var child = new Extensions();
                    child.Value = item.InnerText;
                    if (content.ItemGroups.Last().Children.Count > 0)
                        ((Filter)(content.ItemGroups.Last().Children.Last())).NodeExtensions = child;
                }
                GetAllNodeChildren(item.ChildNodes);
            }
        }
        public void GenerateNewFilterFile(string fileName)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            //settings.IndentChars = ("    ");
            //settings.CloseOutput = true;
            //settings.OmitXmlDeclaration = true;
            using (XmlWriter writer = XmlWriter.Create(fileName, settings))
            {
                writer.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");
                writer.WriteStartElement("Project");
                writer.WriteAttributeString("ToolsVersion", "4.0");
                writer.WriteAttributeString("xml", "ns", null, "http://schemas.microsoft.com/developer/msbuild/2003");
                //
                content.MergeChildren();
                // 
                // add sources
                AddSourcesItemGroup(writer);
                // add headers
                AddHeadersItemGroup(writer);
                // add resources
                AddResourcesItemGroup(writer);
                // add custom builds
                AddCustomBuildsItemGroup(writer);
                // add Images
                AddImagesItemGroup(writer);
                // add Nones
                AddNoneItemGroup(writer);
                // add filters
                AddFiltersItemGroup(writer);
                //
                writer.WriteEndElement();
                writer.Flush();
            }
            var repairedData = File.ReadAllText(fileName).Replace("xml:ns", "xmlns");
            File.WriteAllText(fileName, repairedData);
        }

        public bool CheckFileValidity(string fileName)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                Console.WriteLine("Begin read...");
                doc.Load(fileName);
                if (doc.ChildNodes.Count > 0)
                    Console.WriteLine("File content is valid.");
                else
                    Console.WriteLine("There is no content. Probably file is broken.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("File content is not Valid!!");
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public bool DoRepair(string fileName)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                Console.WriteLine("Begin read...");
                doc.Load(fileName);

                Console.WriteLine("Fetching nodes...");
                GetAllNodeChildren(doc.ChildNodes);

                Console.WriteLine("Old file renamed.");
                File.Copy(fileName, fileName + ".old", true);
                File.Delete(fileName);

                Console.WriteLine("Generating new filter file...");
                GenerateNewFilterFile(fileName);

                Console.WriteLine("Repairing file successfully.");
                Console.WriteLine("The VSXFilter file updated.");
                Console.WriteLine(fileName);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed!!");
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
