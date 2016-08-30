namespace EntityBuilder
{
    using Entities;
    using Entities.Common;
    using Entities.Containment;
    using Entities.Grouping;
    using Entities.Imaging;
    using Entities.Queries;
    using Microsoft.Bond;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Xml;

    public class InjectionDataBuilder
    {
        public static int Main(string[] args)
        {
            try
            {
                var dictsApp = ParseAppXml(@"C:\Users\yajxu\Desktop\Satori\CompositeApp.Normalized.Xml");
                var dictsTv = ParseTVSeries(@"C:\Users\yajxu\Desktop\Satori\TVSeries.Normalized.Xml");
                var dictsYesky = ParseYesky(@"C:\Users\yajxu\Desktop\Satori\Yesky.Normalized.Xml");
                var dictsEditorial = ParseEditorial(@"C:\Users\yajxu\Desktop\Satori\Editorial.Entity.txt");
                var dicts = dictsApp.Union(dictsTv).Union(dictsYesky).Union(dictsEditorial);
                //var dicts = dictsYesky;

                var entityWriter = new StreamWriter(@"C:\Users\yajxu\Desktop\entity.txt");
                var queryWriter = new StreamWriter(@"C:\Users\yajxu\Desktop\query.txt");
                foreach (var dict in dicts)
                {
                    var container = HackSatoriEntity(dict);
                    var bytes = XapHelper.SerializeToByteArray(container);
                    var encode = Convert.ToBase64String(bytes);
                    entityWriter.WriteLine(dict["SatoriId"] + "\t" + encode);
                    queryWriter.WriteLine(dict["Keywords"] + "\t" + dict["SatoriId"]);

                    entityWriter.Flush();
                    queryWriter.Flush();
                }

                entityWriter.Close();
                queryWriter.Close();

                return 0;
            }   
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return -1;
            }
        }

        //Required keys: SatoriId, ThumbnailId, ThumbnailWidth, ThumbnailHeight, Name, DominantType, Description
        public static EntityContainer_2 HackSatoriEntity(Dictionary<string, string> dict)
        {
            var entityContainer = XapHelper.CreateInstance<EntityContainer_2>();

            var dataGroupContainer = XapHelper.CreateInstance<DataGroupContainer_2>();
            var info = XapHelper.CreateInstance<DataGroupInfo_2>();
            info.Context = DisplayContext_2.Default;
            info.DisplayHint = DisplayHint_2.EntityItem;
            info.Key = "cdb:datagroupid.bullseye";
            var dataGroup = XapHelper.CreateInstance<DataGroup_2>();
            dataGroup.Info = info;

            var imgProperty = XapHelper.CreateInstance<PropertyPath_2>();
            imgProperty.XPath = "/EntityContent/Image";
            imgProperty.EnhancedXPath = "Entities.Containment.EntityContainer_2:EntityContent/Entities.BasicEntity_2:Image";
            var descProperty = XapHelper.CreateInstance<PropertyPath_2>();
            descProperty.XPath = "/EntityContent/Description";
            descProperty.EnhancedXPath = "Entities.Containment.EntityContainer_2:EntityContent/Entities.BasicEntity_2:Description";
            var domProperty = XapHelper.CreateInstance<PropertyPath_2>();
            domProperty.XPath = "/EntityContent/DominantType";
            domProperty.EnhancedXPath = "Entities.Containment.EntityContainer_2:EntityContent/Entities.BasicEntity_2:DominantType";
            dataGroup.Properties = new List<Bonded<PropertyBase_2>> { new Bonded<PropertyBase_2>(imgProperty), new Bonded<PropertyBase_2>(descProperty), new Bonded<PropertyBase_2>(domProperty) };

            entityContainer.DataGroupContainer = new Bonded<DataGroupContainer_2>(dataGroupContainer);
            entityContainer.DataGroupContainer.Value.DataGroups = new List<Bonded<DataGroup_2>> { new Bonded<DataGroup_2>(dataGroup) };
            entityContainer.DataGroupContainer.Value.Scenario = "GenericEntityListItem";

            var entityContent = XapHelper.CreateInstance<BasicEntity_2>();
            entityContent.SatoriId = dict["SatoriId"];
            var image = XapHelper.CreateInstance<BingImage_2>();
            image.ThumbnailId = dict["ThumbnailId"];
            image.ThumbnailWidth = (uint?)Convert.ToUInt32(dict["ThumbnailWidth"]);
            image.ThumbnailHeight = (uint?)Convert.ToUInt32(dict["ThumbnailHeight"]);
            image.PartnerId = "MsnJVFeeds";
            image.SourceUrl = string.Format("http://www.bing.com/th?id={0}", dict["SatoriId"]);
            var sourceImageProperties = XapHelper.CreateInstance<ImageProperties_2>();
            sourceImageProperties.Width = Convert.ToUInt32(dict["ThumbnailWidth"]);
            sourceImageProperties.Height = Convert.ToUInt32(dict["ThumbnailHeight"]);
            sourceImageProperties.Format = ImageFormat_2.Jpeg;
            sourceImageProperties.Size = sourceImageProperties.Width * sourceImageProperties.Height;
            image.SourceImageProperties = sourceImageProperties;
            entityContent.Image = new Bonded<Image_2>(image);
            entityContent.Name = dict["Name"];
            entityContent.DominantType = dict["DominantType"];
            var description = XapHelper.CreateInstance<Description_2>();
            description.Text = dict["Description"];
            entityContent.Description = description;
            var seeMoreQuery = XapHelper.CreateInstance<InternalQuery_2>();
            seeMoreQuery.QueryText = dict["Name"];
            seeMoreQuery.QueryUrlText = dict["Name"];
            var queryParameter1 = XapHelper.CreateInstance<QueryParameters_2>();
            queryParameter1.Name = "ufn";
            queryParameter1.Values = new List<string> { dict["Name"] };
            var queryParameter2 = XapHelper.CreateInstance<QueryParameters_2>();
            queryParameter2.Name = "sid";
            queryParameter2.Values = new List<string> { dict["SatoriId"] };
            seeMoreQuery.QueryParams = new List<QueryParameters_2> { queryParameter1, queryParameter2 };
            entityContent.SeeMoreQuery = new Bonded<QueryBase_2>(seeMoreQuery);

            entityContainer.EntityContent = new Bonded<BasicEntity_2>(entityContent);

            return entityContainer;
        }

        public static IEnumerable<Dictionary<string, string>> ParseAppXml(string file)
        {
            var doc = new XmlDocument();
            doc.Load(file);

            foreach (XmlNode node in doc.SelectNodes("root/item"))
            {
                var apps =
                    node.SelectNodes(
                        "Modules/MsnJVData.Module/Components/MsnJVData.ContainerComponent/Components/MsnJVData.TabComponent/TabItemList/MsnJVData.TabItem");

                var pcApp = apps == null
                    ? null
                    : apps.Cast<XmlNode>()
                        .FirstOrDefault(x => x.SelectSingleNode("Title/Items/MsnJVData.Item/Text").InnerText == "电脑版");

                if (pcApp != null)
                {
                    var dict = new Dictionary<string, string>
                    {
                        {
                            "Keywords",
                            node.SelectSingleNode(
                                "Keywords")
                                .InnerText
                        },
                        {
                            "ThumbnailId",
                            Helper.ExtractThumbnailId(pcApp.SelectSingleNode(
                                "ContentList/MsnJVData.ContainerComponent/Components/MsnJVData.ImageComponent/Image/Url")
                                .InnerText )
                        },
                        {
                            "ThumbnailWidth",
                            pcApp.SelectSingleNode(
                                "ContentList/MsnJVData.ContainerComponent/Components/MsnJVData.ImageComponent/Image/Width")
                                .InnerText
                        },
                        {
                            "ThumbnailHeight",
                            pcApp.SelectSingleNode(
                                "ContentList/MsnJVData.ContainerComponent/Components/MsnJVData.ImageComponent/Image/Height")
                                .InnerText
                        },
                        {
                            "Name",
                            node.SelectSingleNode("Modules/MsnJVData.Module/Components/MsnJVData.TitleComponent/Title/Text")
                                .InnerText
                        },
                        {
                            "Description", "搜索下载"
                        },
                        {
                            "DominantType", "版本: " + node.SelectNodes("Modules/MsnJVData.Module/Components/MsnJVData.ContainerComponent/Components/MsnJVData.TabComponent/TabItemList/MsnJVData.TabItem/ContentList/MsnJVData.ContainerComponent/Components/MsnJVData.DataListComponent/Rows/MsnJVData.ItemList").Cast<XmlNode>().FirstOrDefault(x => x.SelectSingleNode("Label").InnerText =="软件版本").SelectSingleNode("Items/MsnJVData.Item/Text").InnerText
                        }  ,
                    {
                        "Score", "1"
                    }
                    };

                    if (dict["Name"].EndsWith("下载"))
                    {
                        dict["Name"] = dict["Name"].Substring(0, dict["Name"].Length - 2);
                    }

                    dict.Add("SatoriId", Helper.GetSatoriId(dict["Name"]));
                    yield return dict;
                }
            }
        }

        public static IEnumerable<Dictionary<string, string>> ParseTVSeries(string file)
        {
            var doc = new XmlDocument();
            doc.Load(file);

            foreach (XmlNode node in doc.SelectNodes("root/item"))
            {
                if (node != null)
                {
                    var dict = new Dictionary<string, string>
                {
                    {
                        "ThumbnailId",
                        Helper.ExtractThumbnailId(node.SelectSingleNode(
                            "Poster/Url")
                            .InnerText )
                    },
                    {
                        "ThumbnailWidth",
                        node.SelectSingleNode(
                            "Poster/Width")
                            .InnerText
                    },
                    {
                        "ThumbnailHeight",
                        node.SelectSingleNode(
                            "Poster/Height")
                            .InnerText
                    },
                    {
                        "Name",
                        node.SelectSingleNode("Poster/AltText")
                            .InnerText
                    },
                    {
                        "Description",
                        node.SelectSingleNode("AbstractInfo/Text")
                            .InnerText
                    }  ,
                    {
                        "Keywords",
                        node.SelectSingleNode("Keywords")
                            .InnerText
                    } ,
                    {
                        "Score", "4"
                    }
                };
                    var types =
                        node.SelectNodes("MetaInfos/MsnJVData.LabelList")
                            .Cast<XmlNode>()
                            .FirstOrDefault(x => x.SelectSingleNode("Name").InnerText == "类型")
                            .SelectNodes("Values/MsnJVData.TextFragment/Text");
                    dict.Add("DominantType", string.Join("|", types.Cast<XmlNode>().Select(x => x.InnerText)));
                    dict.Add("SatoriId", Helper.GetSatoriId(dict["Name"]));
                    yield return dict;
                }
            }
        }

        public static IEnumerable<Dictionary<string, string>> ParseYesky(string file)
        {
            var doc = new XmlDocument();
            doc.Load(file);

            foreach (XmlNode node in doc.SelectNodes("root/item"))
            {
                if (node != null)
                {
                    var dict = new Dictionary<string, string>
                {
                    {
                        "ThumbnailId",
                        Helper.ExtractThumbnailId(node.SelectSingleNode(
                            "Image/Url")
                            .InnerText )
                    },
                    {
                        "ThumbnailWidth",
                        node.SelectSingleNode(
                            "Image/Width")
                            .InnerText
                    },
                    {
                        "ThumbnailHeight",
                        node.SelectSingleNode(
                            "Image/Height")
                            .InnerText
                    },
                    {
                        "Name",
                        node.SelectSingleNode("Keywords")
                            .InnerText
                    } ,
                    {
                        "Keywords",
                        node.SelectSingleNode("Keywords")
                            .InnerText
                    }    ,
                    {
                        "Description","搜索下载"
                    }    ,
                    {
                        "Score", "2"
                    }
                };

                    var metadata = node.SelectNodes("LabelListsTOP/MsnJVData.LabelList").Cast<XmlNode>();
                    Dictionary<string, string> medatDict = metadata.ToDictionary(x => x.SelectSingleNode("Name").InnerText,
                        x => x.SelectSingleNode("Values/MsnJVData.TextFragment/Text")?.InnerText);
                                                                                                
                    var Keys = new Dictionary<string, string> { { "软件版本" ,"版本"}, { "软件大小", "大小" }, { "更新时间", "更新时间" }, { "运行环境", "运行环境" } };
                    var pair = Keys.FirstOrDefault(x => medatDict.ContainsKey(x.Key) && !string.IsNullOrWhiteSpace(medatDict[x.Key]));
                    dict.Add("DominantType", pair.Value + ": " + medatDict[pair.Key]);
                    dict.Add("SatoriId", Helper.GetSatoriId(dict["Name"]));
                    yield return dict;
                }
            }
        }

        public static IEnumerable<Dictionary<string, string>> ParseEditorial(string file)
        {
            var mapping = new Dictionary<string, string> { { "game", "游戏" } };
            var lines = File.ReadAllLines(file).Select(x => x.Split("\t".ToArray()));
            foreach (var line in lines)
            {
                var dict = new Dictionary<string, string>
                {
                    {
                        "ThumbnailId", line[7]
                    },
                    {
                        "SatoriId", line[6]
                    },
                    {
                        "ThumbnailWidth", line[8]
                    },
                    {
                        "ThumbnailHeight", line[9]
                    },
                    {
                        "Name", line[3]
                    },
                    {
                        "Description", line[5].Trim("\" ".ToArray())
                    },
                    {
                        "Keywords", line[3]
                    },
                    {
                        "DominantType", string.IsNullOrWhiteSpace(line[2]) ? (mapping.ContainsKey(line[1]) ? mapping[line[1]] : null) : line[2]
                    },
                    {
                        "Score", "0"
                    }
                };

                if (string.IsNullOrWhiteSpace(dict["DominantType"]))
                {
                    throw new Exception("No Vaild DominantType for Editorial Entity");
                }

                yield return dict;
            }
        }
    }
      
    public class XapHelper
    {
        internal const string kifMajorVersionAttributeName = "KifMajorVersion";
        internal const string kifMinorVersionAttributeName = "KifMinorVersion";
        internal const string schemaAttributeName = "SchemaName";

        internal static Dictionary<string, Dictionary<string, string>> bondDict = null;
        internal static Dictionary<string, Dictionary<string, string>> BonDict => bondDict ?? (bondDict = InitBondDict());

        public static T CreateInstance<T>() where T : Xap.Identity
        {
            var obj = Activator.CreateInstance<T>();

            SetXapIdentityFields(obj, obj.GetType().FullName);
            SetXapIdentityFieldsForSubProperties(obj);
            return obj;
        }

        public static string ToBase64<T>(T entity) where T : Xap.Identity
        {
            var bytes = XapHelper.SerializeToByteArray(entity);
            return Convert.ToBase64String(bytes);
        }

        internal static void SetXapIdentityFieldsForSubProperties(object obj)
        {
            var type = obj.GetType();
            var propertiesInfo = type.GetProperties();
            foreach (var propertyInfo in propertiesInfo)
            {
                var propertyValue = propertyInfo.GetValue(obj, null);
                if ((propertyValue as Xap.Identity) != null)
                {
                    SetXapIdentityFields(propertyValue as Xap.Identity, propertyValue.GetType().FullName);
                }
            }
        }

        internal static void SetXapIdentityFields(Xap.Identity obj, string fullTypeName)
        {
            Dictionary<string, string> bond;
            if (!BonDict.TryGetValue(fullTypeName, out bond))
            {
                throw new Exception($"Cannot find type {fullTypeName} in the bond dict.");
            }

            string schemaName;
            if (!bond.TryGetValue(schemaAttributeName, out schemaName))
            {
                throw new Exception($"type {fullTypeName} is missing the SchemaName attribute.");
            }
            obj.SchemaName = schemaName;

            string kifMajorVersion;
            if (!bond.TryGetValue(kifMajorVersionAttributeName, out kifMajorVersion))
            {
                throw new Exception($"type {fullTypeName} is missing the KifMajorVersion attribute.");
            }
            obj.KifMajorVersion = Convert.ToByte(kifMajorVersion);

            string kifMinorVersion;
            if (!bond.TryGetValue(kifMinorVersionAttributeName, out kifMinorVersion))
            {
                throw new Exception($"type {fullTypeName} is missing the KifMajorVersion attribute.");
            }
            obj.KifMinorVersion = Convert.ToByte(kifMinorVersion);
        }

        internal static readonly byte[] MarshaledContentMagicBytes = { 0x43, 0x42, 0x01, 0x00 };
        public static byte[] SerializeToByteArray(IBondSerializable obj)
        {
            var buffer = new MemoryStream();
            var writer = new CompactBinaryProtocolWriter(buffer);
            obj.Write(writer);

            buffer.Seek(0, SeekOrigin.Begin);
            var bondContent = buffer.ToArray();

            var marshaledBondContent = new byte[bondContent.Length + MarshaledContentMagicBytes.Length];
            Array.Copy(MarshaledContentMagicBytes, marshaledBondContent, MarshaledContentMagicBytes.Length);
            Array.Copy(bondContent, 0, marshaledBondContent, MarshaledContentMagicBytes.Length, bondContent.Length);

            return marshaledBondContent;
        }

        internal static Dictionary<string, Dictionary<string, string>> InitBondDict()
        {
            var dict = new Dictionary<string, Dictionary<string, string>>();

            var bondAssembly = Assembly.GetAssembly(typeof(Person_2));
            var bondTypes = bondAssembly.GetTypes();
            foreach (var bondType in bondTypes)
            {
                // we assume that all the classes belong to the Entities namespace
                if (!bondType.FullName.StartsWith("Entities"))
                {
                    continue;
                }

                var methodInfo = bondType.GetMethod("GetRuntimeSchema");
                if (methodInfo == null)
                {
                    continue;
                }

                var schema = (SchemaDef)methodInfo.Invoke(null, new object[] { });

                // collect all the bond type and field information
                foreach (var bondTypeDef in schema.structs)
                {
                    var bondTypeFullName = string.Empty/*bondPDNamespace*/ + bondTypeDef.metadata.qualified_name;
                    if (dict.ContainsKey(bondTypeFullName))
                    {
                        continue;
                    }

                    dict.Add(bondTypeFullName, bondTypeDef.metadata.attributes);

                    foreach (var bondFieldDef in bondTypeDef.fields)
                    {
                        // use the fully qualified field name to avoid conflict (the same field name from different types)
                        var bondFieldFullName = bondTypeFullName + "." + bondFieldDef.metadata.name;
                        if (!dict.ContainsKey(bondFieldFullName))
                        {
                            dict.Add(bondFieldFullName, bondFieldDef.metadata.attributes);
                        }
                    }
                }
            }

            return dict;
        }
    }

    public class Helper
    {
        public static string ExtractThumbnailId(string image)
        {
            return Regex.Match(image, "(?<=id=)[^&]+").Value;
        }

        public static string GetSatoriId(string query)
        {
            var url =
                string.Format("http://cn.bing.com/search?q={0}&qs=AS&pq=&cvid=entityforqf&FORM=QBLH&mkt=zh-cn&setflight=",
                    HttpUtility.UrlEncode(query));

            var data = _md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(url));
            var result = new StringBuilder();
            for (var i = 0; i < 16; ++i)
            {
                result.Append(GetCharBit4(data[i] / 16));
                result.Append(GetCharBit4(data[i] % 16));
            }

            result.Insert(8, "-");
            result.Insert(13, "-");
            result.Insert(18, "-");
            result.Insert(23, "-");

            return result.ToString().ToLower();
        }

        public static char GetCharBit4(int dig)
        {
            while (true)
            {
                switch (dig)
                {
                    case 0:
                        return '0';
                    case 1:
                        return '1';
                    case 2:
                        return '2';
                    case 3:
                        return '3';
                    case 4:
                        return '4';
                    case 5:
                        return '5';
                    case 6:
                        return '6';
                    case 7:
                        return '7';
                    case 8:
                        return '8';
                    case 9:
                        return '9';
                    case 10:
                        return 'A';
                    case 11:
                        return 'B';
                    case 12:
                        return 'C';
                    case 13:
                        return 'D';
                    case 14:
                        return 'E';
                    case 15:
                        return 'F';
                    default:
                        dig = dig % 16;
                        continue;
                }
            }
        }
        private static readonly MD5 _md5Hasher = MD5.Create();

        public static Tuple<int, int> GetImgSize(string imgPath)
        {
            var img = Image.FromFile(imgPath);
            return new Tuple<int, int>(img.Width, img.Height);
        }
    }
}
