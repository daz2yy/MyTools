@echo off
@echo. 
@echo compile Lua_MangoEngine pkg files...
@echo. 

compile_luabinding -d ../CppToLua/ -o Lua_ME_Box2D ../luato++/Box2D/ME_Box2D.pkg

@echo success...
pause
