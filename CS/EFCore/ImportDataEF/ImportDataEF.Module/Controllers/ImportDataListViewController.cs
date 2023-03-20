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

namespace ImportDataEF.Module.Controllers {
    public class ImportDataListViewController : ViewController<ListView> {

        public ImportDataListViewController() {

            var importInMainView = new SimpleAction(this, "ImportInMainView", PredefinedCategory.RecordEdit);
            importInMainView.TargetViewNesting = Nesting.Root;
            importInMainView.Execute += ImportInMainView_Execute;

            var importInNestedView = new SimpleAction(this, "ImportInNestedView", PredefinedCategory.RecordEdit);
            importInNestedView.TargetViewNesting = Nesting.Nested;
            importInNestedView.Execute += ImportInNestedView_Execute;

        }

        private void ImportInNestedView_Execute(object sender, SimpleActionExecuteEventArgs e) {

            ListView lv = (ListView)View;
            PropertyCollectionSource pcs = lv.CollectionSource as PropertyCollectionSource;
            if (pcs != null) {
                ImportData(ImportDataLogic.CreateDummyPhoneNumberImportDataDelegate(), View);
            }
        }

        private void ImportInMainView_Execute(object sender, SimpleActionExecuteEventArgs e) {
            ImportData(ImportDataLogic.CreateCoolPersonImportDataFromXmlFileDelegate(), View);
        }

        public void ImportData(ImportDataDelegate importDataDelegate, ListView targetListView) {
            if (targetListView == null) {
                throw new ArgumentNullException("targetlistView");
            }
            IObjectSpace importObjectSpace = null;
            try {
                importObjectSpace = Application.CreateObjectSpace(targetListView.CollectionSource.ObjectTypeInfo.Type);
                using (IObjectSpace nestedImportObjectSpace = importObjectSpace.CreateNestedObjectSpace()) {
                    PropertyCollectionSource pcs = targetListView.CollectionSource as PropertyCollectionSource;
                    object masterObject = pcs != null ? nestedImportObjectSpace.GetObjectByKey(pcs.MasterObjectType, nestedImportObjectSpace.GetKeyValue(pcs.MasterObject)) : null;
                    importDataDelegate(nestedImportObjectSpace, masterObject);
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
