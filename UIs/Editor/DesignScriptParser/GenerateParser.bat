copy/b atg\Start.atg+atg\Associative.atg+atg\Imperative.atg+atg\End.atg DS.atg
Coco.exe -namespace DesignScript.Parser DS.atg
copy DS.atg DS.atg.temp
del DS.atg
pause