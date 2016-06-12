
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ScopeRuntime;
using System.Linq;
using System.Xml.Linq;
using System.Xml;
using Newtonsoft.Json.Linq;

public class NavProcessor : Processor
{
    public override Schema Produces(string[] columns, string[] args, Schema input)
    {
        return new Schema("Query,Flight,PartnerData");
    }

    public override IEnumerable<Row> Process(RowSet input, Row output, string[] args)
    {
        var valid = false;
        foreach (Row row in input.Rows)
        {
            valid = false;
            try
            {
                var conf = double.Parse(row["conf"].String);
                  var json = JObject.Parse(row["taskframe"].String);
                        var action = json["Uri"].ToString();

                var data =
                    string.Format(
                        "<Cortana10><CortanaItemList><CortanaItem><Name>{0}</Name><DefinitionList><Definition><ActionUri>{1}</ActionUri><TaskFrame>{2}</TaskFrame><Confidence>{3}</Confidence><Type>Cortana Cat 1</Type></Definition></DefinitionList></CortanaItem></CortanaItemList></Cortana10>",
                        row["result"].String, action, row["taskframe"].String, conf);

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(data);
                var partnerData = xmlDoc.OuterXml;

                output["Query"].UnsafeSet(row["query"].String);
                output["Flight"].UnsafeSet("default");
                output["PartnerData"].UnsafeSet(partnerData);
                valid = true;
            }
            catch
            {
                valid = false;
            }

            if (valid)
            {   
                yield return output;
            }
        }
    }
}
