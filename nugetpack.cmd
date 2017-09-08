copy src\lightningbug\bin\release\lightningbug.dll nuget\lib\net45
copy src\lightningbug\bin\release\lightningbug.pdb nuget\lib\net45
copy src\lightningbug\bin\release\lightningbug.xml nuget\lib\net45
src\.nuget\nuget.exe pack nuget\lightningbug.nuspec
