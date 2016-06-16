::encode utf-8
chcp 65001


set now=%date:~10,4%-%date:~4,2%-%date:~7,2%
set log=log.txt

:begin
cls

echo "==========begin============" > %log%

:::::::::::::::::::::::::::::::::::::::::::::::::::::::::
if exist History\TH2_QF_TOPHIT_CCR.%now%.tsv goto finish

for %%i in ("C:\Users\yajxu\Downloads\[Threshold QF] Suggestion Group Metrics_[Win10] Top Hit CCR*.tsv","C:\Users\yajxu\Downloads\[Threshold QF] Suggestion Group Metrics_[Win10] Top Hit Coverage*.tsv","C:\Users\yajxu\Downloads\[Threshold QF] Overall Click Metrics_Total CVIDs*.tsv") do del /q "%%i"

set var=3
:repeatDownload
::downloading
if not exist "C:\Users\yajxu\Downloads\[Threshold QF] Suggestion Group Metrics_[Win10] Top Hit CCR.tsv" (
start "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" "http://bingdat26.redmond.corp.microsoft.com/WebForms/History/Download.ashx?j=1470&b=1&s=1&N=30&from=&to=&z=XP--1±AllMarkets²zh-CN°-2±TH2°-3±AllImpressions²Apps²Cat3A²Command²DirectNav²SearchTheWeb²Settings²Web"
)

if not exist "C:\Users\yajxu\Downloads\[Threshold QF] Suggestion Group Metrics_[Win10] Top Hit Coverage.tsv" (
start "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" "http://bingdat26.redmond.corp.microsoft.com/WebForms/History/Download.ashx?j=1469&b=1&s=1&N=1&from=&to=&z=XP--1±zh-CN°-2±TH2°-3±Apps²Cat1²Cat3A²Command²DirectNav²Documents²Files²Folders²Music²PathCompletion²Photos²SearchMyStuff²SearchTheWeb²Settings²Store²Videos²Web"
)

if not exist "C:\Users\yajxu\Downloads\[Threshold QF] Overall Click Metrics_Total CVIDs.tsv" (
start "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" "http://bingdat26.redmond.corp.microsoft.com/WebForms/History/Download.ashx?j=1462&b=1&s=1&N=30&from=&to=&z=XP--1±zh-CN°-2±Windows 10°-3±TH1²TH2"
)

choice /t 60 /d y /n
set /a var-=1
if %var% gtr 1 goto repeatDownload

tskill chrome

if not exist "C:\Users\yajxu\Downloads\[Threshold QF] Suggestion Group Metrics_[Win10] Top Hit CCR.tsv" (
echo "Failed to download data" >> %log%
goto finish
)
if not exist "C:\Users\yajxu\Downloads\[Threshold QF] Suggestion Group Metrics_[Win10] Top Hit Coverage.tsv" (
echo "Failed to download data" >> %log%
goto finish
)
if not exist "C:\Users\yajxu\Downloads\[Threshold QF] Overall Click Metrics_Total CVIDs.tsv"  (
echo "Failed to download data" >> %log%
goto finish
)

:existDownloading

copy /y "C:\Users\yajxu\Downloads\[Threshold QF] Suggestion Group Metrics_[Win10] Top Hit CCR.tsv" Latest\TH2_QF_TOPHIT_CCR.csv
copy /y "C:\Users\yajxu\Downloads\[Threshold QF] Suggestion Group Metrics_[Win10] Top Hit CCR.tsv" History\TH2_QF_TOPHIT_CCR.%now%.tsv
copy /y "C:\Users\yajxu\Downloads\[Threshold QF] Suggestion Group Metrics_[Win10] Top Hit Coverage.tsv" Latest\TH2_QF_TOPHIT_Coverge.csv
copy /y "C:\Users\yajxu\Downloads\[Threshold QF] Suggestion Group Metrics_[Win10] Top Hit Coverage.tsv" History\TH2_QF_TOPHIT_Coverage.%now%.tsv
copy /y "C:\Users\yajxu\Downloads\[Threshold QF] Overall Click Metrics_Total CVIDs.tsv" Latest\TH2_QF_TOPHIT_Traffic.csv
copy /y "C:\Users\yajxu\Downloads\[Threshold QF] Overall Click Metrics_Total CVIDs.tsv" History\TH2_QF_TOPHIT_Traffic.%now%.tsv

echo "==========Success============" >> %log%

:finish

::sleep
for /l %%i in (1,1,2) do choice /t 9999 /d y /n

goto begin