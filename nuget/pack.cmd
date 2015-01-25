copy ..\src\lightningbug\bin\release\lightningbug.dll lib\net45
copy ..\src\lightningbug\bin\release\lightningbug.pdb lib\net45
copy ..\src\lightningbug\bin\release\lightningbug.xml lib\net45
..\src\.nuget\nuget.exe pack lightningbug.nuspec
