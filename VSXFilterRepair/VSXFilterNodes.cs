using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace VSXFilterRepair
{
    abstract class NodeItem
    {
        public string IncludeName { get; set; }
        public string NodeName { get; set; }

        public NodeItem(string name)
        {
            this.NodeName = name;
        }

        abstract public void GenerateXmlNode(XmlWriter writer);
    }
    abstract class ItemGroupChild : NodeItem
    {
        public ItemGroupChild(string name) : base(name)
        {

        }
    }
    abstract class BuildNode : ItemGroupChild
    {
        public BuildNode(string name) : base(name)
        {

        }
        public Filter FilterNode { get; set; }
    }

    class FilterNodeCoprator : IEqualityComparer<Filter>
    {
        public bool Equals(Filter x, Filter y)
        {
            return x.IncludeName == y.IncludeName;
        }

        public int GetHashCode(Filter obj)
        {
            return 1;
        }
    }
    class ItemGroupChildCoprator : IEqualityComparer<ItemGroupChild>
    {
        public bool Equals(ItemGroupChild x, ItemGroupChild y)
        {
            if (x.GetType() == y.GetType())
                return x.IncludeName == y.IncludeName;
            else
                return false;
        }

        public int GetHashCode(ItemGroupChild obj)
        {
            return 1;
        }
    }
    class ItemGroup : NodeItem
    {
        public ItemGroup() : base("ItemGroup")
        {

        }
        public List<ItemGroupChild> Children = new List<ItemGroupChild> { };
        //public HashSet<ItemGroupChild> Children = new HashSet<ItemGroupChild>(new ItemGroupChildCoprator()) { };

        override public void GenerateXmlNode(XmlWriter writer)
        {
            writer.WriteStartElement(this.NodeName);
            foreach (var item in Children)
            {
                item.GenerateXmlNode(writer);
            }
            writer.WriteEndElement();
            writer.Flush();
        }
    }
    class VCXProj
    {
        public List<ItemGroup> ItemGroups = new List<ItemGroup> { };
        List<ItemGroupChild> MergedChildren = new List<ItemGroupChild> { };
        public void MergeChildren()
        {
            MergedChildren.Clear();
            foreach (var item in ItemGroups)
            {
                GetMergedChildren(item.Children);
                //MergedChildren.AddRange(item.Children);
            }
        }
        void GetMergedChildren(List<ItemGroupChild> orginal)
        {
            foreach (var item in orginal)
            {
                CheckChild(item, MergedChildren);
            }
        }
        public void CheckChild(ItemGroupChild child, List<ItemGroupChild> mergedList)
        {
            var innerChild = mergedList.SingleOrDefault(x => x.IncludeName == child.IncludeName);

            if (innerChild == null)
                mergedList.Add(child);

            else if (innerChild.GetType() != child.GetType())
                mergedList.Add(child);

            else if (child is BuildNode)
            {
                var newChildFilter = ((BuildNode)child).FilterNode;
                var innerChildFilter = ((BuildNode)innerChild).FilterNode;

                if (newChildFilter == null)
                    return;

                if (innerChildFilter == null)
                {
                    ((BuildNode)innerChild).FilterNode = newChildFilter;
                }
                else
                {
                    if (newChildFilter.IncludeName != Properties.Resources.DEFAULT_HEADER_FILTER &&
                        newChildFilter.IncludeName != Properties.Resources.DEFAULT_SOURCE_FILTER)
                    {
                        ((BuildNode)innerChild).FilterNode = newChildFilter;
                    }
                }
            }
        }
        public void GenerateXmlNode(XmlWriter writer)
        {
            foreach (var item in ItemGroups)
            {
                item.GenerateXmlNode(writer);
            }
        }

        public List<Filter> GetFilters()
        {
            if (MergedChildren.Count == 0)
                throw new Exception("Merged List is empty");
            var filterSet = new HashSet<Filter>(new FilterNodeCoprator());

            var filteredItems = MergedChildren.Where(x => x is BuildNode).Select(x => ((BuildNode)x).FilterNode).ToList();
            var nulls = filteredItems.Where(x => x == null).ToList();
            foreach (var item in filteredItems)
            {
                if (item != null)
                    filterSet.Add(item);
            }
            return filterSet.ToList();
        }
        public List<ItemGroupChild> GetSources()
        {
            if (MergedChildren.Count == 0)
                throw new Exception("Merged List is empty");

            var filteredItems = MergedChildren.Where(x => x.IncludeName.Contains(".cpp")
            || x.IncludeName.Contains(".hpp")
            && !(x is CustomBuild)
            ).ToList();
            return filteredItems;
        }
        public List<ItemGroupChild> GetCustomBuilds()
        {
            if (MergedChildren.Count == 0)
                throw new Exception("Merged List is empty");

            var filteredItems = MergedChildren.Where(x => x is CustomBuild).ToList();
            return filteredItems;
        }
        public List<ItemGroupChild> GetHeaders()
        {
            if (MergedChildren.Count == 0)
                throw new Exception("Merged List is empty");

            var filteredItems = MergedChildren.Where(x => x.IncludeName.ToLower().Contains(".h")
            && !(x is CustomBuild)).ToList();
            return filteredItems;
        }
        public List<ItemGroupChild> GetResources()
        {
            if (MergedChildren.Count == 0)
                throw new Exception("Merged List is empty");

            var filteredItems = MergedChildren.Where(x => x.IncludeName.ToLower().Contains(".rc")
            && !(x is CustomBuild)).ToList();
            return filteredItems;
        }
        public List<ItemGroupChild> GetImages()
        {
            if (MergedChildren.Count == 0)
                throw new Exception("Merged List is empty");

            var filteredItems = MergedChildren.Where(x => (x is ImageNode)
            && !(x is CustomBuild)
            ).ToList();
            return filteredItems;
        }
        public List<ItemGroupChild> GetNones()
        {
            if (MergedChildren.Count == 0)
                throw new Exception("Merged List is empty");

            var filteredItems = MergedChildren.Where(x => (x is None)
            && !(x is CustomBuild)
            ).ToList();

            return filteredItems;
        }
    }

    class UniqueIdentifier
    {
        public UniqueIdentifier()
        {

        }
        public UniqueIdentifier(string value)
        {
            this.Value = value;
        }
        public string Value { get; set; }
    }
    class Extensions
    {
        public Extensions()
        {

        }
        public Extensions(string value)
        {
            this.Value = value;
        }
        public string Value { get; set; }
    }
    class Filter : ItemGroupChild
    {
        public Filter() : base("Filter")
        {
            NodeUniqueIdentifier = new UniqueIdentifier();
            NodeExtensions = new Extensions();
        }
        public BuildNode BuildParent { get; set; }
        public UniqueIdentifier NodeUniqueIdentifier { get; set; }
        public Extensions NodeExtensions { get; set; }
        override public void GenerateXmlNode(XmlWriter writer)
        {
            writer.WriteStartElement(this.NodeName);
            writer.WriteAttributeString("Include", this.IncludeName);
            writer.WriteElementString("UniqueIdentifier", this.NodeUniqueIdentifier.Value);
            writer.WriteElementString("Extensions", this.NodeExtensions.Value);
            writer.WriteEndElement();
        }
    }
    class ClCompile : BuildNode
    {
        public ClCompile() : base("ClCompile")
        {

        }
        override public void GenerateXmlNode(XmlWriter writer)
        {
            writer.WriteStartElement(this.NodeName);
            writer.WriteAttributeString("Include", this.IncludeName);
            if (FilterNode != null)
            {
                writer.WriteElementString("Filter", FilterNode.IncludeName);
            }
            else
            {
                FilterNode = new Filter();
                FilterNode.BuildParent = this;
                if (this.IncludeName.ToLower().Contains(".h"))
                    FilterNode.IncludeName = Properties.Resources.DEFAULT_HEADER_FILTER;
                else if (this.IncludeName.ToLower().Contains(".cpp"))
                    FilterNode.IncludeName = Properties.Resources.DEFAULT_SOURCE_FILTER;

                writer.WriteElementString("Filter", FilterNode.IncludeName);
            }
            writer.WriteEndElement();
        }
    }
    class ClInclude : BuildNode
    {
        public ClInclude() : base("ClInclude")
        {

        }
        override public void GenerateXmlNode(XmlWriter writer)
        {
            writer.WriteStartElement(this.NodeName);
            writer.WriteAttributeString("Include", this.IncludeName);
            if (FilterNode != null)
            {
                writer.WriteElementString("Filter", FilterNode.IncludeName);
            }
            else
            {
                FilterNode = new Filter();
                FilterNode.BuildParent = this;

                if (this.IncludeName.ToLower().Contains(".h"))
                    FilterNode.IncludeName = Properties.Resources.DEFAULT_HEADER_FILTER;
                else if (this.IncludeName.ToLower().Contains(".cpp"))
                    FilterNode.IncludeName = Properties.Resources.DEFAULT_SOURCE_FILTER;

                writer.WriteElementString("Filter", FilterNode.IncludeName);
            }
            writer.WriteEndElement();
        }
    }
    class ResourceCompile : BuildNode
    {
        public ResourceCompile() : base("ResourceCompile")
        {

        }
        override public void GenerateXmlNode(XmlWriter writer)
        {
            writer.WriteStartElement(this.NodeName);
            writer.WriteAttributeString("Include", this.IncludeName);
            if (FilterNode != null)
            {
                writer.WriteElementString("Filter", FilterNode.IncludeName);
            }
            else
            {
                FilterNode = new Filter();
                FilterNode.BuildParent = this;

                FilterNode.IncludeName = Properties.Resources.DEFAULT_RESOURCE_FILTER;

                writer.WriteElementString("Filter", FilterNode.IncludeName);
            }
            writer.WriteEndElement();
        }
    }
    class ImageNode : BuildNode
    {
        public ImageNode() : base("Image")
        {

        }
        override public void GenerateXmlNode(XmlWriter writer)
        {
            writer.WriteStartElement(this.NodeName);
            writer.WriteAttributeString("Include", this.IncludeName);
            if (FilterNode != null)
            {
                writer.WriteElementString("Filter", FilterNode.IncludeName);
            }
            else
            {
                FilterNode = new Filter();
                FilterNode.BuildParent = this;

                FilterNode.IncludeName = Properties.Resources.DEFAULT_RESOURCE_FILTER;

                writer.WriteElementString("Filter", FilterNode.IncludeName);
            }
            writer.WriteEndElement();
        }
    }
    class None : BuildNode
    {
        public None() : base("None")
        {

        }
        override public void GenerateXmlNode(XmlWriter writer)
        {
            writer.WriteStartElement(this.NodeName);
            writer.WriteAttributeString("Include", this.IncludeName);
            if (FilterNode != null)
            {
                writer.WriteElementString("Filter", FilterNode.IncludeName);
            }
            else
            {
                // stay in root
                //FilterNode = new Filter();
                //FilterNode.BuildParent = this;

                //FilterNode.IncludeName = Properties.Resources.DEFAULT_RESOURCE_FILTER;

                //writer.WriteElementString("Filter", FilterNode.IncludeName);
            }
            writer.WriteEndElement();
        }
    }
    class CustomBuild : BuildNode
    {
        public CustomBuild() : base("CustomBuild")
        {

        }
        override public void GenerateXmlNode(XmlWriter writer)
        {
            writer.WriteStartElement(this.NodeName);
            writer.WriteAttributeString("Include", this.IncludeName);
            if (FilterNode != null)
            {
                writer.WriteElementString("Filter", FilterNode.IncludeName);
            }
            else
            {
                FilterNode = new Filter();
                FilterNode.BuildParent = this;

                if (this.IncludeName.ToLower().Contains(".h"))
                    FilterNode.IncludeName = Properties.Resources.DEFAULT_HEADER_FILTER;
                else if (this.IncludeName.ToLower().Contains(".cpp"))
                    FilterNode.IncludeName = Properties.Resources.DEFAULT_SOURCE_FILTER;
                else
                    FilterNode.IncludeName = Properties.Resources.DEFAULT_RESOURCE_FILTER;

                writer.WriteElementString("Filter", FilterNode.IncludeName);
            }
            writer.WriteEndElement();
        }
    }
}
