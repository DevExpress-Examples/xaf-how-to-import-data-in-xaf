using System;
using DevExpress.Xpo;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Validation;

namespace ImportData {
    /// <summary>
    /// Dennis: This delegate should be used to create persistent objects during data import. Use the IObjectSpace and args parameters to create new persistent objects.
    /// By default, if the ListView is a nested and its objects are a part of an association, the first element of the args array will contain the reference to the master object of an association.
    /// The second element of the args array usually contains the index of the imported object.
    /// </summary>
    public delegate T ImportDataDelegate<T>(IObjectSpace objectSpace, params object[] args);


    public delegate bool ValidateDataDelegate<T>(T data, params object[] args);
    /// <summary>
    /// Dennis: This class provides a generic logic to import and validate persistent objects in ListView.
    /// </summary>
    public class ImportDataManager {
        private XafApplication app;
        public ImportDataManager(XafApplication application) {
            Guard.ArgumentNotNull(application, "application");
            app = application;
        }
        public XafApplication Application { get { return app; } }
        /// <summary>
        /// This method adds imported objects into the required ListView.
        /// </summary>
        /// <typeparam name="T">Any persistent object type supporting the IXPSimpleObject interface.</typeparam>
        /// <param name="count">The count of of objects to be imported.</param>
        /// <param name="importDataDelegate">A function of the ImportDataDelegate type that can create persistent objects.</param>
        /// <param name="customValidateDataDelegate">A function of the ValidateDataDelegate type that can be used to additionally validated created persistent objects. Note that by default, the created objects are already validated using the XAF's Validation system. So, you can set this parameter to null or Nothing in VB.NET</param>
        /// <param name="targetListView">A ListView to which objects will be imported.</param>
        /// <returns>The real count of imported objects.</returns>
        /// Dennis: This method reuses the UnitOfWork/NestedUnitOfWork classes, but it's also correct to use independent (that doesn't belong to a View) ObjectSpace/NestedObjectSpace classes here.
        public int ImportData<T>(int count, ImportDataDelegate<T> importDataDelegate, ValidateDataDelegate<T> customValidateDataDelegate, ListView targetListView) where T : IXPSimpleObject {
            if (targetListView == null) {
                throw new ArgumentNullException("targetlistView");
            }
            IObjectSpace importObjectSpace = null;
            int countOK = 0;
            try {
                importObjectSpace = Application.CreateObjectSpace(targetListView.CollectionSource.ObjectTypeInfo.Type);
                for (int i = 0; i < count; i++) {
                    using (IObjectSpace nestedImportObjectSpace = importObjectSpace.CreateNestedObjectSpace()) {
                        PropertyCollectionSource pcs = targetListView.CollectionSource as PropertyCollectionSource;
                        object masterObject = pcs != null ? nestedImportObjectSpace.GetObjectByKey(pcs.MasterObjectType, nestedImportObjectSpace.GetKeyValue(pcs.MasterObject)) : null;
                        T obj = importDataDelegate(nestedImportObjectSpace, masterObject, i);
                        if (obj != null) {
                            bool isValid = false;
                            try {
                                RuleSetValidationResult validationResult = Validator.RuleSet.ValidateTarget(null, obj, new ContextIdentifiers(DefaultContexts.Save.ToString()));
                                isValid = validationResult.State != ValidationState.Invalid && (customValidateDataDelegate != null ? customValidateDataDelegate(obj, masterObject, i) : true);
                                if (isValid) {
                                    nestedImportObjectSpace.CommitChanges();
                                }
                            } catch (Exception) {
                                isValid = false;
                            } finally {
                                if (isValid) {
                                    countOK++;
                                }
                            }
                        }
                    }
                }
                importObjectSpace.CommitChanges();
                targetListView.ObjectSpace.Refresh();
            } catch (Exception commitException) {
                try {
                    importObjectSpace.Rollback();
                } catch (Exception rollBackException) {
                    throw new Exception(String.Format("An exception of type {0} was encountered while attempting to roll back the transaction while importing the data of the {1} type.\nError Message:{2}\nStackTrace:{3}", rollBackException.GetType(), typeof(T), rollBackException.Message, rollBackException.StackTrace), rollBackException);
                }
                throw new UserFriendlyException(String.Format("Importing can't be finished!\nAn exception of type {0} was encountered while importing the data of the {1} type.\nError message = {2}\nStackTrace:{3}\nNo records were imported.", commitException.GetType(), typeof(T), commitException.Message, commitException.StackTrace));
            } finally {
                importObjectSpace.Dispose();
                importObjectSpace = null;
            }
            return countOK;
        }
        /// <summary>
        /// This method adds imported objects into the required ListView.
        /// </summary>
        /// <typeparam name="T">Any persistent object type supporting the IXPSimpleObject interface.</typeparam>
        /// <param name="count">The count of of objects to be imported.</param>
        /// <param name="importDataDelegate">A function of the ImportDataDelegate type that can create persistent objects.</param>
        /// <param name="customValidateDataDelegate">A function of the ValidateDataDelegate type that can be used to additionally validated created persistent objects. Note that by default, the created objects are already validated using the XAF's Validation system. So, you can set this parameter to null or Nothing in VB.NET</param>
        /// <param name="targetListView">A ListView to which objects will be imported.</param>
        /// <param name="notifyAboutResults">Caution!!! If this parameter is set to True, the UserFriendlyException will be thrown to show a platform-independent message box. But the code that follows after this method call won't be executed unless you catch the thrown exception.</param>
        public void ImportData<T>(int count, ImportDataDelegate<T> importDataDelegate, ValidateDataDelegate<T> customValidateDataDelegate, ListView targetListView, bool notifyAboutResults) where T : IXPSimpleObject {
            int countOK = ImportData<T>(count, importDataDelegate, customValidateDataDelegate, targetListView);
            if (notifyAboutResults)//B187848 - Web - UserFriendlyException is unhandled when it is thrown from an Action after the ObjectSpace.Refresh method was called
                throw new UserFriendlyException(String.Format("Finished! {0} data record(s) has/have been imported.", countOK));
        }
    }
}