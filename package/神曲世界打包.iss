; �ű��� Inno Setup �ű��� ���ɣ�
; �йش��� Inno Setup �ű��ļ�����ϸ��������İ����ĵ���

[Setup]
; ע��: AppId ֵ����Ψһʶ���Ӧ�ó���
; ��ֹ������Ӧ�ó���İ�װ��ʹ����ͬ�� AppId ֵ��
; (��Ҫ����һ���µ� GUID����ѡ�񡰹��� | ���� GUID����)
AppId={{F43D986F-DABC-424F-922D-2A1081AED33C}
AppName=����������
AppVerName=���������� 1.1.0
AppPublisher=Daz2yy
DefaultDirName={pf}\����������
DefaultGroupName=����������
OutputBaseFilename=����������v1.1.0
SetupIconFile=E:\MyTools\DragAndRun\DragAndRun\bin\Release\icon.ico
OutputDir=H:\����\����������
Compression=lzma
SolidCompression=yes

[Languages]
Name: "chinese"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "E:\MyTools\DragAndRun\DragAndRun\bin\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs promptifolder
; ע��: ��Ҫ���κι���ϵͳ�ļ���ʹ�á�Flags: ignoreversion��

[Icons]
Name: "{group}\����������"; Filename: "{app}\����������.exe" ; IconFilename: "{app}\icon.ico"
Name: "{group}\{cm:UninstallProgram,����������}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\����������"; Filename: "{app}\����������.exe"; Tasks: desktopicon  ; IconFilename: "{app}\icon.ico"
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\����������"; Filename: "{app}\����������.exe"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\����������.exe"; Description: "{cm:LaunchProgram,����������}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[InstallDelete]
Type: filesandordirs; Name: "{app}\temp"
Type: filesandordirs; Name: "{app}\unins000.dat"
Type: filesandordirs; Name: "{app}\unins000.exe"
