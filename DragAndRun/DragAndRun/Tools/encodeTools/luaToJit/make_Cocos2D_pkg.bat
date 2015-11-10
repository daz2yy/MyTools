@echo off
@echo. 
@echo compile Lua_MangoEngine pkg files...
@echo. 

compile_luabinding -d ../../../OpenSource/cocos2d-x/cocos2d-x/scripting/lua/cocos2dx_support/ -o LuaCocos2d ../luato++/Cocos2D/Cocos2d.pkg

@echo compile Lua_MangoEngine success...
pause
