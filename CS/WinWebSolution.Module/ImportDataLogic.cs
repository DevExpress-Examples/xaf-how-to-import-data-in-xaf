using System;
using System.Xml;
using ImportData;
using DevExpress.ExpressApp;
using DevExpress.Persistent.BaseImpl;

namespace WinWebSolution.Module {
    /// <summary>
    /// Dennis: This class contains the logic of data import for each class. For instance, you may code here the algorithm of importing data objects from Excel using a popular and free FileHelpers library.
    /// </summary>
    public class ImportDataLogic {
        public static ValidateDataDelegate<PhoneNumber> CreateDummyPhoneNumberValidateDataDelegate() {
            return delegate(PhoneNumber phoneNumber, object[] args) {
                if (phoneNumber != null) {
                    return phoneNumber.GetHashCode() % 2 == 0;
                }
                return false;
            };
        }
        public static ValidateDataDelegate<Person> CreateDummyPersonValidateDataDelegate() {
            return delegate(Person person, object[] args) {
                if (person != null) {
                    return person.GetHashCode() % 2 == 0;
                }
                return false;
            };
        }
        public static ImportDataDelegate<PhoneNumber> CreateDummyPhoneNumberImportDataDelegate() {
            return delegate(IObjectSpace os, object[] args) {
                object masterObject = null;
                int index = 0;
                if (args != null) {
                    if (args.Length == 2) {
                        masterObject = args[0];
                        index = Convert.ToInt32(args[1]);
                    }
                }
                PhoneNumber phoneNumber = os.CreateObject<PhoneNumber>();
                phoneNumber.Number = String.Format("Number{0}", index);
                phoneNumber.PhoneType = String.Format("PhoneType{0}", index);
                phoneNumber.Party = masterObject as Party;
                return phoneNumber;
            };
        }
        public static ImportDataDelegate<Person> CreateDummyPersonImportDataDelegate() {
            return delegate(IObjectSpace os, object[] args) {
                object masterObject = null;
                int index = 0;
                if (args != null) {
                    if (args.Length == 2) {
                        masterObject = args[0];
                        index = Convert.ToInt32(args[1]);
                    }
                }
                Person person = os.CreateObject<Person>();
                person.FirstName = String.Format("FirstName{0}", index);
                person.MiddleName = String.Format("MiddleName{0}", index);
                person.LastName = String.Format("LastName{0}", index);
                return person;
            };
        }
        public static ImportDataDelegate<Person> CreateCoolPersonImportDataFromExcelDelegate() {
            return delegate(IObjectSpace os, object[] args) {
                throw new NotImplementedException("TODO: you can use the FileHelpers library to get the data from Excel and then fill your persistent object.");
            };
        }
        public static ImportDataDelegate<Person> CreateCoolPersonImportDataFromXmlFileDelegate() {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(GetXmlString());
            return delegate(IObjectSpace os, object[] args) {
                object masterObject = null;
                int index = 0;
                if (args != null) {
                    if (args.Length == 2) {
                        masterObject = args[0];
                        index = Convert.ToInt32(args[1]);
                    }
                }
                if (index < doc.DocumentElement.ChildNodes.Count) {
                    Person person = os.CreateObject<Person>();
                    person.FirstName = String.Format("FirstName{0}", doc.DocumentElement.ChildNodes[index].ChildNodes[0].InnerText);
                    person.MiddleName = String.Format("MiddleName{0}", doc.DocumentElement.ChildNodes[index].ChildNodes[1].InnerText);
                    person.LastName = String.Format("LastName{0}", doc.DocumentElement.ChildNodes[index].ChildNodes[2].InnerText);
                    return person;
                }
                return null;
            };
        }
        private static string GetXmlString() {
            string xml = @"
            <Persons>
            	<Person>
            		<FirstName>1</FirstName>
            		<MiddleName>1</MiddleName>
            		<LastName>1</LastName>
            	</Person>
                <Person>
		            <FirstName>2</FirstName>
		            <MiddleName>2</MiddleName>
		            <LastName>2</LastName>
	            </Person>
	            <Person>
		            <FirstName>3</FirstName>
		            <MiddleName>3</MiddleName>
		            <LastName>3</LastName>
	            </Person>
            </Persons>
            ";
            return xml;
        }
    }
}