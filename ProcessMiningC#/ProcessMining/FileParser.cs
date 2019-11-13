using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ProcessMining
{
    class FileParser
    {
        class ListComparer<T> : IEqualityComparer<List<T>>
        {
            public bool Equals(List<T> x, List<T> y)
            {
                return x.SequenceEqual(y);
            }

            public int GetHashCode(List<T> obj)
            {
                int hashcode = 0;
                foreach (T t in obj)
                {
                    hashcode ^= t.GetHashCode();
                }
                return hashcode;
            }
        }

        public static Dictionary<List<string>, int> ParseXES(string pathToFile)
        {
            Dictionary<List<String>,int> result = new Dictionary<List<string>, int>(new ListComparer<string>());
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(pathToFile);
                List<String> traceList = new List<string>();
                foreach (XmlNode process in doc.GetElementsByTagName("trace"))
                {
                    foreach (XmlNode child in process.ChildNodes)
                    {
                        if ("event" == child.Name)
                        {
                            foreach (XmlNode eventAttr in child.ChildNodes)
                            {
                                if ("concept:name" == eventAttr.Attributes[0].Value)
                                {
                                    traceList.Add(eventAttr.Attributes[1].Value);
                                    break;
                                }
                            }
                        }
                    }

                    if (result.ContainsKey(traceList))
                    {
                        result[traceList] += 1;
                    }
                    else
                    {
                        result.Add(traceList,1);
                    }
                    traceList=new List<string>();
                }

                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
