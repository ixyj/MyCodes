@echo off
::清除最近打开的文件列表
@REG Delete HKCU\Software\Microsoft\VisualStudio\8.0\FileMRUList /va /f
::清除最近打开的项目列表
@REG Delete HKCU\Software\Microsoft\VisualStudio\8.0\ProjectMRUList /va /f
