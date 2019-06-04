Imports System
Imports DevExpress.Xpo
Imports DevExpress.ExpressApp
Imports DevExpress.ExpressApp.Utils
Imports DevExpress.Persistent.Validation

Namespace ImportData
	''' <summary>
	''' Dennis: This delegate should be used to create persistent objects during data import. Use the IObjectSpace and args parameters to create new persistent objects.
	''' By default, if the ListView is a nested and its objects are a part of an association, the first element of the args array will contain the reference to the master object of an association.
	''' The second element of the args array usually contains the index of the imported object.
	''' </summary>
	Public Delegate Function ImportDataDelegate(Of T)(ByVal objectSpace As IObjectSpace, <[ParamArray]> ByVal args() As Object) As T


	Public Delegate Function ValidateDataDelegate(Of T)(ByVal data As T, <[ParamArray]> ByVal args() As Object) As Boolean
	''' <summary>
	''' Dennis: This class provides a generic logic to import and validate persistent objects in ListView.
	''' </summary>
	Public Class ImportDataManager
		Private app As XafApplication
		Public Sub New(ByVal application As XafApplication)
			Guard.ArgumentNotNull(application, "application")
			app = application
		End Sub
		Public ReadOnly Property Application() As XafApplication
			Get
				Return app
			End Get
		End Property
		''' <summary>
		''' This method adds imported objects into the required ListView.
		''' </summary>
		''' <typeparam name="T">Any persistent object type supporting the IXPSimpleObject interface.</typeparam>
		''' <param name="count">The count of of objects to be imported.</param>
		''' <param name="importDataDelegate">A function of the ImportDataDelegate type that can create persistent objects.</param>
		''' <param name="customValidateDataDelegate">A function of the ValidateDataDelegate type that can be used to additionally validated created persistent objects. Note that by default, the created objects are already validated using the XAF's Validation system. So, you can set this parameter to null or Nothing in VB.NET</param>
		''' <param name="targetListView">A ListView to which objects will be imported.</param>
		''' <returns>The real count of imported objects.</returns>
		''' Dennis: This method reuses the UnitOfWork/NestedUnitOfWork classes, but it's also correct to use independent (that doesn't belong to a View) ObjectSpace/NestedObjectSpace classes here.
		Public Function ImportData(Of T As IXPSimpleObject)(ByVal count As Integer, ByVal importDataDelegate As ImportDataDelegate(Of T), ByVal customValidateDataDelegate As ValidateDataDelegate(Of T), ByVal targetListView As ListView) As Integer
			If targetListView Is Nothing Then
				Throw New ArgumentNullException("targetlistView")
			End If
			Dim importObjectSpace As IObjectSpace = Nothing
			Dim countOK As Integer = 0
			Try
				importObjectSpace = Application.CreateObjectSpace(targetListView.CollectionSource.ObjectTypeInfo.Type)
				For i As Integer = 0 To count - 1
					Using nestedImportObjectSpace As IObjectSpace = importObjectSpace.CreateNestedObjectSpace()
						Dim pcs As PropertyCollectionSource = TryCast(targetListView.CollectionSource, PropertyCollectionSource)
						Dim masterObject As Object = If(pcs IsNot Nothing, nestedImportObjectSpace.GetObjectByKey(pcs.MasterObjectType, nestedImportObjectSpace.GetKeyValue(pcs.MasterObject)), Nothing)
						Dim obj As T = importDataDelegate(nestedImportObjectSpace, masterObject, i)
						If obj IsNot Nothing Then
							Dim isValid As Boolean = False
							Try
								Dim validationResult As RuleSetValidationResult = Validator.RuleSet.ValidateTarget(Nothing, obj, New ContextIdentifiers(DefaultContexts.Save.ToString()))
								isValid = validationResult.State <> ValidationState.Invalid AndAlso (If(customValidateDataDelegate IsNot Nothing, customValidateDataDelegate(obj, masterObject, i), True))
								If isValid Then
									nestedImportObjectSpace.CommitChanges()
								End If
							Catch e1 As Exception
								isValid = False
							Finally
								If isValid Then
									countOK += 1
								End If
							End Try
						End If
					End Using
				Next i
				importObjectSpace.CommitChanges()
				targetListView.ObjectSpace.Refresh()
			Catch commitException As Exception
				Try
					importObjectSpace.Rollback()
				Catch rollBackException As Exception
					Throw New Exception(String.Format("An exception of type {0} was encountered while attempting to roll back the transaction while importing the data of the {1} type." & vbLf & "Error Message:{2}" & vbLf & "StackTrace:{3}", rollBackException.GetType(), GetType(T), rollBackException.Message, rollBackException.StackTrace), rollBackException)
				End Try
				Throw New UserFriendlyException(String.Format("Importing can't be finished!" & vbLf & "An exception of type {0} was encountered while importing the data of the {1} type." & vbLf & "Error message = {2}" & vbLf & "StackTrace:{3}" & vbLf & "No records were imported.", commitException.GetType(), GetType(T), commitException.Message, commitException.StackTrace))
			Finally
				importObjectSpace.Dispose()
				importObjectSpace = Nothing
			End Try
			Return countOK
		End Function
		''' <summary>
		''' This method adds imported objects into the required ListView.
		''' </summary>
		''' <typeparam name="T">Any persistent object type supporting the IXPSimpleObject interface.</typeparam>
		''' <param name="count">The count of of objects to be imported.</param>
		''' <param name="importDataDelegate">A function of the ImportDataDelegate type that can create persistent objects.</param>
		''' <param name="customValidateDataDelegate">A function of the ValidateDataDelegate type that can be used to additionally validated created persistent objects. Note that by default, the created objects are already validated using the XAF's Validation system. So, you can set this parameter to null or Nothing in VB.NET</param>
		''' <param name="targetListView">A ListView to which objects will be imported.</param>
		''' <param name="notifyAboutResults">Caution!!! If this parameter is set to True, the UserFriendlyException will be thrown to show a platform-independent message box. But the code that follows after this method call won't be executed unless you catch the thrown exception.</param>
		Public Sub ImportData(Of T As IXPSimpleObject)(ByVal count As Integer, ByVal importDataDelegate As ImportDataDelegate(Of T), ByVal customValidateDataDelegate As ValidateDataDelegate(Of T), ByVal targetListView As ListView, ByVal notifyAboutResults As Boolean)
			Dim countOK As Integer = ImportData(Of T)(count, importDataDelegate, customValidateDataDelegate, targetListView)
			If notifyAboutResults Then 'B187848 - Web - UserFriendlyException is unhandled when it is thrown from an Action after the ObjectSpace.Refresh method was called
				Throw New UserFriendlyException(String.Format("Finished! {0} data record(s) has/have been imported.", countOK))
			End If
		End Sub
	End Class
End Namespace