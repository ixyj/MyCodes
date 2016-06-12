@echo off
::版权所有    徐亚军       2008. 9. 12
set /p address=请输入源文件的路径:
set /p  srcType=请输入源文件类型（如rar、txt等):
set /p  destType=请输入修改后的文件类型：
set /p file=请输入文件名前缀:
set /p initNum=请输入初始号:
set /p increase=请输入依次递增的公差:

if not "%address:~-1%" == "\"  set address=%address%\

set srcType=.%srcType%
set destType=.%destType%
 
set total=0
set time1=%time%
set hh1=%time1:~0,2% 
set mm1=%time1:~3,2% 
set ss1=%time1:~6,2% 
set ms1=%time1:~9,2%

for /r  %%i in (%address%*%srcType%) do (set /a total+=1)

set /a num=%initNum%+(%total%-1)*%increase%

::==============================================================
for /r  %%i in (%address%*%srcType%)  do (
for /l %%j in (%initNum%,%increase%,%num%) do (
if  not exist  %address%%file%%%j%destType%  ren   "%%i" "%file%%%j%destType%" 
)
)
::==============================================================

set time2=%time%
set hh2=%time2:~0,2% 
set mm2=%time2:~3,2% 
set ss2=%time2:~6,2% 
set ms2=%time2:~9,2%

goto formatTime
:afterFormat
goto calcTime

:result
cls
 echo 重命名文件%total%个,  修改日期：%date% 
echo 修改时间：%time1%--%time2%,共用时%h%:%m%:%s%.%ms%s
pause
exit


:formatTime
::====================================
if %hh1% LSS 10 goto formathh1
if "%hh1:~0,1%" == "0" set hh1=%hh1:~1%
:formathh1
if "%mm1:~0,1%" == "0" set mm1=%mm1:~1%
if "%ss1:~0,1%" == "0" set ss1=%ss1:~1%
if "%ms1:~0,1%" == "0" set ms1=%ms1:~1%

if %hh2% LSS 10 goto formathh2
if "%hh2:~0,1%" == "0" set hh2=%hh2:~1%
:formathh2
if "%mm2:~0,1%" == "0" set mm2=%mm2:~1%
if "%ss2:~0,1%" == "0" set ss2=%ss2:~1%
if "%ms2:~0,1%" == "0" set ms2=%ms2:~1%
goto afterFormat


:calcTime
::===================================
if %ms2%  LSS %ms1% goto MsecondLess
:continueMs
set /a ms=%ms2%-%ms1%
if %ms% LSS 10 set ms=0%ms%

if %ss2%  LSS %ss1% goto secondLess
:continueM
set /a s=%ss2%-%ss1%

if %mm2%  LSS %mm1% goto minuteLess
:continueH
set /a m=%mm2%-%mm1%

if %hh2%  LSS %hh1% set /a hh2=%hh2%+24
set /a h=%hh2%-%hh1%

goto result

:MsecondLess
set /a ms2=%ms2%+100
set /a ss2=%ss2%-1
goto continueMs

:secondLess
set /a ss2=%ss2%+60
set /a mm2=%mm2%-1
goto continueM

:minuteLess
set /a mm2=%mm2%+60
set /a hh2=%hh2%-1
goto continueH








