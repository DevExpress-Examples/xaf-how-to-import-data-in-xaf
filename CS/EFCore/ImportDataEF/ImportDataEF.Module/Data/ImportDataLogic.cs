using DevExpress.ExpressApp;
using DevExpress.Persistent.BaseImpl;
using ImportDataEF.Module.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ImportDataEF.Module {
    public class ImportDataLogic {

        public static ImportDataDelegate CreateDummyPhoneNumberImportDataDelegate() {
            return delegate (IObjectSpace os, object masterObject) {
                for (int i = 0; i < 3; i++) {
                    MyTask phoneNumber = os.CreateObject<MyTask>();
                    phoneNumber.Subject = "Subject" + i;
                    phoneNumber.AssignedTo = masterObject as Contact;
                }
            };
        }


        public static ImportDataDelegate CreateCoolPersonImportDataFromXmlFileDelegate() {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(GetXmlString());
            return delegate (IObjectSpace os, object masterObject) {
                foreach (XmlNode element in doc.ChildNodes[0].ChildNodes) {
                    Contact person = os.CreateObject<Contact>();
                    person.FirstName = String.Format("FirstName{0}", element.ChildNodes[0].InnerText);
                    person.LastName = String.Format("LastName{0}", element.ChildNodes[2].InnerText);

                };
            };
        }
        private static string GetXmlString() {
            string xml = @"
            <Contacts>
            	<Contact>
            		<FirstName>1</FirstName>
            		<MiddleName>1</MiddleName>
            		<LastName>1</LastName>
            	</Contact>
                <Contact>
		            <FirstName>2</FirstName>
		            <MiddleName>2</MiddleName>
		            <LastName>2</LastName>
	            </Contact>
	            <Contact>
		            <FirstName>3</FirstName>
		            <MiddleName>3</MiddleName>
		            <LastName>3</LastName>
	            </Contact>
            </Contacts>
            ";
            return xml;
        }
    }
}
