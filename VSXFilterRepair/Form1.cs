using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace VSXFilter_Repair
{
    public partial class Form1 : Form
    {
        VCXProj content = new VCXProj();
        List<NodeItem> WrongNodes = new List<NodeItem>();
        List<Filter> Filters = new List<Filter>();
        HashSet<Filter> LostFilterNames = new HashSet<Filter>(new FilterNodeCoprator());

        // dictionary<IncludeName,Dictionary<FilterName,DefinitionCount>>
        Dictionary<string, Dictionary<string, int>> MultipleDefinitions = new Dictionary<string, Dictionary<string, int>>();
        
        bool CheckExistInFilters(BuildNode node)
        {
            foreach (var filter in Filters)
            {
                if (node.FilterNode != null)
                    if (filter.IncludeName == node.FilterNode.IncludeName)
                        return true;
            }
            return false;
        }
        void CheckItemsWithFilters()
        {
            foreach (ItemGroup itemGroup in content.ItemGroups)
            {
                foreach (var item in itemGroup.Children)
                {
                    if (item is BuildNode)
                    {
                        if (!CheckExistInFilters((BuildNode)item))
                        {
                            WrongNodes.Add(item);
                        }
                    }
                }
            }
        }
        void MakeReportOfLostFilters()
        {
            LostFilterNames.Clear();
            foreach (var item in WrongNodes)
            {
                if (item is BuildNode)
                    if (((BuildNode)item).FilterNode != null)
                        LostFilterNames.Add(((BuildNode)item).FilterNode);
            }
        }
        // Check Multiple Definition
        void CheckMultipleDefinition()
        {
            foreach (var itemGroup in content.ItemGroups)
            {
                foreach (var item in itemGroup.Children)
                {
                    if (item is BuildNode)
                    {
                        if (((BuildNode)item).FilterNode != null)
                        {
                            string filterName = ((BuildNode)item).FilterNode.IncludeName;
                            if (MultipleDefinitions.ContainsKey(item.IncludeName))
                            {
                                if (MultipleDefinitions[item.IncludeName].ContainsKey(filterName))
                                {
                                    MultipleDefinitions[item.IncludeName][filterName]++;
                                }
                                else
                                {
                                    //var filterCounterMap = new Dictionary<string, int>();
                                    //filterCounterMap.Add(filterName, 1);
                                    MultipleDefinitions[item.IncludeName].Add(filterName, 1);
                                }
                            }
                            else
                            {
                                var filterCounterMap = new Dictionary<string, int>();
                                filterCounterMap.Add(filterName, 1);
                                MultipleDefinitions.Add(item.IncludeName, filterCounterMap);
                            }
                        }
                    }
                }
            }
            // check for multiple
            var finalMap = new Dictionary<string, Dictionary<string, int>>(MultipleDefinitions);
            foreach (var item in finalMap)
            {
                if (item.Value.Count == 1)
                    MultipleDefinitions.Remove(item.Key);
            }
        }
        void GenerateFixedPart()
        {
            string fileName = "fixed-part.xml";
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("    ");
            settings.CloseOutput = true;
            settings.OmitXmlDeclaration = true;
            using (XmlWriter writer = XmlWriter.Create(fileName, settings))
            {
                writer.WriteStartElement("ItemGroup");
                foreach (var item in LostFilterNames)
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
                writer.Flush();
            }
        }
        public Form1()
        {
            InitializeComponent();
            var fileName = @"C:\Users\omidard\Documents\test\WindowsFormsApplication1\WindowsFormsApplication1\bin\Debug\Chief20.vcxproj.filters";
            VSXFilterRepair repair = new VSXFilterRepair();
            repair.DoRepair(fileName);
            //WrongNodes.Clear();
            ////
            //CheckItemsWithFilters();
            //MakeReportOfLostFilters();
            ////
            //Debug.WriteLine("--------------------------------");
            //Debug.WriteLine("------------Lost Filters--------");
            //Debug.WriteLine("--------------------------------");
            //int cnt = 0;
            //foreach (var item in LostFilterNames)
            //{
            //    Debug.WriteLine((cnt++).ToString() + "\t" + item.IncludeName);
            //}
            //Debug.WriteLine("--------------------------------");
            //----------------------------------------------
            //MultipleDefinitions.Clear();
            //CheckMultipleDefinition();
            //GenerateFixedPart();
            Thread.Sleep(100);
        }
    }
}
