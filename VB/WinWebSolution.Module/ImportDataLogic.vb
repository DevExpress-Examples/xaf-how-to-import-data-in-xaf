Imports System
Imports System.Xml
Imports ImportData
Imports DevExpress.ExpressApp
Imports DevExpress.Persistent.BaseImpl

Namespace WinWebSolution.Module
	''' <summary>
	''' Dennis: This class contains the logic of data import for each class. For instance, you may code here the algorithm of importing data objects from Excel using a popular and free FileHelpers library.
	''' </summary>
	Public Class ImportDataLogic
		Public Shared Function CreateDummyPhoneNumberValidateDataDelegate() As ValidateDataDelegate(Of PhoneNumber)
			Return Function(phoneNumber As PhoneNumber, args() As Object)
				If phoneNumber IsNot Nothing Then
					Return phoneNumber.GetHashCode() Mod 2 = 0
				End If
				Return False
			End Function
		End Function
		Public Shared Function CreateDummyPersonValidateDataDelegate() As ValidateDataDelegate(Of Person)
			Return Function(person As Person, args() As Object)
				If person IsNot Nothing Then
					Return person.GetHashCode() Mod 2 = 0
				End If
				Return False
			End Function
		End Function
		Public Shared Function CreateDummyPhoneNumberImportDataDelegate() As ImportDataDelegate(Of PhoneNumber)
			Return Function(os As IObjectSpace, args() As Object)
				Dim masterObject As Object = Nothing
				Dim index As Integer = 0
				If args IsNot Nothing Then
					If args.Length = 2 Then
						masterObject = args(0)
						index = Convert.ToInt32(args(1))
					End If
				End If
				Dim phoneNumber As PhoneNumber = os.CreateObject(Of PhoneNumber)()
				phoneNumber.Number = String.Format("Number{0}", index)
				phoneNumber.PhoneType = String.Format("PhoneType{0}", index)
				phoneNumber.Party = TryCast(masterObject, Party)
				Return phoneNumber
			End Function
		End Function
		Public Shared Function CreateDummyPersonImportDataDelegate() As ImportDataDelegate(Of Person)
			Return Function(os As IObjectSpace, args() As Object)
				Dim masterObject As Object = Nothing
				Dim index As Integer = 0
				If args IsNot Nothing Then
					If args.Length = 2 Then
						masterObject = args(0)
						index = Convert.ToInt32(args(1))
					End If
				End If
				Dim person As Person = os.CreateObject(Of Person)()
				person.FirstName = String.Format("FirstName{0}", index)
				person.MiddleName = String.Format("MiddleName{0}", index)
				person.LastName = String.Format("LastName{0}", index)
				Return person
			End Function
		End Function
		Public Shared Function CreateCoolPersonImportDataFromExcelDelegate() As ImportDataDelegate(Of Person)
            Return Function(os As IObjectSpace, args() As Object)
            	Throw New NotImplementedException("TODO: you can use the FileHelpers library to get the data from Excel and then fill your persistent object.")
			End Function
        End Function
        Public Shared Function CreateCoolPersonImportDataFromXmlFileDelegate() As ImportDataDelegate(Of Person)
            Dim doc As New XmlDocument()
            doc.LoadXml(GetXmlString())
            Return Function(os As IObjectSpace, args() As Object)
                       Dim masterObject As Object = Nothing
                       Dim index As Integer = 0
                       If args IsNot Nothing Then
                           If args.Length = 2 Then
                               masterObject = args(0)
                               index = Convert.ToInt32(args(1))
                           End If
                       End If
                       If index < doc.DocumentElement.ChildNodes.Count Then
                           Dim person As Person = os.CreateObject(Of Person)()
                           person.FirstName = String.Format("FirstName{0}", doc.DocumentElement.ChildNodes(index).ChildNodes(0).InnerText)
                           person.MiddleName = String.Format("MiddleName{0}", doc.DocumentElement.ChildNodes(index).ChildNodes(1).InnerText)
                           person.LastName = String.Format("LastName{0}", doc.DocumentElement.ChildNodes(index).ChildNodes(2).InnerText)
                           Return person
                       End If
                       Return Nothing
                   End Function
        End Function
		Private Shared Function GetXmlString() As String
			Dim xml As String = "" & ControlChars.CrLf & "            <Persons>" & ControlChars.CrLf & "            	<Person>" & ControlChars.CrLf & "            		<FirstName>1</FirstName>" & ControlChars.CrLf & "            		<MiddleName>1</MiddleName>" & ControlChars.CrLf & "            		<LastName>1</LastName>" & ControlChars.CrLf & "            	</Person>" & ControlChars.CrLf & "                <Person>" & ControlChars.CrLf & "		            <FirstName>2</FirstName>" & ControlChars.CrLf & "		            <MiddleName>2</MiddleName>" & ControlChars.CrLf & "		            <LastName>2</LastName>" & ControlChars.CrLf & "	            </Person>" & ControlChars.CrLf & "	            <Person>" & ControlChars.CrLf & "		            <FirstName>3</FirstName>" & ControlChars.CrLf & "		            <MiddleName>3</MiddleName>" & ControlChars.CrLf & "		            <LastName>3</LastName>" & ControlChars.CrLf & "	            </Person>" & ControlChars.CrLf & "            </Persons>" & ControlChars.CrLf & "            "
			Return xml
		End Function
	End Class
End Namespace