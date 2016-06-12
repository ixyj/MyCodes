@echo off
set sgRoot=D:\Branches\SearchGold_ux_dev
set uxRoot=D:\Branches\coreux_working\

:: xml must in $searchGold_ux_dev\deploy\builds\data\uxtest\excelsior\XmlBags
set xml=Suggestions.xml
::if xml=XmlBags\Suggestions.xml
::set binary=BinaryBags%xml:~7,-1%

if not "%sgRoot:~-1%"=="\" set sgRoot=%sgRoot%\
if not "%uxRoot:~-1%"=="\" set uxRoot=%uxRoot%\

set binary=%xml:~0,-3%bin

%sgRoot%tools\vlad\EnforceBinaryBagsForExcelsior\bin\XmlBagConverter.exe %sgRoot%deploy\builds\data\uxtest\excelsior\XmlBags\%xml%
copy /y %sgRoot%deploy\builds\data\uxtest\excelsior\BinaryBags\%binary% %uxRoot%private\frontend\BagRepository\Data\BinaryBags\%binary%
