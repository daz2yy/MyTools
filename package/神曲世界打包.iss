; 脚本由 Inno Setup 脚本向导 生成！
; 有关创建 Inno Setup 脚本文件的详细资料请查阅帮助文档！

[Setup]
; 注意: AppId 值用于唯一识别该应用程序。
; 禁止对其他应用程序的安装器使用相同的 AppId 值！
; (若要生成一个新的 GUID，请选择“工具 | 生成 GUID”。)
AppId={{F43D986F-DABC-424F-922D-2A1081AED33C}
AppName=神曲世界打包
AppVerName=神曲世界打包 1.1.0
AppPublisher=Daz2yy
DefaultDirName={pf}\神曲世界打包
DefaultGroupName=神曲世界打包
OutputBaseFilename=神曲世界打包v1.1.0
SetupIconFile=E:\MyTools\DragAndRun\DragAndRun\bin\Release\icon.ico
OutputDir=H:\发布\神曲世界打包
Compression=lzma
SolidCompression=yes

[Languages]
Name: "chinese"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "E:\MyTools\DragAndRun\DragAndRun\bin\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs promptifolder
; 注意: 不要在任何共享系统文件上使用“Flags: ignoreversion”

[Icons]
Name: "{group}\神曲世界打包"; Filename: "{app}\神曲世界打包.exe" ; IconFilename: "{app}\icon.ico"
Name: "{group}\{cm:UninstallProgram,神曲世界打包}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\神曲世界打包"; Filename: "{app}\神曲世界打包.exe"; Tasks: desktopicon  ; IconFilename: "{app}\icon.ico"
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\神曲世界打包"; Filename: "{app}\神曲世界打包.exe"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\神曲世界打包.exe"; Description: "{cm:LaunchProgram,神曲世界打包}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[InstallDelete]
Type: filesandordirs; Name: "{app}\temp"
Type: filesandordirs; Name: "{app}\unins000.dat"
Type: filesandordirs; Name: "{app}\unins000.exe"
