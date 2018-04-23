#DropDB E1719
#Application E1719Win
#Application E1719Web

*Action Import Data(In root ListView)

#IfDef E1719Win
*HandleDialog
 Message = Finished!*
 Respond = OK
#EndIf
#IfDef E1719Web
*CheckValidationResult
 Message = Finished!*
#EndIf


*CheckTable
 Columns = First Name
 Row[0] = FirstName*
 
*ProcessRecord
 First Name = FirstName1

*Action Phone Numbers.Import Data(In nested ListView)
#IfDef E1719Win
*HandleDialog
 Message = Finished!*
 Respond = OK
#EndIf
#IfDef E1719Web
*CheckValidationResult
 Message = Finished!*
#EndIf

*CheckTable
 Columns = Number
 Row[0] = Number*