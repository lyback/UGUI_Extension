@echo on

rem SET TexturePacker="C:/Program Files/TexturePacker/TexturePacker.exe"
SET TexturePacker="./Tools/TexturePacker/TexturePacker_Win32/bin/TexturePacker.exe"

SET PATH_SRC=%~1
SET PATH_DST=%~2
SET ARGS=%~3

SET DATA_FILE=%PATH_DST%.tpsheet
SET SHEET_FILE=%PATH_DST%.png

rem %TexturePacker% --smart-update %PATH_SRC% --data %DATA_FILE% --format unity-texture2d --sheet %SHEET_FILE% --max-size 2048 --force-squared --size-constraints POT --disable-rotation --trim-mode %TRIN_MODE% --algorithm Polygon --trim-margin 0 --extrude 1 --border-padding 0 --shape-padding %SHAPE_PADDING%
%TexturePacker% --smart-update %PATH_SRC% --data %DATA_FILE% --format unity-texture2d --sheet %SHEET_FILE% %ARGS%

