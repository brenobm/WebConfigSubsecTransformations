using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Web.Publishing.Tasks;
using Microsoft.Web.XmlTransform;
using System.Text.RegularExpressions;

namespace WebConfigSubsecTransformations
{
    public class WebConfigSubsecTransform : Transform
    {
        private string pattern;
        private IList<string> patterns;
        private string replacement;
        private IList<string> replacements;
        private string attributeName;
        private string elementName;

        protected string AttributeName
        {
            get
            {
                if (this.attributeName == null)
                {
                    this.attributeName = this.GetArgumentValue("Attribute");
                }
                return this.attributeName;
            }
        }
        protected IList<string> Patterns
        {
            get
            {
                if (this.pattern == null)
                {
                    this.pattern = this.GetArgumentValue("Pattern");
                    this.patterns = this.pattern.Split('|');
                }

                return patterns;
            }
        }

        protected IList<string> Replacements
        {
            get
            {
                if (this.replacement == null)
                {
                    this.replacement = this.GetArgumentValue("Replacement");
                    this.replacements = this.replacement.Split('|');
                }

                return replacements;
            }
        }

        protected string ElementName
        {
            get
            {
                if (this.elementName == null)
                {
                    this.elementName = this.GetArgumentValue("ElementName");
                }

                return elementName;
            }
        }

        protected string GetArgumentValue(string name)
        {
            // this extracts a value from the arguments provided
            if (string.IsNullOrWhiteSpace(name))
            { throw new ArgumentNullException("name"); }

            string result = null;
            if (this.Arguments != null && this.Arguments.Count > 0)
            {
                foreach (string arg in this.Arguments)
                {
                    if (!string.IsNullOrWhiteSpace(arg))
                    {
                        string trimmedArg = arg.Trim();
                        if (trimmedArg.ToUpperInvariant().StartsWith(name.ToUpperInvariant()))
                        {
                            int start = arg.IndexOf('\'');
                            int last = arg.LastIndexOf('\'');
                            if (start <= 0 || last <= 0 || last <= 0)
                            {
                                throw new ArgumentException("Expected two ['] characters");
                            }

                            string value = trimmedArg.Substring(start, last - start);
                            if (value != null)
                            {
                                // remove any leading or trailing '
                                value = value.Trim().TrimStart('\'').TrimStart('\'');
                            }
                            result = value;
                        }
                    }
                }
            }
            return result;
        }

        protected override void Apply()
        {
            foreach(XmlNode node in this.TargetChildNodes)
            {
                NavigateChildrenNodes(node);
            }            
        }

        private void NavigateChildrenNodes(XmlNode node)
        {
            if (node == null)
                return;

            if (node.ChildNodes != null)
                foreach (XmlNode nodeChild in node.ChildNodes)
                {
                    NavigateChildrenNodes(nodeChild);
                }

            if (string.Compare(node.Name, this.ElementName, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                foreach (XmlAttribute att in node.Attributes)
                {
                    if (string.Compare(att.Name, this.AttributeName, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        // get current value, perform the Regex
                        for (int i = 0; i < (this.Patterns.Count <= this.Replacements.Count ? this.Patterns.Count : this.Replacements.Count); i++)
                        {
                            att.Value = Regex.Replace(att.Value, this.Patterns[i], this.Replacements[i]);
                        }
                    }
                }
            }
        }

    }
}
