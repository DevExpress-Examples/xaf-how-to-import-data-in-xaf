Imports System
Imports ImportData
Imports DevExpress.ExpressApp
Imports DevExpress.Persistent.Base
Imports DevExpress.ExpressApp.Actions
Imports DevExpress.Persistent.BaseImpl
Imports DevExpress.ExpressApp.Templates

Namespace WinWebSolution.Module
	''' <summary>
	''' Dennis: This ViewController adds the ImportData action and allows you to import data in ListView.
	''' </summary>
	Public Class ImportDataListViewController
		Inherits ViewController

		Public Const MaxImportedRecordsCount As Integer = 10
		Public Const ActiveKeyImportAction As String = "ActiveKeyImportAction"
		Public Const ActiveKeyImportActionItemRootListViewForPerson As String = "Item is active only in the root ListView for Person"
		Public Const ActiveKeyImportActionItemNestedListViewForPhoneNumber As String = "Item is active only in nested ListView for PhoneNumber"
		Private importDataActionCore As SingleChoiceAction = Nothing

		Public Sub New()
			TargetViewType = ViewType.ListView
			importDataActionCore = CreateImportAction()
		End Sub
		Private Function CreateImportAction() As SingleChoiceAction
'INSTANT VB NOTE: The variable importDataAction was renamed since Visual Basic does not handle local variables named the same as class members well:
			Dim importDataAction_Renamed As New SingleChoiceAction(Me, "ImportData", PredefinedCategory.RecordEdit)
			AddHandler importDataAction_Renamed.Execute, AddressOf importDataAction_Execute
			Dim item1 As New ChoiceActionItem()
			Dim item2 As New ChoiceActionItem()
			importDataAction_Renamed.Caption = "Import Data"
			importDataAction_Renamed.ImageName = "Attention"
			item1.Caption = "In root ListView"
			item1.Data = "Person_ListView"
			item2.Caption = "In nested ListView"
			item2.Data = "Party_PhoneNumbers_ListView"
			importDataAction_Renamed.Items.Add(item1)
			importDataAction_Renamed.Items.Add(item2)
			importDataAction_Renamed.PaintStyle = ActionItemPaintStyle.CaptionAndImage
			importDataAction_Renamed.ItemType = SingleChoiceActionItemType.ItemIsOperation
			Return importDataAction_Renamed
		End Function
		Protected Overridable Sub ImportData(ByVal e As SingleChoiceActionExecuteEventArgs)
			Dim activeItem As ChoiceActionItem = e.SelectedChoiceActionItem
			Dim lv As ListView = CType(View, ListView)
			Dim importManager As New ImportDataManager(Application)
			Select Case activeItem.Data.ToString()
				Case "Person_ListView"
					importManager.ImportData(Of Person)(MaxImportedRecordsCount, ImportDataLogic.CreateCoolPersonImportDataFromXmlFileDelegate(), Nothing, lv, True)
					'Dennis: This line won't be executed unless you handle the exception thrown from the previus ImportData call.
					'ImportDataManager.ImportData<Person>(MaxImportedRecordsCount, ImportDataLogic.CreateDummyPersonImportDataDelegate(), ImportDataLogic.CreateDummyPersonValidateDataDelegate(), lv, true);
				Case "Party_PhoneNumbers_ListView"
					Dim pcs As PropertyCollectionSource = TryCast(lv.CollectionSource, PropertyCollectionSource)
					If pcs IsNot Nothing Then
						importManager.ImportData(Of PhoneNumber)(MaxImportedRecordsCount, ImportDataLogic.CreateDummyPhoneNumberImportDataDelegate(), ImportDataLogic.CreateDummyPhoneNumberValidateDataDelegate(), lv, True)
					End If
			End Select
		End Sub
		Private Sub importDataAction_Execute(ByVal sender As Object, ByVal e As SingleChoiceActionExecuteEventArgs)
			ImportData(e)
		End Sub
		Protected Overrides Sub UpdateActionActivity(ByVal action As ActionBase)
			MyBase.UpdateActionActivity(action)
			Dim rootListViewForPersonCondition As Boolean = View.IsRoot AndAlso TypeOf View Is ListView AndAlso View.ObjectTypeInfo.Type Is GetType(Person)
			Dim nestedListViewForPhoneNumberCondition As Boolean = (Not View.IsRoot) AndAlso TypeOf View Is ListView AndAlso View.ObjectTypeInfo.Type Is GetType(PhoneNumber)
			ImportDataAction.Active(ActiveKeyImportAction) = rootListViewForPersonCondition OrElse nestedListViewForPhoneNumberCondition
			ImportDataAction.Items(0).Active(ActiveKeyImportActionItemRootListViewForPerson) = rootListViewForPersonCondition
			ImportDataAction.Items(1).Active(ActiveKeyImportActionItemNestedListViewForPhoneNumber) = nestedListViewForPhoneNumberCondition
		End Sub
		Public ReadOnly Property ImportDataAction() As SingleChoiceAction
			Get
				Return importDataActionCore
			End Get
		End Property
	End Class
End Namespace