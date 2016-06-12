# coding=utf-8

import xml.etree.ElementTree
import xml.dom.minidom 
import io

def testNodeFromXml(xmlStr):
    # xml from string/file
    #doc = xml.etree.ElementTree.parse(xmlFile) 
    doc = xml.etree.ElementTree.fromstring(xmlStr)
     
     #xml node lookup
    for node in doc.findall("item/data"):  
        print("data list: tag:%s\ttext:%s\tattrib:%s"%(node.tag, node.text, node.attrib)) 
        
    for node in doc.findall("item/data/value"):  
        print("value list: tag:%s\ttext:%s\tattrib:%s"%(node.tag, node.text, node.attrib)) 
 
    for node in doc.findall("item/*"):  #all item children
        print("* list: tag:%s\ttext:%s\tattrib:%s"%(node.tag, node.text, node.attrib))
     
    for node in doc.findall(".//value"): #all value nodes of doc node, including children/descendants
        print("*b list: tag:%s\ttext:%s\tattrib:%s"%(node.tag, node.text, node.attrib))
        
    dat = doc.find("item/data/value")
    print("dat node: tag:%s\ttext:%s\tattrib:%s"%(dat.tag, dat.text, dat.attrib))     
    print("dat test: text:%s"%(doc.findtext("item/data2")))
     
    dat=doc.find("item/data[1]/value[2]") #index start with 1
    print("data[1][2] node: tag:%s\ttext:%s\tattrib:%s"%(dat.tag, dat.text, dat.attrib)) 
     
     #xml update
    new = xml.etree.ElementTree.Element("append",{"key":"value"}) #append new node
    item=doc.find("item")
    item.append(new)
    
    print(xml.etree.ElementTree.tostring(doc, encoding="utf-8")) #xml => string
    
    new.text = "appendValue"    #insert new dode
    new.set("key", "value0")    
    new.set("attr1", "value1")
    doc.find("item/data[1]").insert(1, new)
    
    item.remove(doc.find("item/data2")) #remove dode
    
    dat.text= "insert2" #update dode
    dat.set("attr", "v2")
    
    rawStr = xml.etree.ElementTree.tostring(doc, encoding="utf-8")    #format xml
    reparsed = xml.dom.minidom.parseString(rawStr)  
    print(reparsed.toprettyxml(indent="    " , newl="\n", encoding="utf-8"))
    file = open("myfile.xml", "w", encoding = "utf-8")
    reparsed.writexml(file, addindent='    ', newl='\n', encoding = 'utf-8')     #write xml

         
if __name__=="__main__":
    xmlStr='<root><item><data version="1.0" url="http://*1*"><value>1</value><value attr="v">2</value></data><data version="2.0" url="http://*2*"><value>3</value></data><data2>test</data2></item></root>'
    testNodeFromXml(xmlStr)