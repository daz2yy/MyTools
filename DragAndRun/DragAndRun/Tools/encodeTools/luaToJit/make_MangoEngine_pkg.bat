@echo off
@echo. 
@echo compile Lua_MangoEngine pkg files...
@echo. 

compile_luabinding -d ../CppToLua/ -o Lua_MangoEngine ../luato++/MangoEngine.pkg

@echo compile Lua_MangoEngine success...
pause
