@echo off
::�������򿪵��ļ��б�
@REG Delete HKCU\Software\Microsoft\VisualStudio\8.0\FileMRUList /va /f
::�������򿪵���Ŀ�б�
@REG Delete HKCU\Software\Microsoft\VisualStudio\8.0\ProjectMRUList /va /f
