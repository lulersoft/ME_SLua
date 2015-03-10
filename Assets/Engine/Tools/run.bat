@echo off 
set luacPath=%1
set encodePath=%2
for /R %encodePath% %%s in (*) do ( 
echo encode %%s complete
%luacPath% -o %%s %%s
) 
pause