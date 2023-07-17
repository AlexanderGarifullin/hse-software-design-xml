using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace xmllimq
{
    internal class Program
    {
       
        static string GetPath(string fileName)
        {
            string currentDirectory = Directory.GetCurrentDirectory(); 
            string parentDirectory = Directory.GetParent(currentDirectory)?.Parent?.FullName; 
            string filesDirectory = Path.Combine(parentDirectory, "Files");
            string filePath = Path.Combine(filesDirectory, fileName); 
            return filePath;
        }
        static void Task6(string txtName, string xmlName)
        {
            Console.WriteLine("1");
            string[] lines;
            try
            {
                lines = File.ReadAllLines(GetPath(txtName));
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Ошибка!");
                return;
            }
            XDocument doc = new XDocument(new XDeclaration("1.0", "windows-1251", "no"));

            XElement root = new XElement("root");
            doc.Add(root);
            
            foreach(string line in lines)
            {
                XElement lineElement = new XElement("line");
                int sum = 0;
                int[] numbers = line.Split(' ').Select(int.Parse).OrderByDescending(n => n).ToArray();
                foreach (int number in numbers)
                {
                    XElement numberElement = new XElement("number", number);
                    lineElement.Add(numberElement);
                    sum += number;
                }
                lineElement.SetAttributeValue("sum", sum);
                doc.Root.Add(lineElement);
            }

            doc.Save(GetPath(xmlName)); 
            
            Console.WriteLine("6 WORKED");
            Console.WriteLine("============");
        }

        static void Task16(string xmlName)
        {
            Console.WriteLine("2");

            XDocument doc;
            try
            {
                doc = XDocument.Load(GetPath(xmlName));
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Ошибка!");
                return;
            }

            var elements = doc.Root.Elements() 
            .OrderByDescending(e => e.Elements().Attributes().Count())
            .ThenBy(e => e.Name.LocalName) 
            .Select(e => new
            {
                ElementName = e.Name.LocalName,
                AttributeCount = e.Elements().Attributes().Count() 
            });
            foreach (var element in elements)
            {
                Console.WriteLine($"{element.ElementName} : {element.AttributeCount}");
            }

           
            Console.WriteLine("16 WORKED");
            Console.WriteLine("============");
        }

        static void Task26(string olxXMLName, string newXMLName)
        {
            Console.WriteLine("3");
            XDocument doc;
            try
            {
                doc = XDocument.Load(GetPath(olxXMLName));
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Ошибка!");
                return;
            }
            doc.Descendants().Attributes().Where(a => a != a.Parent.FirstAttribute).Remove();

            doc.Save(GetPath(newXMLName));
            Console.WriteLine("26 WORKED");
            Console.WriteLine("============");
        }

        static void Task36(string olxXMLName, string newXMLName)
        {
            Console.WriteLine("4");
            XDocument doc;
            try
            {
                doc = XDocument.Load(GetPath(olxXMLName));
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Ошибка!");
                return;
            }

    
            doc.Root.Elements()
            .SelectMany(el1 => el1.Elements())
            .Where(el2 => el2.Descendants().Any())
            .ToList()
            .ForEach(el2 => el2.Add(new XAttribute("node-count", el2.Descendants().Count())));


            doc.Save(GetPath(newXMLName));
            Console.WriteLine("36 WORKED");
            Console.WriteLine("============");
        }
        static void Task46(string olxXMLName, string newXMLName)
        {
            Console.WriteLine("5");
            XDocument doc;
            try
            {
                doc = XDocument.Load(GetPath(olxXMLName));
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Ошибка!");
                return;
            }

            AddOddNodeCountAttribute(doc.Root);


            doc.Save(GetPath(newXMLName));
            Console.WriteLine("46 WORKED");
            Console.WriteLine("============");
        }

        static void AddOddNodeCountAttribute(XElement element)
        {
            if (element.HasElements)
            {

                int totalNodeCount = element.Elements()
              .Sum(childElement => childElement.DescendantNodesAndSelf().Count());


                bool isOddNodeCount = totalNodeCount % 2 == 1;
                
                element.Add(new XAttribute("odd-node-count", isOddNodeCount));
                foreach (var childElement in element.Elements())
                {
                    AddOddNodeCountAttribute(childElement);
                }
            }
        }

        static void Task56(string olxXMLName, string newXMLName)
        {
            Console.WriteLine("6");
            XDocument doc;
            try
            {
                doc = XDocument.Load(GetPath(olxXMLName));
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Ошибка!");
                return;
            }

            XElement rootElement = doc.Root;

            XAttribute xmlnsAttr = rootElement.Attributes().FirstOrDefault(attr => attr.IsNamespaceDeclaration);
   

            string namespaceUri = xmlnsAttr?.Value;
            

            XNamespace ns = namespaceUri;


            foreach (XElement element in rootElement.Elements())
            {
                element.Name = ns + element.Name.LocalName;
            }


            doc.Save(GetPath(newXMLName));

            Console.WriteLine("56 WORKED");
            Console.WriteLine("============");
        }
        static void Task66(string olxXMLName, string newXMLName)
        {
            Console.WriteLine("7");
            XDocument doc;
            try
            {
                doc = XDocument.Load(GetPath(olxXMLName));
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Ошибка!");
                return;
            }

            var result = doc.Descendants("client")
            .SelectMany(c => c.Elements("info")
                .Select(i => new
                {
                    Year = DateTime.Parse(i.Attribute("date").Value).Year,
                    Month = DateTime.Parse(i.Attribute("date").Value).Month,
                    ClientId = c.Attribute("id").Value,
                    Time = XmlConvert.ToTimeSpan(i.Attribute("time").Value).TotalMinutes
                }))
            .GroupBy(i => new { i.Year, i.Month })
            .Where(g => g.Count() >= 3)
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month);

 
            XDocument resultXmlDoc = new XDocument(
                new XDeclaration("1.0", "windows-1251", "no"),
                new XElement("root",
                    result.Select(g =>
                        new XElement($"d{g.Key.Year}-{g.Key.Month}",
                            new XAttribute("total-time", g.Sum(i => i.Time)),
                            new XAttribute("client-count", g.Count()),
                            g.OrderBy(i => i.ClientId)
                             .Select(i => new XElement($"id{i.ClientId}",
                                 new XAttribute("time", i.Time)))))));

            resultXmlDoc.Save(GetPath(newXMLName));
            Console.WriteLine("66 WORKED");
            Console.WriteLine("============");
        }
        static void Task76(string olxXMLName, string newXMLName)
        {
            Console.WriteLine("8");
            XDocument doc;
            try
            {
                doc = XDocument.Load(GetPath(olxXMLName));
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Ошибка!");
                return;
            }


           var newDebts = doc.Root.Elements("record")
          .Select(record => new XElement("debt",
              new XAttribute("house", record.Element("house").Value),
              new XAttribute("flat", record.Element("flat").Value),
              new XElement("name", record.Element("name").Value),
              new XElement("value", record.Element("debt").Value)
          ));

            doc.Root.ReplaceAll(newDebts);

            doc.Save(GetPath(newXMLName));


            Console.WriteLine("76 WORKED");
            Console.WriteLine("============");
        }
        static void Task86(string olxXMLName, string newXMLName)
        {
            Console.WriteLine("9");
            XDocument doc;
            try
            {
                doc = XDocument.Load(GetPath(olxXMLName));
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Ошибка!");
                return;
            }

            var pupils = doc.Descendants("pupil")
          .Select(p => new
          {
              Name = p.Attribute("name").Value.Replace(" ", "_"),
              Class = Convert.ToInt32(p.Attribute("class").Value),
              InfoList = p.Descendants("info")
                  .Select(i => new
                  {
                      Mark = Convert.ToInt32(i.Attribute("mark").Value),
                      Subject = i.Attribute("subject").Value
                  })
                  .OrderByDescending(i => i.Mark)
                  .ThenBy(i => i.Subject)
                  .ToList()
          })
          .OrderBy(p => p.Name);
     

            XDocument newDoc = new XDocument(
            new XElement("pupils",
                pupils.Select(p =>
                    new XElement(p.Name,
                        new XAttribute("class", p.Class),
                        p.InfoList.Select(i =>
                            new XElement("mark" + i.Mark,
                                new XAttribute("subject", i.Subject)
                            )
                        )
                    )
                )
            )
        );
            newDoc.Save(GetPath(newXMLName));

            Console.WriteLine("86 WORKED");
            Console.WriteLine("============");
        }
        static void Task63(string olxXMLName, string newXMLName)
        {
            Console.WriteLine("10");
            XDocument doc;
            try
            {
                doc = XDocument.Load(GetPath(olxXMLName));
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Ошибка!");
                return;
            }


            var clients = doc.Descendants("record")
         .Select(r => new
         {
             ClientId = r.Attribute("id").Value,
             Year = DateTime.Parse(r.Attribute("date").Value).Year,
             Month = DateTime.Parse(r.Attribute("date").Value).Month,
             Time = r.Attribute("time").Value
         })
         .OrderBy(r => r.ClientId)
            .ThenBy(r => r.Year)
            .ThenBy(r => r.Month)
            .ThenBy(r => r.Time);

            doc = new XDocument(
             new XElement("clients",
                 clients.GroupBy(c => c.ClientId)
                 .Select(g =>
                     new XElement("client",
                         new XAttribute("id", g.Key),
                         g.Select(c =>
                             new XElement("time",
                                 new XAttribute("year", c.Year),
                                 new XAttribute("month", c.Month),
                                 c.Time)
                             )
                         )
                     )
                 )
             );

            doc.Save(GetPath(newXMLName));

            Console.WriteLine("63 WORKED");
            Console.WriteLine("============");
        }

        static void Main(string[] args)
        {
            // 1
            Task6("6text.txt","6xml.xml");
            // 2
            Task16("16xml.xml");
            // 3          
            Task26("26oldxml.xml", "26newxml.xml");
            // 4
            Task36("36oldxml.xml", "36newxml.xml");
            // 5
            Task46("46oldxml.xml", "46newxml.xml");
            // 6
            Task56("56oldxml.xml", "56newxml.xml");
            // 7
            Task66("66oldxml.xml", "66newxml.xml");
            // 8
            Task76("76oldxml.xml", "76newxml.xml");
            // 9
            Task86("86oldxml.xml", "86newxml.xml");
            // 10
            Task63("63oldxml.xml", "63newxml.xml");
        }
    }
}
