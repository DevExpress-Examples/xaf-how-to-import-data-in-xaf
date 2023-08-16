using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;

using System.IO;
using System.ComponentModel;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System.Xml;
using ImportDataEF.Module.BusinessObjects;

namespace ImportDataEF.Module.Controllers {
    public class ImportDataListViewController : ViewController<ListView> {

        public ImportDataListViewController() {
            var importInMainView = new SimpleAction(this, "ImportInMainView", PredefinedCategory.RecordEdit);
            importInMainView.TargetViewNesting = Nesting.Root;
            importInMainView.Execute += ImportInMainView_Execute;
        }



        private void ImportInMainView_Execute(object sender, SimpleActionExecuteEventArgs e) {
            ImportDataMain(View);
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
        public void ImportDataMain(ListView targetListView) {
            if (targetListView == null) {
                throw new ArgumentNullException("targetlistView");
            }
            IObjectSpace importObjectSpace = null;
            try {
                importObjectSpace = Application.CreateObjectSpace(targetListView.CollectionSource.ObjectTypeInfo.Type);
                using (IObjectSpace nestedImportObjectSpace = importObjectSpace.CreateNestedObjectSpace()) {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(GetXmlString());

                    foreach (XmlNode element in doc.ChildNodes[0].ChildNodes) {
                        Contact person = nestedImportObjectSpace.CreateObject<Contact>();
                        person.FirstName = String.Format("FirstName{0}", element.ChildNodes[0].InnerText);
                        person.LastName = String.Format("LastName{0}", element.ChildNodes[2].InnerText);

                    };
                    nestedImportObjectSpace.CommitChanges();
                }
                importObjectSpace.CommitChanges();
                targetListView.ObjectSpace.Refresh();
            }
            catch (Exception commitException) {
                try {
                    importObjectSpace.Rollback();
                }
                catch (Exception rollBackException) {
                    throw new Exception(String.Format("An exception of type {0} was encountered while attempting to roll back the transaction while importing the data of the {1} type.\nError Message:{2}\nStackTrace:{3}", rollBackException.GetType(), View.ObjectTypeInfo.Type, rollBackException.Message, rollBackException.StackTrace), rollBackException);
                }
                throw new UserFriendlyException(String.Format("Importing can't be finished!\nAn exception of type {0} was encountered while importing the data of the {1} type.\nError message = {2}\nStackTrace:{3}\nNo records were imported.", commitException.GetType(), View.ObjectTypeInfo.Type, commitException.Message, commitException.StackTrace));
            }
            finally {
                importObjectSpace.Dispose();
                importObjectSpace = null;
            }
        }

    }
}
