@ECHO OFF

SET "release_path=E:/Com2VERSE/graphic/Unity_work/Com2VERSE_Release/Com2VERSE_Release_20220913"
SET "dst_path=E:/c2vclient_renderfeatures/Packages/RenderFeatures"
ECHO "%release_path%/Packages/RenderFeatures"
MKLINK /J "%dst_path%" "%release_path%/Packages/RenderFeatures"

PAUSE