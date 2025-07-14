fx_version 'cerulean'  -- or bodacious, but cerulean is recommended for NUI

game 'gta5'

-- .NET scripts
client_script 'Client/bin/Release/net452/publish/*.net.dll'
--server_script 'Server/bin/Release/net452/publish/*.net.dll'

-- NUI UI
ui_page 'html/index.html'

files {
  'html/index.html',
  'html/style.css',
  'html/app.js',
  'html/images/*.webp',
  'Newtonsoft.Json.dll'
}


author 'code-vine'
version '0.0.1'
description 'Vehicle Menu with NUI and C#'
